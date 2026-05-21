import { ReactNode } from 'react'
import { Link } from 'react-router-dom'

interface AppShellProps {
  children: ReactNode
}

export default function AppShell({ children }: AppShellProps) {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="header-content">
          <h1 className="app-title">Data Quality</h1>
          <nav className="nav-links">
            <Link to="/">Submit</Link>
          </nav>
        </div>
        <div className="api-status">
          <span className="status-badge status-online">API Online</span>
        </div>
      </header>

      <main className="app-main">
        {children}
      </main>

      <footer className="app-footer">
        <p>&copy; 2026 PIQI Data Quality. All rights reserved.</p>
      </footer>
    </div>
  )
}
