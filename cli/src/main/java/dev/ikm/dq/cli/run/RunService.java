package dev.ikm.dq.cli.run;

import dev.ikm.dq.cli.evaluate.EvaluationSummary;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import javax.sql.DataSource;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.time.Instant;

@Service
public class RunService {

	Logger LOG = LoggerFactory.getLogger(RunService.class);

	private final DataSource dataSource;

	@Autowired
	public RunService(DataSource dataSource) {
		this.dataSource = dataSource;
	}

	public RunContext createRun(String name, String modelMnemonic, String rubricMnemonic) throws SQLException {
		String sql = """
				INSERT INTO public.piqi_evaluation_run
				(run_name, piqi_model_mnemonic, evaluation_rubric_mnemonic, status)
				VALUES (?, ?, ?, 'IN_PROGRESS')
				""";

		try (Connection conn = dataSource.getConnection();
		     PreparedStatement pstmt = conn.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {

			pstmt.setString(1, name);
			pstmt.setString(2, modelMnemonic);
			pstmt.setString(3, rubricMnemonic);

			int affectedRows = pstmt.executeUpdate();

			if (affectedRows == 0) {
				throw new SQLException("Creating run failed, no rows affected.");
			}

			try (ResultSet generatedKeys = pstmt.getGeneratedKeys()) {
				if (generatedKeys.next()) {
					LOG.info("Created new evaluation run in postgres");
					return new RunContext(
							generatedKeys.getLong(1),
							name,
							rubricMnemonic,
							modelMnemonic,
							"",
							""
					);
				} else {
					throw new SQLException("Creating run failed, no ID obtained.");
				}
			}
		}
	}

	/**
	 * Updates an existing evaluation run record to mark it as complete.     *     * @param runId   The ID of the run to update.     * @param summary The final summary of the run.     * @throws SQLException if a database access error occurs.
	 */
	public void completeRun(RunContext runContext, EvaluationSummary summary) throws SQLException {
		String sql = """
				UPDATE public.piqi_evaluation_run
				SET status = 'COMPLETED',
				    total_evaluations = ?,
				    total_completed = ?,
				    total_failed = ?,
				    completed_at = ?
				WHERE id = ?
				""";

		try (Connection conn = dataSource.getConnection();
		     PreparedStatement pstmt = conn.prepareStatement(sql)) {

			pstmt.setLong(1, summary.totalEvaluations());
			pstmt.setLong(2, summary.totalValidEvaluations());
			pstmt.setLong(3, summary.totalInvalidEvaluations());
			pstmt.setTimestamp(4, Timestamp.from(Instant.now()));
			pstmt.setLong(5, runContext.runId());

			pstmt.executeUpdate();
		} catch (SQLException e) {
			throw new RuntimeException(e);
		}

		LOG.info("Updated evaluation run status in postgres to finished.");
	}

	public void failRun(RunContext runContext) {
		String sql = """
				UPDATE public.piqi_evaluation_run
				SET status = 'FAILED',
				    completed_at = ?
				WHERE id = ?
				""";

		try (Connection conn = dataSource.getConnection();
			 PreparedStatement pstmt = conn.prepareStatement(sql)) {

			pstmt.setTimestamp(1, Timestamp.from(Instant.now()));
			pstmt.setLong(2, runContext.runId());

			pstmt.executeUpdate();
		} catch (SQLException e) {
			throw new RuntimeException(e);
		}

		LOG.info("Failed to update evaluation run status in postgres to failed.");
	}
}
