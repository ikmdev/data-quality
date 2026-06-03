package dev.ikm.dq.cli.evaluate;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
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
import java.util.concurrent.atomic.AtomicLong;
import java.util.stream.Stream;

@Service
public class EvaluateService {

	Logger LOG = LoggerFactory.getLogger(EvaluateService.class);
	private static final long LOG_EVERY = 10_000;

	private final DatabaseService databaseService;
	private final DataParserService dataParserService;
	private final PiqiService piqiService;
	private final ParquetService parquetService;

	ExecutorService pool = Executors.newVirtualThreadPerTaskExecutor();
	CompletionService<List<EvaluationResult>> completion = new ExecutorCompletionService<>(pool);

	private static final int BATCH_SIZE = 25000;
	int maxConcurrentRequests = 64;
	Semaphore inFlight = new Semaphore(maxConcurrentRequests);
	private final AtomicLong submittedCount = new AtomicLong(0);
	private final AtomicLong completedCount = new AtomicLong(0);
	private final AtomicLong invalidCount = new AtomicLong(0);

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

	public RunSummary performEvaluationRun(RunContext runContext,
	                                       List<Path> data,
	                                       Path outputParquet) {
		// Initialize evaluation summary values
		submittedCount.set(0);
		completedCount.set(0);
		invalidCount.set(0);

		Instant startTime = Instant.now();
		LOG.info("Starting evaluation run '{}' with {} file(s). Concurrency={}, BatchSize={}",
				runContext.runName(), data.size(), maxConcurrentRequests, BATCH_SIZE);

		try {
			long runId = 0;
			parquetService.init(outputParquet);

			for (Path datum : data) {
				LOG.info("Processing file: {}", datum.getFileName());
				try (Stream<PiqiRequest> piqiRequests = dataParserService.parseCSVData(datum, runContext)) {
					Iterator<PiqiRequest> iterator = piqiRequests.iterator();

					long submittedInBatch = 0;

					while (iterator.hasNext()) {
						PiqiRequest piqiRequest = iterator.next();

						// 1. Apply backpressure before submitting to the thread pool
						inFlight.acquire();

						completion.submit(() -> {
							try {
								return piqiService.sendRequestToPiqiEngine(runId, piqiRequest);
							} catch (Exception ex) {
								invalidCount.incrementAndGet();
								LOG.warn("PIQI request failed for requestId={} (count invalid and continue): {}",
										piqiRequest.messageID(), ex.toString());
								return List.of();
							} finally {
								// Always release semaphore to prevent pool starvation
								inFlight.release();
							}
						});

						// Increment Submitted Count
						submittedCount.incrementAndGet();
						submittedInBatch++;

						// 2. Intermittent draining: Drain when batch is full to control memory footprint
						if (submittedInBatch >= BATCH_SIZE) {
							drainCompletedResults(submittedInBatch, startTime);
							submittedInBatch = 0; // Reset batch tracker
						}
					}

					// 3. Clean up remaining tasks for the current file
					if (submittedInBatch > 0) {
						drainCompletedResults(submittedInBatch, startTime);
					}
					LOG.info("Finished processing file: {}", datum.getFileName());
				} catch (InterruptedException e) {
					Thread.currentThread().interrupt();
					throw new RuntimeException("Processing interrupted while reading file: " + datum, e);
				}
			}

			// Export only once all data from all files has been cleanly written
			parquetService.exportToParquet();

		} catch (IOException e) {
			throw new RuntimeException("Error processing data files", e);
		} catch (ExecutionException e) {
			Thread.currentThread().interrupt();
			throw new RuntimeException("Async execution failed during file processing", e);
		} catch (SQLException e) {
			throw new RuntimeException("Database error during orchestration", e);
		} finally {
			pool.shutdown();
		}

		long totalSubmitted = submittedCount.get();
		long totalInvalid = invalidCount.get();
		long totalCompleted = completedCount.get();
		// A "valid" evaluation is one that completed without error.
		long totalValid = totalCompleted - totalInvalid;

		return new RunSummary(
				runContext.runName(),
				totalSubmitted, // Total attempts
				totalValid,
				totalInvalid,
				Duration.between(startTime, Instant.now()));
	}

	/**
	 * Safely blocks and drains an exact number of completed futures, streaming them directly to Parquet.
	 */
	private void drainCompletedResults(long tasksToDrain, Instant startTime) throws InterruptedException, ExecutionException, SQLException {
		for (long i = 0; i < tasksToDrain; i++) {
			// completion.take() blocks until ANY background task finishes execution
			Future<List<EvaluationResult>> completedFuture = completion.take();
			List<EvaluationResult> result = completedFuture.get();

			// Immediately stream to disk storage, allowing GC to collect the Result object
			parquetService.append(result);

			// --- INCREMENT COMPLETED COUNT & LOG PROGRESS ---
			long currentCompleted = completedCount.incrementAndGet();
			logProgressIfNeeded(currentCompleted, submittedCount.get(), invalidCount.get(), startTime);
		}
	}

	private void logProgressIfNeeded(long completed, long submitted, long invalid, Instant startTime) {
		if (completed % LOG_EVERY == 0) {
			long valid = completed - invalid;
			LOG.info("Progress: submitted={}, completed={}, valid={}, invalid={}, inFlight={}",
					submitted, completed, valid, invalid, inFlight.availablePermits());
		}
	}
}
