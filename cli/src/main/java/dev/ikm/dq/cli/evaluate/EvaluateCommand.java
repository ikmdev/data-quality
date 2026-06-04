package dev.ikm.dq.cli.evaluate;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.UUID;

@Component
@CommandLine.Command(name = "evaluate", description = "Evaluate data quality rules")
public class EvaluateCommand implements Runnable {

	private static final Logger LOG = LoggerFactory.getLogger(EvaluateCommand.class);

	private final EvaluateService evaluateService;

	@CommandLine.Option(
			names = {"-n", "--run-name"},
			description = "Name of the run",
			required = true)
	private String runName;

	@CommandLine.Option(
			names = {"-r", "--rubric-mnemonic"},
			description = "Mnemonic of the rubric to use for evaluation",
			required = true)
	private String rubricMnemonic;

	@CommandLine.Option(
			names = {"-m", "--model-mnemonic"},
			description = "Mnemonic of the model to use for evaluation",
			required = true)
	private String modelMnemonic;

	@CommandLine.Option(
			names = {"-p", "--data-provider-id"},
			description = "Identifier for the data provider (e.g., source system or team)",
			defaultValue = "")
	private String dataProviderId;

	@CommandLine.Option(
			names = {"-s", "--data-source-id"},
			description = "Identifier for the data source (e.g., database, table, or file)",
			defaultValue = "")
	private String dataSourceId;

	@CommandLine.Option(
			names = {"-d", "--data"},
			description = "Input data file(s) to process",
			required = true)
	private List<Path> data;

	@CommandLine.Option(
			names = {"-o", "--output-parquet-file"},
			description = "Output parquet file containing all evaluation result data",
			required = true)
	private Path outputParquet;

	@CommandLine.Option(
			names = {"--override"},
			description = "Override existing output file if it already exists",
			defaultValue = "false")
	private boolean overrideOutput;



	@Autowired
	public EvaluateCommand(EvaluateService evaluateService) {
		this.evaluateService = evaluateService;
	}

	@Override
	public void run() {

		// Make sure that all data files exactly exist
		data.forEach(dataPath -> {
			if (!dataPath.toFile().exists()) {
				throw new RuntimeException("Data path does not exist: " + dataPath);
			}
		});

		// Make sure output directory exists
		if (outputParquet.toFile().exists()) {
			if (!overrideOutput) {
				throw new RuntimeException("Output file already exists: " + outputParquet);
			} else {
				try {
					Files.delete(outputParquet);
				} catch (IOException e) {
					throw new RuntimeException(e);
				}
			}
		}

		// Evaluate all data files with associated run name
		RunContext runContext = new RunContext(
				runName,
				rubricMnemonic,
				modelMnemonic,
				dataProviderId.isEmpty()? UUID.randomUUID().toString() : dataProviderId,
				dataSourceId.isEmpty()? UUID.randomUUID().toString() : dataSourceId
		);
		RunSummary summary = evaluateService.performEvaluationRun(runContext, data, outputParquet);

		// Log summary of evaluation run
		LOG.info("Evaluation completed for run '{}'", runName);
		LOG.info("Data files: {}", data);
		LOG.info("Total evaluations performed: {}", summary.totalEvaluations());
		LOG.info("Total completed evaluations: {}", summary.totalValidEvaluations());
		LOG.info("Total failed evaluations: {}", summary.totalInvalidEvaluations());
		LOG.info("Duration: {}", summary.evaluationDuration());
		LOG.info("Failure rate: {}%", String.format("%.2f", summary.failureRate() * 100));
	}
}
