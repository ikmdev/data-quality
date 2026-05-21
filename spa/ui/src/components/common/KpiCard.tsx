interface KpiCardProps {
  label: string
  value: string | number
  accent?: string
  fullWidth?: boolean
}

export default function KpiCard({ label, value, accent = '#0066cc', fullWidth = false }: KpiCardProps) {
  return (
    <div style={{
      background: 'white',
      border: '1px solid #e0e0e0',
      borderTop: `3px solid ${accent}`,
      borderRadius: '6px',
      padding: '16px',
      gridColumn: fullWidth ? '1 / -1' : undefined,
    }}>
      <div style={{ fontSize: '11px', fontWeight: 600, color: '#888', textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: '8px' }}>
        {label}
      </div>
      <div style={{ fontSize: '22px', fontWeight: 700, color: '#222' }}>
        {value}
      </div>
    </div>
  )
}
