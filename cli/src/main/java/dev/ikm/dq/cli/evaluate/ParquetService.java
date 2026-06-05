package dev.ikm.dq.cli.evaluate;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.Arrays;
import java.util.List;

@Service
public class ParquetService implements AutoCloseable {

	private static final Logger LOG = LoggerFactory.getLogger(ParquetService.class);
	private final Path dbFile = Paths.get(System.getProperty("user.dir"), "target").resolve("evaluation_results.duckdb");
	private static final int JDBC_BATCH_SIZE = 50_000;

	private Connection connection;
	private PreparedStatement insertStmt;
	private Path parquetOutputPath;
	private int currentBatchSize = 0;

	public void init(Path parquetOutputPath) throws SQLException {
		this.parquetOutputPath = parquetOutputPath;

		LOG.info("Initializing evaluation_results DuckDB database at: {}", dbFile.toAbsolutePath());

		if (dbFile.toFile().exists()) {
			LOG.warn("Deleting existing evaluation_results database file.");
			dbFile.toFile().delete();
		}

		this.connection = DriverManager.getConnection("jdbc:duckdb:" + dbFile);
		connection.setAutoCommit(false);

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
		LOG.debug("Created 'evaluation_results' table in DuckDB.");

		this.insertStmt = connection.prepareStatement("""
				INSERT INTO evaluation_results VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
				""");
	}

	public void append(List<EvaluationResult> evaluationResults) throws SQLException {
		for (EvaluationResult result : evaluationResults) {
			append(result);
		}
	}

	public void append(EvaluationResult result) throws SQLException {
		insertStmt.setObject(1, result.runId(), java.sql.Types.BIGINT);
		insertStmt.setString(2, result.messageId());
		insertStmt.setString(3, result.dataClass());
		insertStmt.setString(4, result.attributeName());
		insertStmt.setString(5, result.attributeValue());
		insertStmt.setString(6, result.assessment());
		insertStmt.setString(7, result.status());
		insertStmt.setString(8, result.reason());
		insertStmt.setString(9, result.effect());
		insertStmt.addBatch();
		currentBatchSize++;

		// Execute the batch if it reaches the threshold ---
		if (currentBatchSize >= JDBC_BATCH_SIZE) {
			LOG.debug("JDBC batch size limit reached, executing batch...");
			int[] updateCounts = insertStmt.executeBatch();
			long totalRowsFlushed = Arrays.stream(updateCounts).asLongStream().sum();
			LOG.debug("Flushed {} rows to DuckDB in this batch.", totalRowsFlushed);
			currentBatchSize = 0; // Reset the counter
		}
	}

	/**
	 * Flush batch to DuckDB and export to parquet file.
	 */
	public void exportToParquet() throws SQLException {
		// Flush any remaining records in the last batch ---
		if (currentBatchSize > 0) {
			LOG.info("Flushing final batch of {} records to DuckDB...", currentBatchSize);
			int[] updateCounts = insertStmt.executeBatch();
			long totalRowsFlushed = Arrays.stream(updateCounts).asLongStream().sum();
			LOG.info("Flushed {} rows to the database.", totalRowsFlushed);
			currentBatchSize = 0;
		}

		connection.commit();
		LOG.debug("DuckDB transaction committed.");

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
		if (insertStmt != null) insertStmt.close();
		if (connection != null) connection.close();
		LOG.info("ParquetService closed successfully.");
	}
}
