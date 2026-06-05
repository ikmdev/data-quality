package dev.ikm.dq.cli.ingest;

import java.nio.file.Path;

public record IngestContext(
		Path duckDBScript,
		Path sourceFile,
		String sourceId,
		String providerId) {
}
