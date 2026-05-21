/**
 * Batch Client - Main entry point for batch file handling
 * Re-exports from modular components for backward compatibility
 */

import { BATCH_CONFIG } from './batchConfig';

// File parsing
export { parseBatchFile, type BatchRow } from './fileParser';

// Message building
export { buildBatchRowRequest, type PIQIBatchRequest } from './messageBuilder';

// Response extraction
export { extractBatchScore, summarizeAssessmentStatuses } from './responseExtractor';

// Re-export config for backward compatibility
export const BATCH_COLUMN_MAP = BATCH_CONFIG.columnMap;
