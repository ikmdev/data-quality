package dev.ikm.dq.cli.evaluate;

import java.time.Duration;

public record EvaluationSummary(
		String runName,
		long totalEvaluations,
		long totalValidEvaluations,
		long totalInvalidEvaluations,
		Duration evaluationDuration) {

	public double failureRate() {
		return totalEvaluations == 0 ? 0.0 : (double) totalInvalidEvaluations / totalEvaluations;
	}

	public EvaluationSummary merge(EvaluationSummary other) {
		return new EvaluationSummary(
				runName,
				this.totalEvaluations + other.totalEvaluations,
				this.totalValidEvaluations + other.totalValidEvaluations,
				this.totalInvalidEvaluations + other.totalInvalidEvaluations,
				this.evaluationDuration.plus(other.evaluationDuration));
	}
}
