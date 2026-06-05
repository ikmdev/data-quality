package dev.ikm.dq.cli.ingest;

import java.nio.file.Path;

public record IngestContext(
		Path duckDBScript,
		Path sourceFile,
		long runId,
		String sourceId,
		String providerId) {
}
