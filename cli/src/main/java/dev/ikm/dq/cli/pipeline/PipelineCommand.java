package dev.ikm.dq.cli.pipeline;

import dev.ikm.dq.cli.evaluate.EvaluateService;
import dev.ikm.dq.cli.evaluate.EvaluationSummary;
import dev.ikm.dq.cli.ingest.IngestContext;
import dev.ikm.dq.cli.ingest.IngestService;
import dev.ikm.dq.cli.piqi.PiqiRequest;
import dev.ikm.dq.cli.run.RunContext;
import dev.ikm.dq.cli.run.RunService;
import org.apache.arrow.vector.FieldVector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.nio.file.Path;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.atomic.AtomicReference;

@Component
@CommandLine.Command(name = "pipeline", description = "Run a data quality pipeline")
public class PipelineCommand implements Runnable {

	private static final Logger LOG = LoggerFactory.getLogger(PipelineCommand.class);

	@CommandLine.Option(names = {"--run-name"}, description = "Name of the run", required = true)
	private String runName;

	@CommandLine.Option(names = {"--rubric-mnemonic"}, description = "Mnemonic of the rubric to use for evaluation", required = true)
	private String rubricMnemonic;

	@CommandLine.Option(names = {"--model-mnemonic"}, description = "Mnemonic of the model to use for evaluation", required = true)
	private String modelMnemonic;

	@CommandLine.Option(names = {"--data-provider-id"}, description = "Identifier for the data provider (e.g., source system or team)", defaultValue = "")
	private String dataProviderId;

	@CommandLine.Option(names = {"--data-source-id"}, description = "Identifier for the data source (e.g., database, table, or file)", defaultValue = "")
	private String dataSourceId;

	@CommandLine.Option(names = {"--data-source-file"}, description = "Input data source file(s) to process", required = true)
	private List<Path> dataSources;

	@CommandLine.Option(names = {"--ingestion-script"}, description = "Ingest logic duckDB script file", defaultValue = "", required = true)
	private Path script;


	private final RunService runService;
	private final IngestService ingestService;
	private final EvaluateService evaluateService;

	@Autowired
	public PipelineCommand(RunService runService, IngestService ingestService, EvaluateService evaluateService) {
		this.runService = runService;
		this.ingestService = ingestService;
		this.evaluateService = evaluateService;
	}

	@Override
	public void run() {

		// Perform basic validation on CLI inputs
		validateCommandLineInputs();

		LOG.info("Starting Pipeline '{}'", runName);
		LOG.info("Model: {} | Rubric: {}", modelMnemonic, rubricMnemonic);
		LOG.info("Data Sources: {} files to process", dataSources.size());

		AtomicReference<RunContext> runContext = new AtomicReference<>();

		try {
			// Create a new Evaluation Run in database and make corresponding RunContext
			runContext.set(runService.createRun(runName, modelMnemonic, rubricMnemonic)
					.withDataSourceId(dataSourceId.isEmpty() ? UUID.randomUUID().toString() : dataSourceId)
					.withDataProviderId(dataProviderId.isEmpty() ? UUID.randomUUID().toString() : dataProviderId));
			RunContext context = runContext.get();

			LOG.info("Created evaluation run record. Internal Run ID: {}", context.runId());
			LOG.debug("Resolved Data Provider ID: {}", context.dataProviderId());
			LOG.debug("Resolved Data Source ID: {}", context.dataSourceId());
			LOG.info("Using DuckDB Script: {}", script.getFileName());


			// Initialize the EvaluateService, which will be used to perform the evaluation
			evaluateService.initializeEvaluation(context);

			for (Path dataSource : dataSources) {

				// Create Ingestion Context for each Data Source file
				IngestContext ingestContext = new IngestContext(script, dataSource, context.dataSourceId(), context.dataProviderId());

				LOG.info("========================================");
				LOG.info("Processing Data source File: {}", dataSource.getFileName());

				// Perform data ingestion and then exit
				ingestService.performDataIngestion(ingestContext, vectorSchemaRoot -> {

					// 1. Extract the vectors from the Arrow C-data buffer
					FieldVector payloadJsonVector = vectorSchemaRoot.getVector("payload_json");
					FieldVector messageIdVector = vectorSchemaRoot.getVector("messageId");

					// 2. Safely copy the strings out of Arrow memory into Java memory
					List<PiqiRequest> batchRequests = new ArrayList<>(vectorSchemaRoot.getRowCount());
					for (int row = 0; row < vectorSchemaRoot.getRowCount(); row++) {
						Object payloadObj = payloadJsonVector.getObject(row);
						Object messageIdObj = messageIdVector.getObject(row);

						// SKIP this row if the DuckDB query evaluated either field to a SQL NULL
						if (payloadObj == null || messageIdObj == null) {
							LOG.debug("Skipping row {} in batch: Null payload or messageId encountered", row);
							continue;
						}

						String payload = payloadObj.toString();
						String messageId = messageIdObj.toString();

						batchRequests.add(new PiqiRequest(
								context.dataProviderId(),
								context.dataSourceId(),
								messageId,
								context.modelMnemonic(),
								context.rubricMnemonic(),
								payload
						));
					}

					// 3. Arrow memory will be safely overwritten now.
					//    Pass the safe Java list to EvaluateService to crunch asynchronously!
					evaluateService.evaluateBatch(batchRequests);
				});

				LOG.info("Data evaluation completed successfully.");
			}

			// Finalize the total evaluation
			EvaluationSummary evaluationSummary = evaluateService.finalizeEvaluation();

			// Close out the run with metrics
			runService.completeRun(context, evaluationSummary);

			// Log final summary of the evaluation run
			logEvaluationSummary(evaluationSummary);

		} catch (SQLException e) {
			runService.failRun(runContext.get());
			LOG.error("Data pipeline failed: {}", e.getMessage(), e);
			throw new RuntimeException(e);
		}
	}

	private void validateCommandLineInputs() {
		// Make sure that all data files exactly exist
		dataSources.forEach(dataPath -> {
			if (!dataPath.toFile().exists()) {
				throw new RuntimeException("Data path does not exist: " + dataPath);
			}
		});
	}

	private void logEvaluationSummary(EvaluationSummary evaluationSummary) {
		if (evaluationSummary != null) {
			LOG.info("========================================");
			LOG.info("PIPELINE COMPLETED FOR RUN '{}'", runName);
			LOG.info("Total Files Processed      : {}", dataSources.size());
			LOG.info("----------------------------------------");
			LOG.info("Total Evaluations           : {}", evaluationSummary.totalEvaluations());
			LOG.info("Total Valid Evaluations     : {}", evaluationSummary.totalValidEvaluations());
			LOG.info("Total Invalid/Failed        : {}", evaluationSummary.totalInvalidEvaluations());
			LOG.info("Total Pipeline Duration     : {}", evaluationSummary.evaluationDuration());
			LOG.info("Evaluation Failure Rate     : {}%", String.format("%.2f", evaluationSummary.failureRate() * 100));
			LOG.info("========================================");
		} else {
			LOG.warn("Pipeline completed but no evaluations were processed.");
		}
	}

}
