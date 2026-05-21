import { createContext, createElement, useCallback, useContext, useState, type ReactNode } from 'react';
import {
  type BatchAssessmentItem,
  type BatchSummary,
  getBatchJobStatus,
  submitBatchJob,
} from '../api/piqiClient';
import { parseBatchFile } from '../api/batchClient';
import { BATCH_CONFIG } from '../api/batchConfig';

export interface AssessmentItem {
  messageId: string;
  rowNum: number;
  dataClass: string;
  attributeName: string;
  attributeValue: string;
  rawAttributeValue?: string;
  assessment: string;
  status: 'Pass' | 'Fail' | 'Skip';
  reason: string;
  effect: string;
}

export interface UseBatchApiReturn {
  isProcessing: boolean;
  errorMessage: string | null;
  summary: BatchSummary | null;
  assessmentItems: AssessmentItem[];
  processingStatus: string;
  processFile: (
    file: File,
    provider: string,
    source: string,
    model: string,
    rubric: string,
    concurrency?: number
  ) => Promise<void>;
  clearBatch: () => void;
}

const BatchApiContext = createContext<UseBatchApiReturn | null>(null);

export function BatchApiProvider({ children }: { children: ReactNode }) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [summary, setSummary] = useState<BatchSummary | null>(null);
  const [assessmentItems, setAssessmentItems] = useState<AssessmentItem[]>([]);
  const [processingStatus, setProcessingStatus] = useState('');

  const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

  const processFile = useCallback(
    async (file: File, provider: string, source: string, model: string, rubric: string, _concurrency: number = BATCH_CONFIG.pollingConfig.defaultConcurrency) => {
      setIsProcessing(true);
      setErrorMessage(null);
      setSummary(null);
      setAssessmentItems([]);

      let rows: Array<Record<string, unknown>> = [];
      try {
        setProcessingStatus('Reading batch file...');
        rows = await parseBatchFile(file);

        if (!rows.length) {
          throw new Error('Batch file contained no rows.');
        }

        setProcessingStatus(`Found ${rows.length} rows. Starting processing...`);
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : String(err);
        setErrorMessage(`Batch parse error: ${errorMsg}`);
        setIsProcessing(false);
        return;
      }

      try {
        setProcessingStatus('Submitting batch job to backend queue...');
        const submitted = await submitBatchJob({
          rows,
          provider,
          source,
          model,
          rubric,
        });

        if (!submitted.succeeded || !submitted.jobId) {
          throw new Error('Batch job submission did not return a valid job id.');
        }

        const jobId = submitted.jobId;
        setProcessingStatus(`Batch job ${jobId} submitted. Waiting for worker...`);

        let attempts = 0;
        const maxAttempts = BATCH_CONFIG.pollingConfig.maxPollAttempts;
        const pollInterval = BATCH_CONFIG.pollingConfig.pollIntervalMs;

        while (attempts < maxAttempts) {
          attempts += 1;
          const status = await getBatchJobStatus(jobId);

          if (status.isFailed) {
            throw new Error(status.error || `Batch job ${jobId} failed.`);
          }

          if (status.isCompleted && status.result) {
            const resultSummary = status.result.summary as BatchSummary;
            const items = (status.result.assessmentItems || []) as BatchAssessmentItem[];
            setSummary(resultSummary);
            setAssessmentItems(items);
            setProcessingStatus(
              resultSummary.failedRows > 0
                ? `Batch complete with errors. Processed ${resultSummary.processedRows} of ${resultSummary.totalRows} rows; failures: ${resultSummary.failedRows}.`
                : `Batch complete. Processed ${resultSummary.processedRows} rows.`
            );
            setIsProcessing(false);
            return;
          }

          const progress = typeof status.progress === 'number' ? status.progress : 0;
          setProcessingStatus(`Batch job ${jobId} is ${status.state}. Progress: ${progress}%`);
          await sleep(pollInterval);
        }

        throw new Error('Batch job polling timed out before completion.');
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : String(err);
        setErrorMessage(errorMsg);
        setProcessingStatus('Batch processing failed.');
        setIsProcessing(false);
      }
    },
    []
  );

  const clearBatch = useCallback(() => {
    setIsProcessing(false);
    setErrorMessage(null);
    setSummary(null);
    setAssessmentItems([]);
    setProcessingStatus('');
  }, []);

  const value: UseBatchApiReturn = {
    isProcessing,
    errorMessage,
    summary,
    assessmentItems,
    processingStatus,
    processFile,
    clearBatch,
  };

  return createElement(BatchApiContext.Provider, { value }, children);
}

export function useBatchApi(): UseBatchApiReturn {
  const context = useContext(BatchApiContext);
  if (!context) {
    throw new Error('useBatchApi must be used within BatchApiProvider.');
  }
  return context;
}
