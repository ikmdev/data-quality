package dev.ikm.dq.cli.parquet;

import dev.ikm.dq.cli.evaluate.EvaluationResult;
import org.duckdb.DuckDBAppender;
import org.duckdb.DuckDBConnection;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;

@Service
public class DatabaseService implements AutoCloseable {

	private static final Logger LOG = LoggerFactory.getLogger(DatabaseService.class);

	private Connection connection;
	private DuckDBAppender appender;

	@Value("${db.name}")
	String pgDbName;

	@Value("${db.user}")
	String pgUser;

	@Value("${db.password}")
	String pgPassword;

	@Value("${db.host}")
	String pgHost;

	@Value("${db.port}")
	int pgPort;


	public void init() throws SQLException {

		// 1. Initialize IN-MEMORY DuckDB instance (No massive lock files needed on disk)
		this.connection = DriverManager.getConnection("jdbc:duckdb:");

		// 2. We still create a table definition because the Appender needs a schema to latch onto
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
			LOG.debug("Created schema 'evaluation_results' in in-memory DuckDB.");


			// Step 3: Install and Load the Postgres extension
			st.execute("INSTALL postgres;");
			st.execute("LOAD postgres;");

			// Step 4: Attach the Postgres database!
			// (duckdb will name the remote connection 'pg_db')
			String attachCommand = String.format(
					"ATTACH 'dbname=%s user=%s password=%s host=%s port=%s' AS pg_db (TYPE POSTGRES);",
					pgDbName, pgUser, pgPassword, pgHost, pgPort
			);
			st.execute(attachCommand);

			LOG.info("Successfully attached DuckDB to remote Postgres database: {}", pgDbName);
		}

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

	public void insertIntoPostgres() throws SQLException {
		// 1. Close the appender — this forces any remaining C++ memory buffers to flush to the table
		if (appender != null) {
			LOG.debug("Flushing DuckDB Appender buffers...");
			appender.close();
			appender = null;
		}

		// 2. Export the highly optimized table out to the Parquet file
		LOG.info("Inserting database records to Postgres piqi_evaluation_results table");
		try (Statement st = connection.createStatement()) {
			st.execute("""
					    INSERT INTO pg_db.public.piqi_evaluation_results (
					        run_id, message_id, data_class, attribute_name, attribute_value, assessment, status, reason, effect
					    )
					    SELECT * FROM evaluation_results;
					""");
		}
		LOG.info("Successfully inserted data into Postgres.");
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
