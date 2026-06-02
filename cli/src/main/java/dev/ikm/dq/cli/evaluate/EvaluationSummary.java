package dev.ikm.dq.cli.evaluate;

import java.time.Duration;
import java.time.Instant;

public record EvaluationSummary(
		String runName,
		long totalEvaluations,
		long totalValidEvaluations,
		long totalInvalidEvaluations,
		Duration evaluationDuration) {

	public double failureRate() {
		return totalEvaluations == 0 ? 0.0 : (double) totalInvalidEvaluations / totalEvaluations;
	}
}
