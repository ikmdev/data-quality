package dev.ikm.dq.cli.evaluate;

public record PiqiContext(
		String runName,
		String rubricMnemonic,
		String modelMnemonic,
		String dataProviderId,
		String dataSourceId) {
}
