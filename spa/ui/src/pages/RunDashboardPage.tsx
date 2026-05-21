import { useParams } from 'react-router-dom'
import KpiCard from '../components/common/KpiCard'
import ChecksCountCard from '../components/common/ChecksCountCard'
import { useBatchApi } from '../app/hooks/useBatchApi'

export default function RunDashboardPage() {
  const { runId } = useParams()
  const { summary } = useBatchApi()
  const run = {
    runId: summary?.batchRunId ?? (runId ?? 'N/A'),
    elapsed: summary ? `${(summary.elapsedMs / 1000).toFixed(1)}s` : 'N/A',
    rate: summary?.processingRate ?? 'N/A',
    successRate: summary?.successRate ?? '0%',
    totalRows: summary?.totalRows ?? 0,
    processed: summary?.processedRows ?? 0,
    succeeded: summary?.succeededRows ?? 0,
    failed: summary?.failedRows ?? 0,
    average: summary?.averageScore ?? 'N/A',
    passedChecks: summary?.classBreakdown.pass ?? 0,
    failedChecks: summary?.classBreakdown.fail ?? 0,
    skippedChecks: summary?.classBreakdown.skip ?? 0,
    summary:
      summary
        ? `${summary.processedRows} of ${summary.totalRows} rows processed. Checks: ${summary.classBreakdown.pass} passed, ${summary.classBreakdown.fail} failed, ${summary.classBreakdown.skip} skipped.`
        : 'No batch has been processed yet. Upload and process a file to see run details.',
  }

  return (
    <div className="page-container">
      {/* Progress bar */}
      <div style={{ height: '6px', background: 'linear-gradient(to right, #0066cc, #00c6a2)', borderRadius: '3px' }} />

      {/* Run ID + Elapsed */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <KpiCard label="Run ID" value={runId ?? run.runId} accent="#0066cc" />
        <KpiCard label="Elapsed" value={run.elapsed} accent="#9c27b0" />
      </div>

      {/* Rate + Success Rate */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <KpiCard label="Rate" value={run.rate} accent="#28a745" />
        <KpiCard label="Success Rate" value={run.successRate} accent="#ff9800" />
      </div>

      {/* Row counts */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '12px' }}>
        <KpiCard label="Total Rows" value={run.totalRows} accent="#0066cc" />
        <KpiCard label="Processed" value={run.processed} accent="#28a745" />
        <KpiCard label="Succeeded" value={run.succeeded} accent="#28a745" />
        <KpiCard label="Failed" value={run.failed} accent="#dc3545" />
        <KpiCard label="Average" value={run.average} accent="#9c27b0" />
      </div>

      {/* Check totals */}
      <div style={{ display: 'flex', gap: '16px' }}>
        <ChecksCountCard label="Passed Checks" value={run.passedChecks} bg="#f0fff4" color="#28a745" border="#c3e6cb" />
        <ChecksCountCard label="Failed Checks" value={run.failedChecks} bg="#fff5f5" color="#dc3545" border="#f5c6cb" />
        <ChecksCountCard label="Skipped Checks" value={run.skippedChecks} bg="#fffbf0" color="#856404" border="#ffeeba" />
      </div>

      {/* Summary text */}
      <div style={{ background: 'white', border: '1px solid #e0e0e0', borderRadius: '6px', padding: '12px 16px', fontSize: '13px', color: '#555' }}>
        {run.summary}
      </div>
    </div>
  )
}
