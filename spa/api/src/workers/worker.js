/**
 * Batch job worker for the queue
 * Orchestrates concurrent processing of batch rows
 */

const { batchQueue } = require('../queue/batchQueue');
const { scoreMessage, saveBatchAssessmentResults } = require('../services/piqiService');
const { BATCH_CONFIG, BATCH_ROW_CONCURRENCY } = require('./batchConfig');
const { buildBatchRowRequest, extractBatchScore } = require('./messageBuilder');
const { extractAssessmentItemsFromResponse, summarizeAssessmentStatuses } = require('./assessmentExtractor');

function isSuccessfulScoringResponse(response) {
  if (!response || typeof response !== 'object') {
    return false;
  }

  if (response.succeeded === true) {
    return true;
  }

  if (response.succeeded === false) {
    return false;
  }

  // Some scorer implementations do not return `succeeded`, but still return
  // a valid audited payload.
  return response.auditedMessage !== undefined
    || response.score !== undefined
    || response.qualityScore !== undefined;
}

function deriveScoreFromAssessmentSummary(assessmentSummary) {
  const passCount = Number(assessmentSummary?.pass || 0);
  const failCount = Number(assessmentSummary?.fail || 0);
  const skipCount = Number(assessmentSummary?.skip || 0);
  const total = passCount + failCount + skipCount;

  if (total <= 0) {
    return null;
  }

  // Factor all three outcomes: Pass=100, Skip=50, Fail=0.
  const weighted = (passCount * 1) + (skipCount * 0.5) + (failCount * 0);
  return Math.round((weighted / total) * 100);
}

/**
 * Process a single batch row through the scorer
 */
async function processBatchRow(rowData, rowNum, job, provider, source, model, rubric) {
  const requestBody = buildBatchRowRequest(rowData, rowNum, provider, source, model, rubric);

  const response = await scoreMessage(requestBody, 'ScoreAuditMessage');

  if (!isSuccessfulScoringResponse(response)) {
    throw new Error(typeof response === 'string' ? response : JSON.stringify(response, null, 2));
  }

  const score = extractBatchScore(response);
  const messageId = requestBody.messageID;

  const assessmentItemList = extractAssessmentItemsFromResponse(response, messageId, rowNum, BATCH_CONFIG);
  const assessmentSummary = summarizeAssessmentStatuses(assessmentItemList);
  const derivedScore = deriveScoreFromAssessmentSummary(assessmentSummary);
  const resolvedScore = score === null ? derivedScore : score;
  const persistItems = assessmentItemList.map((item) => ({
    messageId: item.messageId,
    rowNum: item.rowNum,
    dataClass: item.dataClass,
    attributeName: item.attributeName,
    attributeValue: item.attributeValue,
    assessment: item.assessment,
    status: item.status,
    reason: item.reason,
    effect: item.effect,
  }));

  return {
    processed: 1,
    succeeded: 1,
    failed: 0,
    score: resolvedScore,
    assessmentItemList,
    assessmentSummary,
    persistItems,
  };
}

/**
 * Build fallback result when a row fails to process
 */
function buildSkippedRowResult(rowData, rowNum, errorMsg) {
  const fallbackMessageId = rowData?.UniqueID || `row-${rowNum}`;
  const fallbackItem = {
    messageId: fallbackMessageId,
    rowNum,
    dataClass: BATCH_CONFIG.fallbackAssessmentClass,
    attributeName: BATCH_CONFIG.fallbackAssessmentAttribute,
    attributeValue: 'N/A',
    rawAttributeValue: 'N/A',
    assessment: BATCH_CONFIG.fallbackAssessmentName,
    status: 'Fail',
    reason: errorMsg,
    effect: 'Scoring',
  };

  return {
    processed: 1,
    succeeded: 0,
    failed: 1,
    score: null,
    assessmentItemList: [fallbackItem],
    assessmentSummary: { pass: 0, fail: 1, skip: 0 },
    persistItems: [{
      messageId: fallbackItem.messageId,
      rowNum: fallbackItem.rowNum,
      dataClass: fallbackItem.dataClass,
      attributeName: fallbackItem.attributeName,
      attributeValue: fallbackItem.attributeValue,
      assessment: fallbackItem.assessment,
      status: fallbackItem.status,
      reason: fallbackItem.reason,
      effect: fallbackItem.effect,
    }],
    fallbackItem,
  };
}

