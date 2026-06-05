package dev.ikm.dq.cli.pipeline;

import dev.ikm.dq.cli.evaluate.EvaluateService;
import dev.ikm.dq.cli.evaluate.EvaluationSummary;
import dev.ikm.dq.cli.piqi.PiqiRequest;
import dev.ikm.dq.cli.run.RunContext;
import dev.ikm.dq.cli.ingest.IngestContext;
import dev.ikm.dq.cli.ingest.IngestService;
import dev.ikm.dq.cli.run.RunService;
import org.apache.arrow.vector.FieldVector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.IOException;
import java.nio.file.Files;
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

	@CommandLine.Option(names = {"--output-parquet-file"}, description = "Output parquet file containing all evaluation result data", required = true)
	private Path outputParquet;

	@CommandLine.Option(names = {"--ingestion-script"}, description = "Ingest logic duckDB script file", defaultValue = "", required = true)
	private Path script;

	@CommandLine.Option(names = {"--override-parquet-file"}, description = "Override existing output file if it already exists", defaultValue = "false")
	private boolean overrideOutput;

	private final RunService runService;
	private final IngestService ingestService;
	private final EvaluateService evaluateService;
	private final AtomicReference<EvaluationSummary> evaluationSummaryReference;

	@Autowired
	public PipelineCommand(RunService runService, IngestService ingestService, EvaluateService evaluateService) {
		this.runService = runService;
		this.ingestService = ingestService;
		this.evaluateService = evaluateService;
		this.evaluationSummaryReference = new AtomicReference<>(null);
	}

	@Override
	public void run() {

		// Perform basic validation on CLI inputs
		validateCommandLineInputs();

		try {
			// Create a new Evaluation Run in database and make corresponding RunContext
			RunContext runContext = runService.createRun(runName, modelMnemonic, rubricMnemonic)
					.withDataSourceId(dataSourceId.isEmpty() ? UUID.randomUUID().toString() : dataSourceId)
					.withDataProviderId(dataProviderId.isEmpty() ? UUID.randomUUID().toString() : dataProviderId);

			for (Path dataSource : dataSources) {

				// Create Ingestion Context for each Data Source file
				IngestContext ingestContext = new IngestContext(script, dataSource, runContext.dataSourceId(), runContext.dataProviderId());

				// Perform data ingestion and then exit
				ingestService.performDataIngestion(ingestContext, vectorSchemaRoot -> {
					LOG.info("Received batch with schema: {}", vectorSchemaRoot.getSchema());
					LOG.info("Batch row count: {}", vectorSchemaRoot.getRowCount());

					// 1. Extract the vectors from the Arrow C-data buffer
					FieldVector payloadJsonVector = vectorSchemaRoot.getVector("payload_json");
					FieldVector messageIdVector = vectorSchemaRoot.getVector("messageId");

					// 2. Safely copy the strings out of Arrow memory into Java memory
					List<PiqiRequest> batchRequests = new ArrayList<>(vectorSchemaRoot.getRowCount());
					for (int row = 0; row < vectorSchemaRoot.getRowCount(); row++) {
						String payload = payloadJsonVector.getObject(row).toString();
						String messageId = messageIdVector.getObject(row).toString();

						batchRequests.add(new PiqiRequest(
								runContext.dataProviderId(),
								runContext.dataSourceId(),
								messageId,
								runContext.modelMnemonic(),
								runContext.rubricMnemonic(),
								payload
						));
					}

					// 3. Arrow memory will be safely overwritten now.
					//    Pass the safe Java list to EvaluateService to crunch asynchronously!
					EvaluationSummary evaluationSummary = evaluateService.evaluateBatch(runContext, batchRequests, outputParquet);
					if (evaluationSummaryReference.get() == null) {
						evaluationSummaryReference.set(evaluationSummary);
					} else {
						evaluationSummaryReference.set(evaluationSummary.merge(evaluationSummaryReference.get()));
					}
				});

				LOG.info("Data ingestion completed successfully.");
			}
		} catch (SQLException e) {
			LOG.error("Data ingestion failed: {}", e.getMessage(), e);
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
	}

}
