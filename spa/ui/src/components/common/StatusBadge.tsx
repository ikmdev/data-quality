type StatusBadgeValue = 'PASSED' | 'FAILED' | 'SKIPPED' | 'Pass' | 'Fail' | 'Skip'

interface StatusBadgeProps {
  status: StatusBadgeValue
}

const statusStyles: Record<'PASSED' | 'FAILED' | 'SKIPPED', React.CSSProperties> = {
  PASSED: { backgroundColor: '#d4edda', color: '#155724', border: '1px solid #c3e6cb' },
  FAILED: { backgroundColor: '#f8d7da', color: '#721c24', border: '1px solid #f5c6cb' },
  SKIPPED: { backgroundColor: '#fff3cd', color: '#856404', border: '1px solid #ffeeba' },
}

function normalizeStatus(status: StatusBadgeValue): 'PASSED' | 'FAILED' | 'SKIPPED' {
  if (status === 'Pass') return 'PASSED'
  if (status === 'Fail') return 'FAILED'
  if (status === 'Skip') return 'SKIPPED'
  return status
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  const normalizedStatus = normalizeStatus(status)

  return (
    <span style={{
      ...statusStyles[normalizedStatus],
      padding: '2px 10px',
      borderRadius: '4px',
      fontSize: '12px',
      fontWeight: 700,
      letterSpacing: '0.3px',
    }}>
      {normalizedStatus}
    </span>
  )
}
