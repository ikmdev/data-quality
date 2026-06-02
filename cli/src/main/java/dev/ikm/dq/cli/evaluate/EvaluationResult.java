package dev.ikm.dq.cli.evaluate;

public record EvaluationResult(
    Long runId,
    String messageId,
    String dataClass,
    String attributeName,
    String attributeValue,
    String assessment,
    String status,
    String reason,
    String effect
) {}