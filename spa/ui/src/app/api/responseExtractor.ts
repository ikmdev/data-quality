/**
 * Response extraction and score utilities
 * Extracts scores and assessment summaries from scorer responses
 */

import { BATCH_CONFIG } from './batchConfig';

export function extractBatchScore(payload: unknown): number | null {
  const data = payload as any;
  if (!data || !data.scoringData || !data.scoringData.messageResults) {
    return null;
  }
  const score = parseFloat(data.scoringData.messageResults.piqiScore);
  return isNaN(score) ? null : Math.round(score);
}

export function summarizeAssessmentStatuses(assessments: any[]): { pass: number; fail: number; skip: number } {
  const summary = { pass: 0, fail: 0, skip: 0 };
  if (!Array.isArray(assessments)) return summary;

  const cfg = BATCH_CONFIG;
  assessments.forEach((item) => {
    const status = (item.status || '').toLowerCase();
    if (cfg.statusConfig.passStatuses.includes(status)) {
      summary.pass += 1;
    } else if (cfg.statusConfig.failStatuses.includes(status)) {
      summary.fail += 1;
    } else if (cfg.statusConfig.skipStatuses.includes(status)) {
      summary.skip += 1;
    }
  });

  return summary;
}
