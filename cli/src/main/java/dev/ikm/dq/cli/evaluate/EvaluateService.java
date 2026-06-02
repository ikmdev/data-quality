package dev.ikm.dq.cli.evaluate;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.nio.file.Path;
import java.sql.SQLException;
import java.time.Duration;
import java.time.Instant;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.CompletionService;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorCompletionService;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.Semaphore;
import java.util.stream.Stream;

@Service
public class EvaluateService {

	private final DatabaseService databaseService;
	private final DataParserService dataParserService;
	private final PiqiService piqiService;
	private final ParquetService parquetService;

	int httpThreads = 8;
	int maxInFlight = 8 * 200;

	ExecutorService pool = Executors.newFixedThreadPool(httpThreads);
	CompletionService<EvaluationResult> completion = new ExecutorCompletionService<>(pool);
	Semaphore inFlight = new Semaphore(maxInFlight);
	long submitted = 0;

	@Autowired
	public EvaluateService(DatabaseService databaseService,
						   DataParserService dataParserService,
						   PiqiService piqiService,
						   ParquetService parquetService) {
		this.databaseService = databaseService;
		this.dataParserService = dataParserService;
		this.piqiService = piqiService;
		this.parquetService = parquetService;
	}

	public EvaluationSummary evaluateData(PiqiContext piqiContext,
										  List<Path> data,
										  Path outputParquet) {
		// Initialize evaluation summary values
		long totalEvaluations = 0;
		long totalValidEvaluations = 0;
		long totalInvalidEvaluations = 0;
		Instant startTime = Instant.now();

		// Perform evaluation on data files
		try {
			for (Path datum : data) {
				try (Stream<PiqiRequest> piqiRequests = dataParserService.parseCSVData(datum, piqiContext)) {
					Iterator<PiqiRequest> iterator = piqiRequests.iterator();

					while(iterator.hasNext()) {
						PiqiRequest piqiRequest = iterator.next();
						inFlight.acquire();
						completion.submit(() -> {
							try {
								return piqiService.sendRequestToPiqiEngine(piqiContext, piqiRequest);
							} finally {
								inFlight.release();
							}
						});
						submitted++;
					}
				} catch (InterruptedException e) {
					throw new RuntimeException(e);
				}

				parquetService.init(outputParquet);
				// drain completed results
				for (long i = 0; i < submitted; i++) {
					Future<EvaluationResult> f = completion.take();
					EvaluationResult result = f.get();
					parquetService.append(result);
				}
				parquetService.exportToParquet();
			}
		} catch (IOException e) {
			throw new RuntimeException("Error processing data files", e);
		} catch (ExecutionException | InterruptedException e) {
			throw new RuntimeException(e);
		} catch (SQLException e) {
			throw new RuntimeException(e);
		} finally {
			pool.shutdown();
		}

		return new EvaluationSummary(
				piqiContext.runName(),
				totalEvaluations,
				totalValidEvaluations,
				totalInvalidEvaluations,
				Duration.between(startTime, Instant.now())
		);
	}
}
