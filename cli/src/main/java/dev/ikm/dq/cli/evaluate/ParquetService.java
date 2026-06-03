package dev.ikm.dq.cli.evaluate;

import org.springframework.stereotype.Service;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;

@Service
public class ParquetService implements AutoCloseable {

	private Connection connection;
	private PreparedStatement insertStmt;
	private Path parquetOutputPath;



	public void init(Path parquetOutputPath) throws SQLException {
		this.parquetOutputPath = parquetOutputPath;
		Path dbFile = Paths.get(System.getProperty("user.dir"), "target").resolve("evaluation_results.duckdb");

		if (dbFile.toFile().exists()) {
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
	}

	/**
	 * Flush batch to DuckDB and export to parquet file.
	 */
	public void exportToParquet() throws SQLException {
		insertStmt.executeBatch();
		connection.commit();

		try (Statement st = connection.createStatement()) {
			String path = parquetOutputPath.toAbsolutePath().toString();
			st.execute("COPY evaluation_results TO '" + path + "' (FORMAT PARQUET)");
		}
	}

	@Override
	public void close() throws Exception {
		if (insertStmt != null) insertStmt.close();
		if (connection != null) connection.close();
	}
}
