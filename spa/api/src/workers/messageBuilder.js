/**
 * Message building utilities
 * Converts spreadsheet rows into PIQI request messages
 */

const BATCH_COLUMN_MAP = {
  UniqueID: 'UniqueID',
  LongAccessionNumberUID: 'LongAccessionNumberUID',
  LabChemTestSID: 'LabChemTestSID',
  LabChemTestName: 'LabChemTestName',
  LabChemTestUrgencySID: 'LabChemTestUrgencySID',
  Urgency: 'Urgency',
  LabChemResultValue: 'LabChemResultValue',
  LabChemResultNumericValue: 'LabChemResultNumericValue',
  TopographySID: 'TopographySID',
  Topography: 'Topography',
  AccessionInstitutionSID: 'AccessionInstitutionSID',
  AccessioningInstitution: 'AccessioningInstitution',
  OrderingInstitutionSID: 'OrderingInstitutionSID',
  OrderingInstutionName: 'OrderingInstutionName',
  CollectingInstitutionSID: 'CollectingInstitutionSID',
  CollectingInstitutionName: 'CollectingInstitutionName',
  LOINCSID: 'LOINCSID',
  LOINC: 'LOINC',
  Units: 'Units',
  Abnormal: 'Abnormal',
  RefHigh: 'RefHigh',
  RefLow: 'RefLow',
};

function normalizeBatchCell(value) {
  return value === undefined || value === null ? '' : String(value).trim();
}

function getBatchColumnValue(row, key) {
  const colName = BATCH_COLUMN_MAP[key];
  return normalizeBatchCell(row && row[colName]);
}

function extractBatchScore(response) {
  if (!response || typeof response !== 'object') {
    return null;
  }
  if (typeof response.score === 'number') {
    return response.score;
  }
  if (response.qualityScore !== undefined) {
    return response.qualityScore;
  }
  return null;
}

/**
 * Build a PIQI request from a spreadsheet row
 * @param {Object} rowData - Row from spreadsheet
 * @param {number} rowNum - Row number
 * @param {string} provider - Data provider ID
 * @param {string} source - Data source ID
 * @param {string} model - PIQI model mnemonic
 * @param {string} rubric - Evaluation rubric mnemonic
 * @returns {Object} PIQI request ready to send
 */
function buildBatchRowRequest(rowData, rowNum, provider, source, model, rubric) {
  const uniqueID = getBatchColumnValue(rowData, 'UniqueID') || `row-${rowNum}`;
  const unitsValue = getBatchColumnValue(rowData, 'Units');
  const testName = getBatchColumnValue(rowData, 'LabChemTestName');
  const loincCode = getBatchColumnValue(rowData, 'LOINC');
  const refLow = getBatchColumnValue(rowData, 'RefLow');
  const refHigh = getBatchColumnValue(rowData, 'RefHigh');
  const abnormal = getBatchColumnValue(rowData, 'Abnormal');

  const labResult = {
    test: { codings: [], text: testName },
    referenceRange: {},
    resultValue: { text: getBatchColumnValue(rowData, 'LabChemResultValue'), type: { text: 'PQ' } },
    resultUnit: {
      codings: unitsValue ? [{ code: unitsValue, display: unitsValue, system: 'UCUM' }] : [],
      text: unitsValue,
    },
    interpretation: abnormal
      ? { codings: [{ code: abnormal, system: '2.16.840.1.113883.5.83' }], text: abnormal }
      : { codings: [{ code: 'N', system: '2.16.840.1.113883.5.83' }], text: 'N' },
  };

  if (loincCode) {
    labResult.test.codings.push({
      code: loincCode,
      display: testName,
      system: '2.16.840.1.113883.6.1',
    });
  }

  if (refLow) {
    labResult.referenceRange.lowValue = refLow;
  }

  if (refHigh) {
    labResult.referenceRange.highValue = refHigh;
  }

  const messageData = {
    messageId: uniqueID,
    messageID: uniqueID,
    formatID: '',
    useCaseID: '',
    patient: {
      id: uniqueID,
      labResults: [labResult],
    },
    dataSourceID: source,
    dataProviderID: provider,
  };

  return {
    dataProviderID: provider,
    dataSourceID: source,
    piqiModelMnemonic: model,
    evaluationRubricMnemonic: rubric,
    messageData: JSON.stringify(messageData),
    messageID: uniqueID,
  };
}

module.exports = {
  buildBatchRowRequest,
  extractBatchScore,
  getBatchColumnValue,
};
