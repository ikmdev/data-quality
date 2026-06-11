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

export async function run(csvRows, sourceId, providerId) {
    const db = await initDuckDB();
    const conn = await db.connect();

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
    const fileName = 'input_row.csv';

    // NOTE: keep header names aligned with SQL, including "Device ID"
    const header = [
        "Unique_ID",
        "LongAccessionNumberUID",
        "LabChemTestSID",
        "LabChemTestName",
        "LabChemTestUrgencySID",
        "Urgency",
        "LabChemResultValue",
        "LabChemResultNumericValue",
        "TopographySID",
        "Topography",
        "AccessionInstitutionSID",
        "AccessioningInstitution",
        "OrderingInstitutionSID",
        "OrderingInstutionName",
        "CollectingInstitutionSID",
        "CollectingInstitutionName",
        "LOINCSID",
        "LOINC",
        "Units",
        "Abnormal",
        "RefHigh",
        "RefLow",
        "Device ID"
    ];

    function parseCsvLine(line) {
        const cells = [];
        let current = '';
        let inQuotes = false;

        for (let i = 0; i < line.length; i++) {
            const ch = line[i];

            if (ch === '"') {
                if (inQuotes && line[i + 1] === '"') {
                    current += '"';
                    i++;
                } else {
                    inQuotes = !inQuotes;
                }
            } else if (ch === ',' && !inQuotes) {
                cells.push(current);
                current = '';
            } else {
                current += ch;
            }
        }
        cells.push(current);
        return cells;
    }

    function toCsvCell(value) {
        const s = String(value ?? '');
        const escaped = s.replace(/"/g, '""');
        return /[",\n\r]/.test(s) ? `"${escaped}"` : escaped;
    }

    function toCsvLine(cells) {
        return cells.map(toCsvCell).join(',');
    }

    const expectedColumns = header.length;

    // Ensure each row has the same number of columns as header
    const paddedRows = csvRows.map((row) => {
        const cols = parseCsvLine(row.replace(/\r/g, ''));
        if (cols.length < expectedColumns) {
            while (cols.length < expectedColumns) cols.push('');
        } else if (cols.length > expectedColumns) {
            cols.length = expectedColumns;
        }
        return toCsvLine(cols);
    });

    // Include header if caller did not provide one
    const firstRowCols = paddedRows.length ? parseCsvLine(paddedRows[0]) : [];
    const firstCell = (firstRowCols[0] || '').trim().replace(/^"|"$/g, '');
    const isHeaderMissing = firstCell !== "Unique_ID";

    const headerLine = toCsvLine(header);
    const fullCsvText = (isHeaderMissing ? headerLine + "\n" : "") + paddedRows.join('\n');

    // Apply placeholders in SQL template
    const finalSql = sqlTemplate
        .replace(/\$\{SOURCE_FILE\}/g, fileName)
        .replace(/\$\{SOURCE_ID\}/g, sourceId)
        .replace(/\$\{PROVIDER_ID\}/g, providerId);

    await db.registerFileText(fileName, fullCsvText);

    try {
        const result = await conn.query(finalSql);
        result.toArray().forEach(row => {
            if (row.payload_json) {
                allResults.push(JSON.parse(row.payload_json));
            }
        });
    } catch (error) {
        console.error("DuckDB execution error:", error);
        throw error;
    } finally {
        await db.dropFile(fileName);
        await conn.close();
    }

    return allResults;
}