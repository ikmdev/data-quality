package dev.ikm.dq.cli.pipeline;

import dev.ikm.dq.cli.evaluate.EvaluateService;
import dev.ikm.dq.cli.ingest.IngestService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.nio.file.Path;
import java.util.List;

@Component
@CommandLine.Command(name = "pipeline", description = "Run a data quality pipeline")
public class PipelineCommand implements Runnable {

	@CommandLine.Option(
			names = {"--run-name"},
			description = "Name of the run",
			required = true)
	private String runName;

	@CommandLine.Option(
			names = {"--rubric-mnemonic"},
			description = "Mnemonic of the rubric to use for evaluation",
			required = true)
	private String rubricMnemonic;

	@CommandLine.Option(
			names = {"--model-mnemonic"},
			description = "Mnemonic of the model to use for evaluation",
			required = true)
	private String modelMnemonic;

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
			names = {"--data"},
			description = "Input data file(s) to process",
			required = true)
	private List<Path> data;

	@CommandLine.Option(
			names = {"--output-parquet-file"},
			description = "Output parquet file containing all evaluation result data",
			required = true)
	private Path outputParquet;

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
			names = {"--override-parquet-file"},
			description = "Override existing output file if it already exists",
			defaultValue = "false")
	private boolean overrideOutput;

	private final IngestService ingestService;
	private final EvaluateService evaluateService;

	@Autowired
	public PipelineCommand(IngestService ingestService, EvaluateService evaluateService) {
		this.ingestService = ingestService;
		this.evaluateService = evaluateService;
	}

	@Override
	public void run() {

	}
}
