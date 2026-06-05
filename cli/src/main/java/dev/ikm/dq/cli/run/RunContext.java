package dev.ikm.dq.cli.run;

public record RunContext(
		long runId,
		String runName,
		String rubricMnemonic,
		String modelMnemonic,
		String dataProviderId,
		String dataSourceId) {

	public RunContext withDataProviderId(String dataProviderId) {
		return new RunContext(runId, runName, rubricMnemonic, modelMnemonic, dataProviderId, dataSourceId);
	}

	public RunContext withDataSourceId(String dataSourceId) {
		return new RunContext(runId, runName, rubricMnemonic, modelMnemonic, dataProviderId, dataSourceId);
	}

}
