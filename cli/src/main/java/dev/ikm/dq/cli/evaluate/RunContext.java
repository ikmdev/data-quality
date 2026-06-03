package dev.ikm.dq.cli.evaluate;

public record RunContext(
		String runName,
		String rubricMnemonic,
		String modelMnemonic,
		String dataProviderId,
		String dataSourceId) {
}
