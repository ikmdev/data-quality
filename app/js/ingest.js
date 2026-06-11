import * as duckdb from 'https://cdn.jsdelivr.net/npm/@duckdb/duckdb-wasm@1.28.1-dev106.0/+esm';

// Cache to prevent re-initializing DuckDB on every call
let dbInstance = null;

export async function initDuckDB() {
    if (dbInstance) return dbInstance;

    // 1. Configure the Web Worker and WASM binaries using JSDelivr bundles
    const JSDELIVR_BUNDLES = duckdb.getJsDelivrBundles();
    const bundle = await duckdb.selectBundle(JSDELIVR_BUNDLES);

    const worker_url = URL.createObjectURL(
        new Blob([`importScripts("${bundle.mainWorker}");`], {type: 'text/javascript'})
    );

    const worker = new Worker(worker_url);
    const logger = new duckdb.ConsoleLogger();

    dbInstance = new duckdb.AsyncDuckDB(logger, worker);
    await dbInstance.instantiate(bundle.mainModule, bundle.pthreadWorker);

    return dbInstance;
}

export async function run(tsvRows, sourceId, providerId) {
    const db = await initDuckDB();
    const conn = await db.connect();

    // Fetch the SQL template exactly ONCE before the loop
    let sqlTemplate = "";
    try {
        const sqlResponse = await fetch('./duckdb/piqi_lab_data_ingestion.sql');
        if (!sqlResponse.ok) throw new Error("Could not find piqi_lab_data_ingestion.sql file");
        sqlTemplate = await sqlResponse.text();
    } catch (e) {
        console.error("Failed to load SQL file", e);
        await conn.close();
        throw e;
    }

    const allResults = [];
    const fileName = 'input_row.tsv';

    const readCsvFlags = "all_varchar = true, header = true, sep = '\t'";

    // Apply the replacements to the string template globally
    const finalSql = sqlTemplate
        .replace(/\$\{SOURCE_FILE\}/g, fileName)
        .replace(/\$\{SOURCE_ID\}/g, sourceId)
        .replace(/\$\{PROVIDER_ID\}/g, providerId);

    // DuckDB WASM can process a giant block of TSV text directly.
    // Instead of looping, map all rows into one TSV blob so DuckDB does it vectorized!
    const header = "Unique_ID\tLongAccessionNumberUID\tLabChemTestSID\tLabChemTestName\tLabChemTestUrgencySID\tUrgency\tLabChemResultValue\tLabChemResultNumericValue\tTopographySID\tTopography\tAccessionInstitutionSID\tAccessioningInstitution\tOrderingInstitutionSID\tOrderingInstutionName\tCollectingInstitutionSID\tCollectingInstitutionName\tLOINCSID\tLOINC\tUnits\tAbnormal\tRefHigh\tRefLow\tDevice ID";

    // Split the header strictly by tab to count the exact number of EXPECTED columns
    const headerColumns = header.split('\t').length;
    // Fix truncated rows by padding them with missing tabs!
    const paddedRows = tsvRows.map(row => {
        // Split row by tab, ignoring \r if present
        const cols = row.replace(/\r/g, '').split('\t');

        let paddedRow = row;
        // If the row string has fewer columns than the header expects, dynamically tack on empty tabs
        if (cols.length < headerColumns) {
            const missingTabs = headerColumns - cols.length;
            paddedRow += '\t'.repeat(missingTabs);
        }
        return paddedRow;
    });

    // Combine all inputs into a single text block
    const isHeaderMissing = !paddedRows[0].startsWith("Unique_ID");
    const fullTsvText = (isHeaderMissing ? header + "\n" : "") + paddedRows.join('\n');

    // Register single virtual file
    await db.registerFileText(fileName, fullTsvText);

    try {
        // Run the globally replaced SQL script exactly once!
        const result = await conn.query(finalSql);

        // Convert the Arrow block back to JS
        result.toArray().forEach(row => {
            if (row.payload_json) {
                allResults.push(JSON.parse(row.payload_json));
            }
        });
    } catch (error) {
        console.error("DuckDB execution error:", error);
        throw error;
    } finally {
        // Clean up WASM resources
        await db.dropFile(fileName);
        await conn.close();
    }

    // Return the clean array to the caller
    return allResults;
}