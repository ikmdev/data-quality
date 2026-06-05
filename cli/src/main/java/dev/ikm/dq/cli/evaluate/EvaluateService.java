package dev.ikm.dq.cli.evaluate;

import dev.ikm.dq.cli.parquet.ParquetService;
import dev.ikm.dq.cli.piqi.PiqiRequest;
import dev.ikm.dq.cli.piqi.PiqiService;
import dev.ikm.dq.cli.run.RunContext;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.nio.file.Path;
import java.sql.SQLException;
import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.concurrent.CompletionService;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorCompletionService;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicLong;

@Service
public class EvaluateService {

	Logger LOG = LoggerFactory.getLogger(EvaluateService.class);
	private static final long LOG_EVERY = 10_000;

	private final PiqiService piqiService;
	private final ParquetService parquetService;

	ExecutorService pool = Executors.newVirtualThreadPerTaskExecutor();
	CompletionService<List<EvaluationResult>> completion = new ExecutorCompletionService<>(pool);

	private static final int BATCH_SIZE = 25000;
	int maxConcurrentRequests = 16;
	Semaphore inFlight = new Semaphore(maxConcurrentRequests);
	private final AtomicLong submittedCount = new AtomicLong(0);
	private final AtomicLong completedCount = new AtomicLong(0);
	private final AtomicLong invalidCount = new AtomicLong(0);

	@Autowired
	public EvaluateService(PiqiService piqiService, ParquetService parquetService) {
		this.piqiService = piqiService;
		this.parquetService = parquetService;
	}

	public EvaluationSummary evaluateBatch(RunContext runContext,
	                                       List<PiqiRequest> batchRequests,
	                                       Path outputParquet) {
		// Initialize evaluation summary values
		submittedCount.set(0);
		completedCount.set(0);
		invalidCount.set(0);
		Instant startTime = Instant.now();
		LOG.info("Starting evaluation run '{}' with {} requests. Concurrency={}, Client BatchSize={}",
				runContext.runName(), batchRequests.size(), maxConcurrentRequests, BATCH_SIZE);

		try {
			parquetService.init(outputParquet);

			long submittedInBatch = 0;

			try {
				for (PiqiRequest piqiRequest : batchRequests) {

					// 1. Apply backpressure before submitting to the thread pool
					inFlight.acquire();

					completion.submit(() -> {
						try {
							return piqiService.sendRequestToPiqiEngine(runContext.runId(), piqiRequest);
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
			} catch (InterruptedException e) {
				Thread.currentThread().interrupt();
				throw new RuntimeException("Processing interrupted while iterating over request batch", e);
			}

			// Export only once all data from all files has been cleanly written
			parquetService.exportToParquet();

		} catch (ExecutionException e) {
			Thread.currentThread().interrupt();
			throw new RuntimeException("Async execution failed during file processing", e);
		} catch (SQLException e) {
			throw new RuntimeException("Database error during orchestration", e);
		} finally {
			pool.shutdown();
			// Now, close the ParquetService to release its resources
			try {
				parquetService.close();
			} catch (Exception e) {
				LOG.error("Failed to close ParquetService cleanly.", e);
			}
		}

		return new EvaluationSummary(
				runContext.runName(),
				submittedCount.get(),
				completedCount.get() - invalidCount.get(),
				invalidCount.get(),
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

			// Immediately stream to parquet, allowing GC to collect the Result object
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
