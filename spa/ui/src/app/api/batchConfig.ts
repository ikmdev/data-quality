/**
 * Batch processing configuration
 * Controls column mapping, field paths, system codes, and polling behavior
 */

// Expected columns in batch files (maps internal names to column headers)
export const DEFAULT_BATCH_COLUMN_MAP = {
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

// Message structure: where to place fields in the PIQI request
export const DEFAULT_BATCH_MESSAGE_STRUCTURE = {
  dataClass: 'labResults', // primary data class we're sending
  testFieldName: 'test',
  referenceRangeFieldName: 'referenceRange',
  resultValueFieldName: 'resultValue',
  resultUnitFieldName: 'resultUnit',
  interpretationFieldName: 'interpretation',
  patientFieldName: 'patient',
};

// System identifiers (OID codes for standards like LOINC, interpretation codes)
export const DEFAULT_BATCH_SYSTEM_CODES = {
  interpretationSystem: '2.16.840.1.113883.5.83', // HL7 observation interpretation code system
  loincSystem: '2.16.840.1.113883.6.1', // LOINC code system
  ucumSystem: 'UCUM', // Units of Measure code system
};

// Default values when data is missing
export const DEFAULT_BATCH_DEFAULTS = {
  resultValueType: 'PQ', // Physical Quantity
  defaultInterpretationCode: 'N', // Normal
  defaultInterpretationText: 'N',
  messageFormatId: '',
  messageUseCaseId: '',
};

// Status mapping and summary behavior
export const DEFAULT_BATCH_STATUS_CONFIG = {
  passStatuses: ['pass', 'passed'],
  failStatuses: ['fail', 'failed'],
  skipStatuses: ['skip', 'skipped'],
};

// Polling and job configuration
export const DEFAULT_BATCH_POLLING_CONFIG = {
  defaultConcurrency: 20,
  maxConcurrency: 30,
  pollIntervalMs: 1000, // 1 second between status checks
  maxPollAttempts: 1200, // 20 minutes at 1s interval
  totalPollTimeoutMs: 1200000, // 20 minutes in ms
};

export interface BatchConfig {
  columnMap: typeof DEFAULT_BATCH_COLUMN_MAP;
  messageStructure: typeof DEFAULT_BATCH_MESSAGE_STRUCTURE;
  systemCodes: typeof DEFAULT_BATCH_SYSTEM_CODES;
  defaults: typeof DEFAULT_BATCH_DEFAULTS;
  statusConfig: typeof DEFAULT_BATCH_STATUS_CONFIG;
  pollingConfig: typeof DEFAULT_BATCH_POLLING_CONFIG;
}

const DEFAULT_BATCH_CONFIG: BatchConfig = {
  columnMap: DEFAULT_BATCH_COLUMN_MAP,
  messageStructure: DEFAULT_BATCH_MESSAGE_STRUCTURE,
  systemCodes: DEFAULT_BATCH_SYSTEM_CODES,
  defaults: DEFAULT_BATCH_DEFAULTS,
  statusConfig: DEFAULT_BATCH_STATUS_CONFIG,
  pollingConfig: DEFAULT_BATCH_POLLING_CONFIG,
};

/**
 * Load batch configuration from environment or use defaults
 * Can be overridden via import-time environment variables (Vite exposes these as import.meta.env)
 */
function loadBatchConfig(): BatchConfig {
  try {
    // Try to load from env if present (Vite prefixes public env vars with VITE_)
    const configJson = import.meta.env.VITE_BATCH_CONFIG_JSON;
    if (configJson) {
      const parsed = JSON.parse(configJson);
      return {
        columnMap: { ...DEFAULT_BATCH_COLUMN_MAP, ...parsed.columnMap },
        messageStructure: { ...DEFAULT_BATCH_MESSAGE_STRUCTURE, ...parsed.messageStructure },
        systemCodes: { ...DEFAULT_BATCH_SYSTEM_CODES, ...parsed.systemCodes },
        defaults: { ...DEFAULT_BATCH_DEFAULTS, ...parsed.defaults },
        statusConfig: { ...DEFAULT_BATCH_STATUS_CONFIG, ...parsed.statusConfig },
        pollingConfig: { ...DEFAULT_BATCH_POLLING_CONFIG, ...parsed.pollingConfig },
      };
    }
  } catch (err) {
    console.warn('Failed to parse VITE_BATCH_CONFIG_JSON, using defaults:', err);
  }
  return DEFAULT_BATCH_CONFIG;
}

export const BATCH_CONFIG = loadBatchConfig();
