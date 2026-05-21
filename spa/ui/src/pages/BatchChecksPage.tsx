import { useState, useMemo } from 'react'
import StatusBadge from '../components/common/StatusBadge'
import { useBatchApi } from '../app/hooks/useBatchApi'

type FilterMode = 'all' | 'fail-skip' | 'pass-only'

const PAGE_SIZE = 10


export default function BatchChecksPage() {
  const [filter, setFilter] = useState<FilterMode>('all');
  const [page, setPage] = useState(1);
  const { assessmentItems } = useBatchApi();
  const uniqueMessages = new Set(assessmentItems.map(c => c.messageId)).size;

  const filtered = useMemo(() => {
    if (filter === 'fail-skip') return assessmentItems.filter(c => c.status === 'Fail' || c.status === 'Skip');
    if (filter === 'pass-only') return assessmentItems.filter(c => c.status === 'Pass');
    return assessmentItems;
  }, [filter, assessmentItems]);

  const totalPages = Math.ceil(filtered.length / PAGE_SIZE);
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const passed = assessmentItems.filter(c => c.status === 'Pass').length;
  const failed = assessmentItems.filter(c => c.status === 'Fail').length;
  const skipped = assessmentItems.filter(c => c.status === 'Skip').length;

  function handleFilter(mode: FilterMode) {
    setFilter(mode);
    setPage(1);
  }

  return (
    <div className="page-container">
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2 style={{ margin: 0 }}>Batch checks</h2>
          <p style={{ margin: 0, fontSize: '13px', color: '#666' }}>
            {uniqueMessages} messages · {assessmentItems.length} checks.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '16px', fontSize: '13px', fontWeight: 600 }}>
          <span style={{ color: '#28a745' }}>● Pass</span>
          <span style={{ color: '#dc3545' }}>● Fail</span>
          <span style={{ color: '#ffc107' }}>● Skip</span>
        </div>
      </div>

      {/* Filter buttons */}
      <div style={{ display: 'flex', gap: '8px' }}>
        {(['all', 'fail-skip', 'pass-only'] as FilterMode[]).map(mode => (
          <button
            key={mode}
            onClick={() => handleFilter(mode)}
            className={filter === mode ? 'btn btn-primary' : 'btn btn-secondary'}
            style={{ fontSize: '13px' }}
          >
            {mode === 'all' ? 'All' : mode === 'fail-skip' ? 'Fail + Skip' : 'Pass only'}
          </button>
        ))}
      </div>

      {/* Summary row */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: '1px', border: '1px solid #e0e0e0', borderRadius: '6px', overflow: 'hidden' }}>
        {[
          { label: 'AVG SCORE', value: '-', color: '#333' },
          { label: 'PASS', value: passed, color: '#28a745' },
          { label: 'FAIL', value: failed, color: '#dc3545' },
          { label: 'SKIP', value: skipped, color: '#856404' },
        ].map(item => (
          <div key={item.label} style={{ background: 'white', padding: '14px 16px' }}>
            <div style={{ fontSize: '11px', color: '#888', fontWeight: 600, textTransform: 'uppercase', marginBottom: '4px' }}>{item.label}</div>
            <div style={{ fontSize: '20px', fontWeight: 700, color: item.color }}>{item.value}</div>
          </div>
        ))}
      </div>

      {/* Table */}
      <div style={{ background: 'white', border: '1px solid #e0e0e0', borderRadius: '6px', overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #e0e0e0', background: '#fafafa' }}>
              {['MESSAGEID', 'DATACLASS', 'ATTRIBUTENAME', 'ATTRIBUTEVALUE', 'ASSESSMENT', 'STATUS', 'REASON', 'EFFECT'].map(col => (
                <th key={col} style={{ textAlign: 'left', padding: '10px 12px', fontWeight: 600, fontSize: '11px', color: '#555', letterSpacing: '0.4px' }}>
                  {col}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {paged.length === 0 ? (
              <tr>
                <td colSpan={8} style={{ padding: '20px', textAlign: 'center', color: '#888' }}>
                  No checks to display.
                </td>
              </tr>
            ) : (
              paged.map((row, i) => (
                <tr
                  key={i}
                  style={{
                    borderBottom: '1px solid #f0f0f0',
                    background: row.status === 'Fail' ? '#fff8f8' : row.status === 'Pass' ? '#f8fff8' : '#fffdf0',
                  }}
                >
                  <td style={{ padding: '10px 12px', fontWeight: 500 }}>{row.messageId}</td>
                  <td style={{ padding: '10px 12px' }}>{row.dataClass}</td>
                  <td style={{ padding: '10px 12px' }}>{row.attributeName}</td>
                  <td style={{ padding: '10px 12px' }}>{row.attributeValue}</td>
                  <td style={{ padding: '10px 12px' }}>{row.assessment}</td>
                  <td style={{ padding: '10px 12px' }}><StatusBadge status={row.status} /></td>
                  <td style={{ padding: '10px 12px', color: '#666' }}>{row.reason}</td>
                  <td style={{ padding: '10px 12px' }}>{row.effect}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px', padding: '12px 16px', borderTop: '1px solid #f0f0f0' }}>
            <button className="btn btn-secondary" disabled={page === 1} onClick={() => setPage(p => p - 1)} style={{ fontSize: '12px', padding: '4px 12px' }}>Prev</button>
            <span style={{ fontSize: '13px', alignSelf: 'center' }}>Page {page} of {totalPages}</span>
            <button className="btn btn-secondary" disabled={page === totalPages} onClick={() => setPage(p => p + 1)} style={{ fontSize: '12px', padding: '4px 12px' }}>Next</button>
          </div>
        )}
      </div>
    </div>
  )
}
