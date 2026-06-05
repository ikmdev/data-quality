package dev.ikm.dq.cli.parquet;

import dev.ikm.dq.cli.evaluate.EvaluationResult;
import org.duckdb.DuckDBAppender;
import org.duckdb.DuckDBConnection;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;

@Service
public class ParquetService implements AutoCloseable {

	private static final Logger LOG = LoggerFactory.getLogger(ParquetService.class);

	private Connection connection;
	private DuckDBAppender appender;
	private Path parquetOutputPath;

	public void init(Path parquetOutputPath) throws SQLException {
		this.parquetOutputPath = parquetOutputPath;

		// 1. Delete output file if it somehow exists to prevent append conflicts initially
		if (Files.exists(parquetOutputPath)) {
			try {
				Files.delete(parquetOutputPath);
			} catch (Exception ignored) {
			}
		}

		// 2. Initialize IN-MEMORY DuckDB instance (No massive lock files needed on disk)
		this.connection = DriverManager.getConnection("jdbc:duckdb:");

		// 3. We still create a table definition because the Appender needs a schema to latch onto
		try (Statement st = connection.createStatement()) {
			st.execute("""
					CREATE TABLE evaluation_results (
					    run_id BIGINT,
					    message_id VARCHAR,
					    data_class VARCHAR,
					    attribute_name VARCHAR,
					    attribute_value VARCHAR,
					    assessment VARCHAR,
					    status VARCHAR,
					    reason VARCHAR,
					    effect VARCHAR
					)
					""");
		}
		LOG.debug("Created schema 'evaluation_results' in in-memory DuckDB.");

		// 4. Unwrap standard JDBC connection to unlock DuckDB-specific features
		DuckDBConnection duckDBConn = (DuckDBConnection) connection;

		// 5. Create the high-speed vectorized Appender bound to the target table
		this.appender = duckDBConn.createAppender("main", "evaluation_results");
	}

	public void append(List<EvaluationResult> evaluationResults) throws SQLException {
		// Appenders expect rows built sequentially as blocks in memory
		for (EvaluationResult result : evaluationResults) {
			appender.beginRow();
			appender.append(result.runId());
			appender.append(result.messageId());
			appender.append(result.dataClass());
			appender.append(result.attributeName());
			appender.append(result.attributeValue());
			appender.append(result.assessment());
			appender.append(result.status());
			appender.append(result.reason());
			appender.append(result.effect());
			appender.endRow();
		}
		// Notice: No manual batch tracking! The Appender internally manages memory
		// chunks and flushes when optimal, removing the need for JDBC_BATCH_SIZE counting.
	}

	/**
	 * Flush Appender memory buffers to the database, then export to parquet file.
	 */
	public void exportToParquet() throws SQLException {
		// 1. Close the appender — this forces any remaining C++ memory buffers to flush to the table
		if (appender != null) {
			LOG.debug("Flushing DuckDB Appender buffers...");
			appender.close();
			appender = null;
		}

		// 2. Export the highly optimized table out to the Parquet file
		String path = parquetOutputPath.toAbsolutePath().toString();
		LOG.info("Exporting database records to Parquet file: {}", path);
		try (Statement st = connection.createStatement()) {
			st.execute("COPY evaluation_results TO '" + path + "' (FORMAT PARQUET)");
		}
		LOG.info("Successfully exported data to Parquet.");
	}

	@Override
	public void close() throws Exception {
		LOG.debug("Closing ParquetService resources...");
		if (appender != null) {
			try {
				appender.close();
			} catch (Exception ignored) {
			}
		}
		if (connection != null) {
			try {
				connection.close();
			} catch (Exception ignored) {
			}
		}
		LOG.info("ParquetService closed successfully.");
	}
}
