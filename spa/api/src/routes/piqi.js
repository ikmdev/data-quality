const express = require('express');
const { saveBatchAssessmentResults, scoreMessage } = require('../services/piqiService');
const { batchQueue } = require('../queue/batchQueue');

const router = express.Router();

// Store batch results in memory (for quick retrieval after completion)
const batchResults = new Map();

function normalizeBatchMnemonics(model, rubric) {
  const modelValue = String(model || '').trim();
  const rubricValue = String(rubric || '').trim();

  const modelAliases = {
    PAT_CLINICAL: 'PAT_CLINICAL_V1',
  };

  const rubricAliases = {
    'PIQI.ALLIANCE': 'USCDI_V3',
    'PIQI.ALLIANCE.R1': 'USCDI_V3',
    USCDIV3: 'USCDI_V3',
    USCDI_V3_1: 'USCDI_V31',
  };

  const normalizedModel = modelAliases[modelValue] || modelValue;
  const normalizedRubric = rubricAliases[rubricValue] || rubricValue;

  return { normalizedModel, normalizedRubric };
}

function validatePayload(req, res, next) {
  const { dataProviderID, dataSourceID, piqiModelMnemonic, evaluationRubricMnemonic, messageData } = req.body;
  if (!dataProviderID || !dataSourceID || !piqiModelMnemonic || !evaluationRubricMnemonic || !messageData) {
    return res.status(400).json({ succeeded: false, error: 'Missing required fields.' });
  }
  next();
}

router.post('/ScoreMessage', validatePayload, async (req, res) => {
  try {
    const result = await scoreMessage(req.body, 'ScoreMessage');
    res.status(200).json(result);
  } catch (err) {
    console.error('ScoreMessage error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Internal server error.' });
  }
});

router.post('/ScoreAuditMessage', validatePayload, async (req, res) => {
  try {
    const result = await scoreMessage(req.body, 'ScoreAuditMessage');
    res.status(200).json(result);
  } catch (err) {
    console.error('ScoreAuditMessage error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Internal server error.' });
  }
});

router.post('/BatchAssessmentResults', async (req, res) => {
  try {
    const { batchRunId, items } = req.body || {};

    if (!batchRunId || !Array.isArray(items) || items.length === 0) {
      return res.status(400).json({ succeeded: false, error: 'batchRunId and non-empty items are required.' });
    }

    const result = await saveBatchAssessmentResults(batchRunId, items);
    res.status(200).json(result);
  } catch (err) {
    console.error('BatchAssessmentResults error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Internal server error.' });
  }
});

// NEW: Submit a batch job (queued processing)
router.post('/SubmitBatchJob', async (req, res) => {
  try {
    const { rows, provider, source, model, rubric } = req.body || {};

    if (!rows || !Array.isArray(rows) || rows.length === 0) {
      return res.status(400).json({ succeeded: false, error: 'rows array is required and must not be empty.' });
    }

    if (!provider || !source || !model || !rubric) {
      return res.status(400).json({ succeeded: false, error: 'provider, source, model, and rubric are required.' });
    }

    const { normalizedModel, normalizedRubric } = normalizeBatchMnemonics(model, rubric);
    const batchRunId = `batch-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;

    if (normalizedModel !== model || normalizedRubric !== rubric) {
      console.warn(
        `SubmitBatchJob normalized mnemonics: model ${model} -> ${normalizedModel}, rubric ${rubric} -> ${normalizedRubric}`
      );
    }

    // Queue the batch job
    const job = await batchQueue.add(
      { rows, provider, source, model: normalizedModel, rubric: normalizedRubric, batchRunId },
      { attempts: 1, removeOnComplete: false, removeOnFail: false }
    );

    res.status(202).json({
      succeeded: true,
      jobId: job.id,
      batchRunId,
      message: `Batch job ${job.id} queued for processing. Total rows: ${rows.length}.`,
    });
  } catch (err) {
    console.error('SubmitBatchJob error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Failed to queue batch job.' });
  }
});

// NEW: Check batch job status
router.get('/BatchJobStatus/:jobId', async (req, res) => {
  try {
    const { jobId } = req.params;
    const job = await batchQueue.getJob(jobId);

    if (!job) {
      return res.status(404).json({ succeeded: false, error: 'Job not found.' });
    }

    const state = await job.getState();
    const progress = job.progress();
    const result = state === 'completed' ? (job.returnvalue || null) : null;

    res.status(200).json({
      succeeded: true,
      jobId,
      state,
      progress,
      isCompleted: state === 'completed',
      isFailed: state === 'failed',
      result,
      error: state === 'failed' ? (job.failedReason || job.stacktrace) : null,
    });
  } catch (err) {
    console.error('BatchJobStatus error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Failed to retrieve job status.' });
  }
});

// NEW: Get batch job result
router.get('/BatchJobResult/:jobId', async (req, res) => {
  try {
    const { jobId } = req.params;
    const job = await batchQueue.getJob(jobId);

    if (!job) {
      return res.status(404).json({ succeeded: false, error: 'Job not found.' });
    }

    const state = await job.getState();

    if (state !== 'completed') {
      return res.status(400).json({ succeeded: false, error: `Job is ${state}, not completed.` });
    }

    const result = job.returnvalue;

    res.status(200).json({
      succeeded: true,
      jobId,
      result,
    });
  } catch (err) {
    console.error('BatchJobResult error:', err.message);
    res.status(500).json({ succeeded: false, error: 'Failed to retrieve job result.' });
  }
});

module.exports = router;
