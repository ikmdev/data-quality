package dev.ikm.dq.cli.evaluate;

import dev.ikm.dq.cli.parquet.DatabaseService;
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
import java.util.concurrent.atomic.AtomicReference;

@Service
public class EvaluateService implements AutoCloseable {

	Logger LOG = LoggerFactory.getLogger(EvaluateService.class);
	private static final long LOG_EVERY = 10_000;

	private final PiqiService piqiService;
	private final DatabaseService databaseService;

	ExecutorService pool;
	CompletionService<List<EvaluationResult>> completion;

	int maxConcurrentRequests = 16;
	Semaphore inFlight = new Semaphore(maxConcurrentRequests);
	private final AtomicReference<RunContext> runContext;
	private final AtomicLong submittedCount;
	private final AtomicLong completedCount;
	private final AtomicLong invalidCount;
	private final AtomicReference<Instant> startTime;

	@Autowired
	public EvaluateService(PiqiService piqiService, DatabaseService databaseService) {
		this.piqiService = piqiService;
		this.databaseService = databaseService;

		this.pool = Executors.newVirtualThreadPerTaskExecutor();
		this.completion = new ExecutorCompletionService<>(pool);

		this.runContext = new AtomicReference<>();
		this.submittedCount = new AtomicLong(0);
		this.completedCount = new AtomicLong(0);
		this.invalidCount = new AtomicLong(0);
		this.startTime = new AtomicReference<>(Instant.now());
	}

	public void initializeEvaluation(RunContext runContext) {
		try {
			databaseService.init();
			this.runContext.set(runContext);
			submittedCount.set(0);
			completedCount.set(0);
			invalidCount.set(0);
			startTime.set(Instant.now());
		} catch (SQLException e) {
			throw new RuntimeException("Failed to initialize ParquetService for evaluation.", e);
		}

	}

	public void evaluateBatch(
			List<PiqiRequest> batchRequests) {
		try {
			long batchSubmitted = 0;

			for (PiqiRequest piqiRequest : batchRequests) {

				// 1. Apply backpressure before submitting to the thread pool
				inFlight.acquire();

				completion.submit(() -> {
					try {
						return piqiService.sendRequestToPiqiEngine(runContext.get().runId(), piqiRequest);
					} catch (Exception ex) {
						invalidCount.incrementAndGet();
						LOG.warn("PIQI request failed for requestId={}: {} \n{}",
								piqiRequest.messageID(), piqiRequest.messageData(), ex.toString());
						throw new RuntimeException("PIQI request failed for requestId=" + piqiRequest.messageID(), ex);
//						return List.of();
					} finally {
						// Always release semaphore to prevent pool starvation
						inFlight.release();
					}
				});

				// Increment Submitted Count
				submittedCount.incrementAndGet();

				// Increatement local batch count
				batchSubmitted++;
			}

			if (batchSubmitted > 0) {
				drainCompletedResults(batchSubmitted, startTime.get());
			}

		} catch (ExecutionException e) {
			Thread.currentThread().interrupt();
			throw new RuntimeException("Async execution failed during file processing", e);
		} catch (SQLException e) {
			throw new RuntimeException("Database error during orchestration", e);
		} catch (InterruptedException e) {
			Thread.currentThread().interrupt();
			throw new RuntimeException("Processing interrupted while iterating over request batch", e);
		}
	}

	public EvaluationSummary finalizeEvaluation() {
		try {
			databaseService.insertIntoPostgres();
		} catch (SQLException e) {
			throw new RuntimeException("Failed to insert results to postgres.", e);
		}
		return new EvaluationSummary(
				runContext.get().runName(),
				submittedCount.get(),
				completedCount.get() - invalidCount.get(),
				invalidCount.get(),
				Duration.between(startTime.get(), Instant.now())
		);
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
			databaseService.append(result);

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

	@Override
	public void close() throws Exception {
		LOG.info("Shutting down evaluation thread pool...");
		pool.shutdown();
		try {
			databaseService.close();
		} catch (Exception e) {
			LOG.error("Failed to close ParquetService cleanly.", e);
		}
	}
}
