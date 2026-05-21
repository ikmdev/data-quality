interface ChecksCountCardProps {
  label: string
  value: number
  bg: string
  color: string
  border: string
}

export default function ChecksCountCard({ label, value, bg, color, border }: ChecksCountCardProps) {
  return (
    <div style={{
      background: bg,
      border: `1px solid ${border}`,
      borderRadius: '8px',
      padding: '20px',
      textAlign: 'center',
      flex: 1,
    }}>
      <div style={{ fontSize: '11px', fontWeight: 700, color, textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: '10px' }}>
        {label}
      </div>
      <div style={{ fontSize: '32px', fontWeight: 700, color: '#222' }}>
        {value}
      </div>
    </div>
  )
}
