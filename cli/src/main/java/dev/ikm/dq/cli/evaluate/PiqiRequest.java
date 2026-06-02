package dev.ikm.dq.cli.evaluate;

public record PiqiRequest(
		String dataProviderID,
		String dataSourceID,
		String messageID,
		String piqiModelMnemonic,
		String evaluationRubricMnemonic,
		String messageData) {
}
