/**
 * Message building for batch requests
 * Converts spreadsheet rows to PIQI message structure
 */

import { BATCH_CONFIG } from './batchConfig';
import type { BatchRow } from './fileParser';

interface MessageData {
  messageId: string;
  messageID: string;
  formatID: string;
  useCaseID: string;
  patient: {
    id: string;
    labResults: Array<{
      test: {
        codings: Array<{ code: string; display: string; system: string }>;
        text: string;
      };
      referenceRange: { lowValue?: string; highValue?: string };
      resultValue: { text: string; type: { text: string } };
      resultUnit: {
        codings: Array<{ code: string; display: string; system: string }>;
        text: string;
      };
      interpretation: {
        codings: Array<{ code: string; system: string }>;
        text: string;
      };
    }>;
  };
  dataSourceID: string;
  dataProviderID: string;
}

export interface PIQIBatchRequest {
  dataProviderID: string;
  dataSourceID: string;
  messageID: string;
  piqiModelMnemonic: string;
  evaluationRubricMnemonic: string;
  messageData: string;
}

function normalizeBatchCell(value: unknown): string {
  return value === undefined || value === null ? '' : String(value).trim();
}

function getBatchColumnValue(row: BatchRow, key: keyof typeof BATCH_CONFIG.columnMap): string {
  const colName = BATCH_CONFIG.columnMap[key];
  return normalizeBatchCell(row && row[colName]);
}

function convertBatchSpreadsheetRowToMessageData(
  row: BatchRow,
  provider: string,
  source: string,
  fallbackId: string
): MessageData {
  const uniqueID = getBatchColumnValue(row, 'UniqueID') || fallbackId;
  const unitsValue = getBatchColumnValue(row, 'Units');
  const testName = getBatchColumnValue(row, 'LabChemTestName');
  const loincCode = getBatchColumnValue(row, 'LOINC');
  const refLow = getBatchColumnValue(row, 'RefLow');
  const refHigh = getBatchColumnValue(row, 'RefHigh');
  const abnormal = getBatchColumnValue(row, 'Abnormal');

  const cfg = BATCH_CONFIG;

  const labResult: MessageData['patient']['labResults'][0] = {
    test: { codings: [], text: testName },
    referenceRange: {},
    resultValue: { text: getBatchColumnValue(row, 'LabChemResultValue'), type: { text: cfg.defaults.resultValueType } },
    resultUnit: {
      codings: unitsValue ? [{ code: unitsValue, display: unitsValue, system: cfg.systemCodes.ucumSystem }] : [],
      text: unitsValue,
    },
    interpretation: abnormal
      ? { codings: [{ code: abnormal, system: cfg.systemCodes.interpretationSystem }], text: abnormal }
      : { codings: [{ code: cfg.defaults.defaultInterpretationCode, system: cfg.systemCodes.interpretationSystem }], text: cfg.defaults.defaultInterpretationText },
  };

  if (loincCode) {
    labResult.test.codings.push({
      code: loincCode,
      display: testName,
      system: cfg.systemCodes.loincSystem,
    });
  }

  if (refLow) {
    labResult.referenceRange.lowValue = refLow;
  }
  if (refHigh) {
    labResult.referenceRange.highValue = refHigh;
  }

  return {
    messageId: uniqueID,
    messageID: uniqueID,
    formatID: cfg.defaults.messageFormatId,
    useCaseID: cfg.defaults.messageUseCaseId,
    patient: {
      labResults: [labResult],
      id: uniqueID,
    },
    dataSourceID: source,
    dataProviderID: provider,
  };
}

export function buildBatchRowRequest(
  row: BatchRow,
  rowNum: number,
  provider: string,
  source: string,
  model: string,
  rubric: string
): PIQIBatchRequest {
  const fallbackId = (row.messageID || row.messageId || row.UniqueID || row.ID || `row-${rowNum}`) as string;
  const messageData = convertBatchSpreadsheetRowToMessageData(row, provider, source, fallbackId);
  const messageId = messageData.messageID || messageData.messageId || fallbackId;

  return {
    dataProviderID: provider,
    dataSourceID: source,
    messageID: messageId,
    piqiModelMnemonic: model,
    evaluationRubricMnemonic: rubric,
    messageData: JSON.stringify(messageData),
  };
}
