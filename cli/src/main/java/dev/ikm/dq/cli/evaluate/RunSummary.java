package dev.ikm.dq.cli.evaluate;

import java.time.Duration;

public record RunSummary(
		String runName,
		long totalEvaluations,
		long totalValidEvaluations,
		long totalInvalidEvaluations,
		Duration evaluationDuration) {

	public double failureRate() {
		return totalEvaluations == 0 ? 0.0 : (double) totalInvalidEvaluations / totalEvaluations;
	}
}
