import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import AppShell from '../components/layout/AppShell'
import SubmitMessagePage from '../pages/SubmitMessagePage'
import RunDashboardPage from '../pages/RunDashboardPage'
import { BatchApiProvider } from './hooks/useBatchApi'

export default function App() {
  return (
    <BrowserRouter>
      <BatchApiProvider>
        <AppShell>
          <Routes>
            <Route path="/" element={<SubmitMessagePage />} />
            <Route path="/batch" element={<Navigate to="/" replace />} />
            <Route path="/run/:runId" element={<RunDashboardPage />} />
            <Route path="/run/:runId/checks" element={<Navigate to="/" replace />} />
            {/* Demo shortcut */}
            <Route path="/demo" element={<Navigate to="/run/batch-1778001303781-l0tmey" replace />} />
          </Routes>
        </AppShell>
      </BatchApiProvider>
    </BrowserRouter>
  )
}