/**
 * Process a row with error handling
 */
async function processBatchRowSafely(rowData, rowNum, job, provider, source, model, rubric) {
  try {
    return await processBatchRow(rowData, rowNum, job, provider, source, model, rubric);
  } catch (err) {
    const errorMsg = err instanceof Error ? err.message : String(err);
    // Suppressed noisy error logging for each row
    return buildSkippedRowResult(rowData, rowNum, errorMsg);
  }
}

/**
 * Main batch job processor
 * Spawns worker coroutines to process rows concurrently
 */
batchQueue.process(async (job) => {
  const { rows, provider, source, model, rubric, batchRunId } = job.data;
  const startedAt = Date.now();
  await job.progress(0);
  
  let processed = 0;
  let succeeded = 0;
  let failed = 0;
  let scoreSum = 0;
  let scoreCount = 0;
  const classBreakdown = { pass: 0, fail: 0, skip: 0 };
  const allAssessmentItems = [];
  const pendingPersistItems = [];
  let completedRows = 0;
  let nextRowIndex = 0;
  const workerCount = Math.min(BATCH_ROW_CONCURRENCY, Math.max(rows.length, 1));

  const workerTasks = Array.from({ length: workerCount }, async () => {
    while (true) {
      const currentIndex = nextRowIndex;
      nextRowIndex += 1;

      if (currentIndex >= rows.length) {
        return;
      }

      const rowNum = currentIndex + 1;
      const rowResult = await processBatchRowSafely(rows[currentIndex], rowNum, job, provider, source, model, rubric);

      processed += rowResult.processed;
      succeeded += rowResult.succeeded;
      failed += rowResult.failed;

      if (rowResult.score !== null) {
        scoreSum += rowResult.score;
        scoreCount += 1;
      }

      classBreakdown.pass += rowResult.assessmentSummary.pass;
      classBreakdown.fail += rowResult.assessmentSummary.fail;
      classBreakdown.skip += rowResult.assessmentSummary.skip;
      allAssessmentItems.push(...rowResult.assessmentItemList);
      pendingPersistItems.push(...rowResult.persistItems);
      completedRows += 1;

      const processingProgress = Math.min(99, Math.round((completedRows / rows.length) * 100));
      await job.progress(processingProgress);
    }
  });

  await Promise.all(workerTasks);

  const avgScore = scoreCount > 0 ? Math.round(scoreSum / scoreCount) : null;
  const elapsedMs = Date.now() - startedAt;
  const successRate = processed > 0 ? ((succeeded / processed) * 100).toFixed(1) : '0.0';
  const processingRate = elapsedMs > 0 ? `${(processed / (elapsedMs / 1000)).toFixed(2)} rows/sec` : '0.00 rows/sec';
  const summary = {
    batchRunId,
    totalRows: rows.length,
    processedRows: processed,
    succeededRows: succeeded,
    failedRows: failed,
    averageScore: avgScore === null ? 'N/A' : `${avgScore}%`,
    classBreakdown,
    elapsedMs,
    successRate: `${successRate}%`,
    processingRate,
    completedAt: new Date().toISOString(),
  };

  // Batch database writes for better performance
  if (pendingPersistItems.length > 0) {
    const batchSize = 100; // Adjust batch size as needed
    for (let i = 0; i < pendingPersistItems.length; i += batchSize) {
      const batch = pendingPersistItems.slice(i, i + batchSize);
      await saveBatchAssessmentResults(batchRunId, batch);
    }
  }

  await job.progress(100);

  return {
    summary,
    assessmentItems: allAssessmentItems,
  };
});

// Removed the console log for batch worker initialization
// console.log('Batch worker initialized');
