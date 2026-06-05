package dev.ikm.dq.cli.ingest;

import org.apache.arrow.vector.FieldVector;
import org.apache.arrow.vector.types.pojo.Field;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.BufferedWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.SQLException;
import java.util.List;
import java.util.UUID;

@Component
@CommandLine.Command(name = "ingest", description = "Ingest data into PIQI model")
public class IngestCommand implements Runnable {

	private static final Logger LOG = LoggerFactory.getLogger(IngestCommand.class);

	private final IngestService ingestService;

	@Autowired
	public IngestCommand(IngestService ingestService) {
		this.ingestService = ingestService;
	}

	@CommandLine.Option(
			names = {"--source-data-file"},
			description = "Source data to be ingested (e.g., CSV, JSON, Parquet)",
			defaultValue = "",
			required = true)
	private Path source;

	@CommandLine.Option(
			names = {"--ingestion-script"},
			description = "Ingest logic duckDB script file",
			defaultValue = "",
			required = true)
	private Path script;

	@CommandLine.Option(
			names = {"--output-text-file"},
			description = "Optional output file for ingested data in JSON format (for debugging/verification)",
			defaultValue = "")
	private Path outputTextFile;

	@CommandLine.Option(
			names = {"--data-provider-id"},
			description = "Identifier for the data provider (e.g., source system or team)",
			defaultValue = "")
	private String dataProviderId;

	@CommandLine.Option(
			names = {"--data-source-id"},
			description = "Identifier for the data source (e.g., database, table, or file)",
			defaultValue = "")
	private String dataSourceId;

	@CommandLine.Option(
			names = {"--override"},
			description = "Override existing output file if it already exists",
			defaultValue = "false")
	private boolean overrideOutput;

	@Override
	public void run() {
		LOG.info("Starting data ingestion from source: {}", source);
		LOG.info("Using ingest logic script: {}", script);

		// Make sure output directory exists
		if (outputTextFile.toFile().exists()) {
			if (!overrideOutput) {
				throw new RuntimeException("Output file already exists: " + outputTextFile);
			} else {
				try {
					Files.delete(outputTextFile);
				} catch (IOException e) {
					throw new RuntimeException(e);
				}
			}
		}

		IngestContext ingestContext = new IngestContext(
				script,
				source,
				0,
				dataSourceId.isEmpty() ? UUID.randomUUID().toString() : dataSourceId,
				dataProviderId.isEmpty() ? UUID.randomUUID().toString() : dataProviderId);

		try (BufferedWriter writer = outputTextFile != null ? Files.newBufferedWriter(outputTextFile) : null) {

			ingestService.performDataIngestion(ingestContext, vectorSchemaRoot -> {
				LOG.info("Received batch with schema: {}", vectorSchemaRoot.getSchema());
				LOG.info("Batch row count: {}", vectorSchemaRoot.getRowCount());

				if (writer != null) {
					try {
						List<Field> fields = vectorSchemaRoot.getSchema().getFields();

						for (int row = 0; row < vectorSchemaRoot.getRowCount(); row++) {
							writer.write("----- Record " + (row + 1) + " -----");
							writer.newLine();

							for (Field field : fields) {
								FieldVector vector = vectorSchemaRoot.getVector(field.getName());
								Object value = vector.getObject(row);

								// For JSON payloads, formatting might look better without quotes
								// You could also pretty-print the JSON here if you add Jackson/Gson
								writer.write(String.format("  %-15s : %s", field.getName(), value));
								writer.newLine();
							}
							writer.newLine(); // Add blank line between records
						}
					} catch (IOException e) {
						throw new RuntimeException("Failed to write batch to file", e);
					}
				} else {
					// No output file — just log
					List<Field> fields = vectorSchemaRoot.getSchema().getFields();
					for (int row = 0; row < vectorSchemaRoot.getRowCount(); row++) {
						for (Field field : fields) {
							FieldVector vector = vectorSchemaRoot.getVector(field.getName());
							LOG.debug("{} = {}", field.getName(), vector.getObject(row));
						}
					}
				}
			});

			LOG.info("Data ingestion completed successfully.");
		} catch (Exception e) {
			LOG.error("Data ingestion failed: {}", e.getMessage(), e);
		}
	}
}
