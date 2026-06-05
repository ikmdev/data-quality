package dev.ikm.dq.cli.ingest;

import org.apache.arrow.memory.BufferAllocator;
import org.apache.arrow.memory.RootAllocator;
import org.apache.arrow.vector.VectorSchemaRoot;
import org.apache.arrow.vector.ipc.ArrowReader;
import org.duckdb.DuckDBResultSet;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.function.Consumer;

@Service
public class IngestService {

	private static final Logger LOG = LoggerFactory.getLogger(IngestService.class);

	private final Path dbFile = Paths.get(System.getProperty("user.dir"), "target").resolve("evaluation_queue.duckdb");
	private final static long BATCH_SIZE = 8_192; // Adjust if needed (multiples of 2,048)

	public IngestSummary performDataIngestion(IngestContext ingestContext, Consumer<VectorSchemaRoot> dataConsumer) {
		long totalProcessed = 0;

		// 1. Memory allocator for Arrow (tied to lifecycle of connection)
		try (BufferAllocator allocator = new RootAllocator();
		     Statement statement = DriverManager.getConnection("jdbc:duckdb:").createStatement()) {

			// 3. Read and execute the entire transformation script
			String scriptContent = Files.readString(ingestContext.duckDBScript());

			// Replace placeholders safely from the outside
			scriptContent = scriptContent
					.replace("${SOURCE_FILE}", ingestContext.sourceFile().toAbsolutePath().toString())
					.replace("${SOURCE_ID}", ingestContext.sourceId())
					.replace("${PROVIDER_ID}", ingestContext.providerId());

			// 4. Query directly via stream, no table creation/insertion needed
			try (ResultSet rs = statement.executeQuery(scriptContent)) { // <-- PASS SCRIPT HERE

				DuckDBResultSet duckdbRs = (DuckDBResultSet) rs;

				try (ArrowReader reader = (ArrowReader) duckdbRs.arrowExportStream(allocator, BATCH_SIZE)) {
					while (reader.loadNextBatch()) {
						VectorSchemaRoot root = reader.getVectorSchemaRoot();
						dataConsumer.accept(root);
						totalProcessed += root.getRowCount();
					}
				}
			}
		} catch (Exception e) {
			throw new RuntimeException("DuckDB ingestion pipeline failed.", e);
		}

		// Return summary (assuming you want to add totalProcessed to it later)
		return new IngestSummary(totalProcessed);
	}
}