import { useState, useRef } from 'react'
import { usePiqiApi } from '../app/hooks/usePiqiApi'
import { useBatchApi } from '../app/hooks/useBatchApi'

interface ScoreResult {
  denominator?: number
  numerator?: number
  piqiScore?: number
  criticalFailureCount?: number
  weightedDenominator?: number
  weightedNumerator?: number
  weightedPIQIScore?: number
}

interface ScoringData {
  messageResults?: ScoreResult
}

interface PiqiApiResponse {
  succeeded?: boolean
  errorMessage?: string
  scoringData?: ScoringData
  auditedMessage?: unknown
  httpStatus?: number
}

interface AssessmentItem {
  attributeName: string
  attributeValue: string
  rawAttributeValue: string
  assessment: string
  status: string
  reason: string
  effect: string
}

function parseAuditedMessage(raw: unknown): Record<string, unknown> | null {
  if (!raw) {
    return null
  }

  if (typeof raw === 'object') {
    return raw as Record<string, unknown>
  }

  if (typeof raw === 'string') {
    try {
      return JSON.parse(raw) as Record<string, unknown>
    } catch {
      return null
    }
  }

  return null
}

function getClassFieldPath(className: string): string | null {
  const mapping: Record<string, string> = {
    'Lab Results': 'labResults',
    Medications: 'medications',
    Allergies: 'allergies',
    Conditions: 'conditions',
    Procedures: 'procedures',
    'Vital Signs': 'vitalSigns',
    Immunizations: 'immunizations',
    Demographics: 'demographics',
    Encounters: 'encounters',
    Providers: 'providers',
    'Clinical Documents': 'clinicalDocuments',
    'Diagnostic Imaging': 'diagnosticImaging',
    Goals: 'goals',
    'Health Assessments': 'healthAssessments',
    'Medical Devices': 'medicalDevices',
  }

  return mapping[className] ?? null
}

function safeJsonParse(input: string): unknown {
  const trimmed = input.trim()
  if (!trimmed) {
    return null
  }

  const looksJson = (
    (trimmed.startsWith('{') && trimmed.endsWith('}'))
    || (trimmed.startsWith('[') && trimmed.endsWith(']'))
  )

  if (!looksJson) {
    return null
  }

  try {
    return JSON.parse(trimmed)
  } catch {
    return null
  }
}

function firstCodingValue(data: Record<string, unknown> | null): string | null {
  if (!data) {
    return null
  }

  const codings = Array.isArray(data.codings) ? data.codings : []
  if (!codings.length || typeof codings[0] !== 'object' || !codings[0]) {
    return null
  }

  const coding = codings[0] as Record<string, unknown>
  const candidate = coding.code ?? coding.display ?? coding.system
  return typeof candidate === 'string' ? candidate : null
}

function simplifyAttributeValue(attributeName: string, dataClass: string, rawValue: unknown): string {
  if (rawValue === null || rawValue === undefined) {
    return 'N/A'
  }

  const normalizedAttribute = attributeName.toLowerCase()
  const primitive = (
    typeof rawValue === 'string'
    || typeof rawValue === 'number'
    || typeof rawValue === 'boolean'
  )
    ? String(rawValue)
    : null

  const parsed = typeof rawValue === 'object'
    ? rawValue as Record<string, unknown>
    : safeJsonParse(primitive ?? '') as Record<string, unknown> | null

  if (!parsed) {
    return primitive?.trim() || 'N/A'
  }

  if (normalizedAttribute.includes('resultvalue') || normalizedAttribute.includes('valuequantity')) {
    if (typeof parsed.text === 'string' && parsed.text.trim()) {
      return parsed.text
    }
    if (parsed.value !== undefined && parsed.value !== null) {
      return String(parsed.value)
    }
    if (parsed.lowValue !== undefined && parsed.highValue !== undefined) {
      return `${String(parsed.lowValue)} - ${String(parsed.highValue)}`
    }
  }

  if (
    normalizedAttribute.includes('resultunit')
    || normalizedAttribute.includes('unit')
    || normalizedAttribute.includes('test')
    || normalizedAttribute.includes('code')
    || normalizedAttribute.includes('specimen')
    || normalizedAttribute.includes('type')
  ) {
    const codingValue = firstCodingValue(parsed)
    if (codingValue) {
      return codingValue
    }
    if (typeof parsed.text === 'string' && parsed.text.trim()) {
      return parsed.text
    }
  }

  if (normalizedAttribute.includes('date') || normalizedAttribute.includes('time')) {
    if (typeof parsed.text === 'string' && parsed.text.trim()) {
      return parsed.text
    }
    if (parsed.value !== undefined && parsed.value !== null) {
      return String(parsed.value)
    }
  }

  if (typeof parsed.text === 'string' && parsed.text.trim()) {
    return parsed.text
  }

  const fallbackCoding = firstCodingValue(parsed)
  if (fallbackCoding) {
    return fallbackCoding
  }

  if (Array.isArray(parsed) && parsed.length > 0) {
    const first = parsed[0]
    if (typeof first === 'string' || typeof first === 'number' || typeof first === 'boolean') {
      return String(first)
    }
  }

  const keys = Object.keys(parsed)
  if (keys.length === 1) {
    const value = parsed[keys[0]]
    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      return String(value)
    }
  }

  return dataClass === 'Lab Results' ? 'Structured Value' : 'Structured Data'
}

function extractAssessmentItems(auditedMessage: Record<string, unknown> | null, className: string): AssessmentItem[] {
  if (!auditedMessage) {
    return []
  }

  // Backend returns data directly on auditedMessage OR nested under .patient
  const patient = (auditedMessage.patient as Record<string, unknown> | undefined) ?? auditedMessage

  const fieldPath = getClassFieldPath(className)
  if (!fieldPath) {
    return []
  }

  const classItems = patient[fieldPath]
  if (!Array.isArray(classItems)) {
    return []
  }

  const rows: AssessmentItem[] = []
  classItems.forEach((element) => {
    if (!element || typeof element !== 'object') {
      return
    }

    Object.entries(element as Record<string, unknown>).forEach(([key, attribute]) => {
      if (!attribute || typeof attribute !== 'object') {
        return
      }

      const attributeObject = attribute as Record<string, unknown>
      const attributeAudit = attributeObject.attributeAudit as Record<string, unknown> | undefined
      const assessmentItems = attributeAudit?.assessmentItems
      if (!Array.isArray(assessmentItems)) {
        return
      }

      assessmentItems.forEach((item) => {
        if (!item || typeof item !== 'object') {
          return
        }

        const assessment = item as Record<string, unknown>
        const attributeName = typeof assessment.attributeName === 'string' ? assessment.attributeName : key
        const valueDisplay = simplifyAttributeValue(attributeName, className, attributeObject.data)
        const rawAttributeValue = attributeObject.data === undefined || attributeObject.data === null
          ? 'N/A'
          : (typeof attributeObject.data === 'object'
            ? JSON.stringify(attributeObject.data)
            : String(attributeObject.data))

        rows.push({
          attributeName,
          attributeValue: valueDisplay,
          rawAttributeValue,
          assessment: typeof assessment.assessment === 'string' ? assessment.assessment : '-',
          status: typeof assessment.status === 'string' ? assessment.status : 'Unknown',
          reason: typeof assessment.reason === 'string' && assessment.reason.trim() ? assessment.reason : '-',
          effect: typeof assessment.effect === 'string' && assessment.effect.trim() ? assessment.effect : '-',
        })
      })
    })
  })

  return rows
}

function statusClassName(status: string): string {
  const normalized = status.toLowerCase()
  if (normalized === 'passed') {
    return 'status-pass'
  }
  if (normalized === 'skipped') {
    return 'status-skip'
  }
  return 'status-fail'
}

function displayPercent(value: number | undefined): string {
  if (typeof value !== 'number') {
    return '-'
  }
  return `${value}%`
}

function generateMessageId(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID()
  }

  return `msg-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`
}

function randomSuffix(length: number): string {
  return Math.random().toString(36).slice(2, 2 + length).toUpperCase()
}

function generateProviderId(): string {
  return `Provider-${randomSuffix(8)}`
}

function generateSourceId(): string {
  return `Source-${randomSuffix(8)}`
}

function resolveMessageId(parsedMessage: unknown): string {
  if (parsedMessage && typeof parsedMessage === 'object') {
    const candidate = (parsedMessage as Record<string, unknown>).messageID
      ?? (parsedMessage as Record<string, unknown>).messageId

    if (typeof candidate === 'string' && candidate.trim()) {
      return candidate
    }
  }

  return generateMessageId()
}

export default function SubmitMessagePage() {
  const [jsonPayload, setJsonPayload] = useState('')
  const [providerId, setProviderId] = useState(generateProviderId)
  const [sourceId, setSourceId] = useState(generateSourceId)
  const [piqiModelMnemonic, setPiqiModelMnemonic] = useState('PAT_CLINICAL_V1')
  const [evaluationRubricMnemonic, setEvaluationRubricMnemonic] = useState('USCDI_V3')
  const [facility, setFacility] = useState('Facility 1')
  const [application, setApplication] = useState('Application A')
  const [format, setFormat] = useState('JSON')
  const [useCase, setUseCase] = useState('Quality Monitoring')
  const [isAdvancedOpen, setIsAdvancedOpen] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)
  const {
    isLoading,
    errorMessage,
    response,
    submitScoreAuditMessage,
    clearApiState,
  } = usePiqiApi()
  const [showFailedOnly, setShowFailedOnly] = useState(false)

  // Batch state
  const batchFileInputRef = useRef<HTMLInputElement>(null)
  const [batchSelectedFile, setBatchSelectedFile] = useState<File | null>(null)
  const [batchProvider, setBatchProvider] = useState('Provider Alpha')
  const [batchSource, setBatchSource] = useState('Main EHR Feed')
  const [batchModel, setBatchModel] = useState('PAT_CLINICAL_V1')
  const [batchRubric, setBatchRubric] = useState('USCDI_V3')
  const {
    isProcessing,
    errorMessage: batchError,
    summary: batchSummary,
    processingStatus,
    processFile,
    clearBatch,
  } = useBatchApi()

  const handleBatchFileSelect = (file: File) => {
    if (file.name.match(/\.(xlsx?|csv)$/i)) {
      setBatchSelectedFile(file)
    } else {
      alert('Please select a valid Excel or CSV file')
    }
  }

  const handleBatchProcess = async () => {
    if (!batchSelectedFile) return
    await processFile(batchSelectedFile, batchProvider, batchSource, batchModel, batchRubric)
  }

  const handleBatchClear = () => {
    setBatchSelectedFile(null)
    if (batchFileInputRef.current) batchFileInputRef.current.value = ''
    clearBatch()
  }

  const apiEndpointUrl = `${import.meta.env.VITE_PIQI_API_BASE_URL ?? ''}/PIQI/ScoreAuditMessage`

  const handleNewSession = () => {
    setProviderId(generateProviderId())
    setSourceId(generateSourceId())
  }

  const handleClear = () => {
    setJsonPayload('')
    setValidationError(null)
    clearApiState()
    setShowFailedOnly(false)
  }

  const handleSubmit = async () => {
    setValidationError(null)
    clearApiState()

    if (!jsonPayload.trim()) {
      setValidationError('Please paste a PIQI/FHIR JSON payload before submitting.')
      return
    }

    if (!providerId.trim() || !sourceId.trim()) {
      setValidationError('Provider ID and Source ID are required.')
      return
    }

    if (!piqiModelMnemonic.trim() || !evaluationRubricMnemonic.trim()) {
      setValidationError('Model mnemonic and rubric mnemonic are required.')
      return
    }

    let parsedMessage: unknown
    try {
      parsedMessage = JSON.parse(jsonPayload)
    } catch {
      setValidationError('Invalid JSON format. Please correct the payload and try again.')
      return
    }

    const messageID = resolveMessageId(parsedMessage)

    try {
      await submitScoreAuditMessage({
        dataProviderID: providerId,
        dataSourceID: sourceId,
        messageID,
        piqiModelMnemonic,
        evaluationRubricMnemonic,
        messageData: JSON.stringify(parsedMessage),
      })
    } catch {
      // Error state is managed by the hook.
    }
  }

  const typedResponse = response as PiqiApiResponse | null
  const scoringData = typedResponse?.scoringData
  const messageResults = scoringData?.messageResults
  const auditedMessageObject = parseAuditedMessage(typedResponse?.auditedMessage)

  const DATA_CLASSES = [
    'Lab Results', 'Medications', 'Allergies', 'Conditions', 'Procedures',
    'Vital Signs', 'Immunizations', 'Demographics', 'Encounters', 'Providers',
    'Clinical Documents', 'Diagnostic Imaging', 'Goals', 'Health Assessments', 'Medical Devices',
  ]

  // Collect all assessment items across all data classes
  const allAssessmentItems: (AssessmentItem & { dataClass: string })[] = []
  DATA_CLASSES.forEach((className) => {
    extractAssessmentItems(auditedMessageObject, className).forEach((item) => {
      allAssessmentItems.push({ ...item, dataClass: className })
    })
  })

  const uniqueClasses = [...new Set(allAssessmentItems.map((i) => i.dataClass))]

  const passCount = allAssessmentItems.filter((r) => r.status.toLowerCase() === 'passed').length
  const failCount = allAssessmentItems.filter((r) => r.status.toLowerCase() === 'failed').length
  const skipCount = allAssessmentItems.filter((r) => r.status.toLowerCase() === 'skipped').length

  const visibleRows = showFailedOnly
    ? allAssessmentItems.filter((row) => row.status.toLowerCase() === 'failed')
    : allAssessmentItems

  return (
    <div className="page-container">
      <h1>Submit Message</h1>
      <section className="form-section">
        <h2>PIQI OR FHIR JSON MESSAGE</h2>
        <textarea
          placeholder="Paste JSON payload here"
          rows={12}
          className="json-input"
          value={jsonPayload}
          onChange={(event) => setJsonPayload(event.target.value)}
        />
      </section>

      <section className="form-section">
        <h2>JSON FILE UPLOAD</h2>
        <div className="drop-zone">
          <p>Drop JSON file here or click to choose</p>
        </div>
      </section>

      <div className="form-row labeled-form-row">
        <div className="labeled-input-group">
          <label htmlFor="provider-id-input" className="form-label">Provider ID</label>
          <input
            id="provider-id-input"
            type="text"
            className="form-input"
            value={providerId}
            onChange={(event) => setProviderId(event.target.value)}
          />
        </div>
        <div className="labeled-input-group">
          <label htmlFor="source-id-input" className="form-label">Source ID</label>
          <input
            id="source-id-input"
            type="text"
            className="form-input"
            value={sourceId}
            onChange={(event) => setSourceId(event.target.value)}
          />
        </div>
      </div>

      <section className="form-section advanced-settings-section">
        <div
          className="advanced-settings-toggle"
          role="button"
          tabIndex={0}
          onClick={() => setIsAdvancedOpen((current) => !current)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault()
              setIsAdvancedOpen((current) => !current)
            }
          }}
          aria-expanded={isAdvancedOpen}
          aria-controls="advanced-settings-body"
        >
          <h3 className="advanced-settings-title">Advanced Settings</h3>
          <span className="advanced-settings-toggle-right">
            <span className={`advanced-settings-arrow ${isAdvancedOpen ? 'open' : ''}`}>▾</span>
          </span>
        </div>

        {isAdvancedOpen && (
          <div id="advanced-settings-body" className="advanced-settings-body">
            <div className="form-row labeled-form-row">
              <div className="labeled-input-group">
                <label htmlFor="api-endpoint-input" className="form-label">API Endpoint URL</label>
                <input id="api-endpoint-input" type="text" className="form-input" value={apiEndpointUrl} readOnly />
              </div>
              <div className="labeled-input-group">
                <label htmlFor="model-mnemonic-input" className="form-label">Model Mnemonic</label>
                <input
                  id="model-mnemonic-input"
                  type="text"
                  className="form-input"
                  value={piqiModelMnemonic}
                  onChange={(event) => setPiqiModelMnemonic(event.target.value)}
                />
              </div>
            </div>

            <div className="form-row labeled-form-row">
              <div className="labeled-input-group">
                <label htmlFor="rubric-mnemonic-input" className="form-label">Rubric Mnemonic</label>
                <input
                  id="rubric-mnemonic-input"
                  type="text"
                  className="form-input"
                  value={evaluationRubricMnemonic}
                  onChange={(event) => setEvaluationRubricMnemonic(event.target.value)}
                />
              </div>
              <div className="labeled-input-group">
                <label htmlFor="facility-input" className="form-label">Facility</label>
                <input
                  id="facility-input"
                  type="text"
                  className="form-input"
                  value={facility}
                  onChange={(event) => setFacility(event.target.value)}
                />
              </div>
            </div>

            <div className="form-row labeled-form-row">
              <div className="labeled-input-group">
                <label htmlFor="application-input" className="form-label">Application</label>
                <input
                  id="application-input"
                  type="text"
                  className="form-input"
                  value={application}
                  onChange={(event) => setApplication(event.target.value)}
                />
              </div>
              <div className="labeled-input-group">
                <label htmlFor="format-input" className="form-label">Format</label>
                <input
                  id="format-input"
                  type="text"
                  className="form-input"
                  value={format}
                  onChange={(event) => setFormat(event.target.value)}
                />
              </div>
            </div>

            <div className="labeled-input-group">
              <label htmlFor="use-case-input" className="form-label">Use Case</label>
              <input
                id="use-case-input"
                type="text"
                className="form-input"
                value={useCase}
                onChange={(event) => setUseCase(event.target.value)}
              />
            </div>
          </div>
        )}
      </section>

      <div className="button-group">
        <button className="btn btn-primary" onClick={handleSubmit} disabled={isLoading}>
          {isLoading ? 'Submitting...' : 'Submit'}
        </button>
        <button className="btn btn-secondary" onClick={handleClear} disabled={isLoading}>Clear</button>
        <button className="btn btn-secondary" onClick={handleNewSession} disabled={isLoading}>New Session</button>
      </div>

      {(validationError || errorMessage) && (
        <section className="form-section">
          <p className="message-error">{validationError ?? errorMessage}</p>
        </section>
      )}

      {typedResponse && typedResponse.succeeded && (
        <section className="success-panel">
          <div className="success-banner">
            <strong>Success (Status: {typedResponse.httpStatus ?? 200})</strong>
          </div>

          <section className="results-card">
            <h3>Message Results</h3>
            <div className="metric-grid">
              <article className="metric-box"><div className="metric-label">Denominator</div><div className="metric-value">{messageResults?.denominator ?? '-'}</div></article>
              <article className="metric-box"><div className="metric-label">Numerator</div><div className="metric-value">{messageResults?.numerator ?? '-'}</div></article>
              <article className="metric-box"><div className="metric-label">PIQI Score</div><div className="metric-value">{displayPercent(messageResults?.piqiScore)}</div></article>
              <article className="metric-box"><div className="metric-label">Critical Failure Count</div><div className="metric-value">{messageResults?.criticalFailureCount ?? '-'}</div></article>
              <article className="metric-box"><div className="metric-label">Weighted Denominator</div><div className="metric-value">{messageResults?.weightedDenominator ?? '-'}</div></article>
              <article className="metric-box"><div className="metric-label">Weighted Numerator</div><div className="metric-value">{messageResults?.weightedNumerator ?? '-'}</div></article>
              <article className="metric-box"><div className="metric-label">Weighted PIQI Score</div><div className="metric-value">{displayPercent(messageResults?.weightedPIQIScore)}</div></article>
            </div>
          </section>

          <section className="results-card">
            <h2 style={{ marginBottom: '16px' }}>
              Assessment Checks
              <span style={{ fontSize: '14px', fontWeight: 400, color: '#666', marginLeft: '8px' }}>
                {uniqueClasses.length} classes · {allAssessmentItems.length} checks
              </span>
            </h2>

            {/* Filter button */}
            {allAssessmentItems.length > 0 && (
              <>
                <div style={{ marginBottom: '16px' }}>
                  <button className="btn btn-secondary" onClick={() => setShowFailedOnly((current) => !current)}>
                    {showFailedOnly ? 'Show All' : 'Show FAILED Only'}
                  </button>
                </div>

                {/* KPI boxes */}
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '12px', marginBottom: '16px' }}>
                  <div style={{ padding: '12px', backgroundColor: '#f9f9f9', borderRadius: '6px', textAlign: 'center' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>AVG SCORE</div>
                    <div style={{ fontSize: '20px', fontWeight: 700 }}>{displayPercent(messageResults?.piqiScore)}</div>
                  </div>
                  <div style={{ padding: '12px', backgroundColor: '#f0fdf4', borderRadius: '6px', textAlign: 'center' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>PASS</div>
                    <div style={{ fontSize: '20px', fontWeight: 700, color: '#16a34a' }}>{passCount}</div>
                  </div>
                  <div style={{ padding: '12px', backgroundColor: '#fef2f2', borderRadius: '6px', textAlign: 'center' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>FAIL</div>
                    <div style={{ fontSize: '20px', fontWeight: 700, color: '#dc2626' }}>{failCount}</div>
                  </div>
                  <div style={{ padding: '12px', backgroundColor: '#fffbeb', borderRadius: '6px', textAlign: 'center' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>SKIP</div>
                    <div style={{ fontSize: '20px', fontWeight: 700, color: '#d97706' }}>{skipCount}</div>
                  </div>
                </div>

                {/* Assessment table */}
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                      <tr style={{ borderBottom: '2px solid #ddd', backgroundColor: '#f9f9f9' }}>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>DATACLASS</th>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>ATTRIBUTE NAME</th>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>ATTRIBUTE VALUE</th>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>ASSESSMENT</th>
                        <th style={{ padding: '12px', textAlign: 'center', fontWeight: 600, fontSize: '12px' }}>STATUS</th>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>REASON</th>
                        <th style={{ padding: '12px', textAlign: 'left', fontWeight: 600, fontSize: '12px' }}>EFFECT</th>
                      </tr>
                    </thead>
                    <tbody>
                      {visibleRows.map((row, index) => (
                        <tr
                          key={`${row.attributeName}-${row.assessment}-${index}`}
                          style={{
                            borderBottom: '1px solid #eee',
                            backgroundColor:
                              row.status.toLowerCase() === 'passed' ? '#f0fdf4'
                              : row.status.toLowerCase() === 'failed' ? '#fef2f2'
                              : '#fffbeb',
                          }}
                        >
                          <td style={{ padding: '12px', fontSize: '13px' }}>{row.dataClass}</td>
                          <td style={{ padding: '12px', fontSize: '13px' }}>{row.attributeName}</td>
                          <td style={{ padding: '12px', fontSize: '13px', maxWidth: '150px', wordBreak: 'break-word' }} title={row.rawAttributeValue}>{row.attributeValue}</td>
                          <td style={{ padding: '12px', fontSize: '13px' }}>{row.assessment}</td>
                          <td style={{ padding: '12px', textAlign: 'center' }}>
                            <span className={`status-pill ${statusClassName(row.status)}`}>{row.status}</span>
                          </td>
                          <td style={{ padding: '12px', fontSize: '13px', maxWidth: '200px', wordBreak: 'break-word' }}><em>{row.reason}</em></td>
                          <td style={{ padding: '12px', fontSize: '13px' }}>{row.effect}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )}

            {allAssessmentItems.length === 0 && (
              <p className="results-note">No assessment items found in the audited message.</p>
            )}
          </section>

          <section className="results-card">
            <details>
              <summary><strong>Audited Message</strong></summary>
              <pre className="response-output">{JSON.stringify(auditedMessageObject ?? typedResponse.auditedMessage ?? {}, null, 2)}</pre>
            </details>
          </section>
        </section>
      )}

      {typedResponse && !typedResponse.succeeded && (
        <section className="form-section">
          <p className="message-error">{typedResponse.errorMessage || 'PIQI API returned an unsuccessful response.'}</p>
          <pre className="response-output">{JSON.stringify(typedResponse, null, 2)}</pre>
        </section>
      )}

      {/* ── Batch Upload Section ─────────────────────────────────────── */}
      <section className="form-section" style={{ marginTop: '32px' }}>
        <h2>Batch Upload</h2>

        {/* Drop zone */}
        <div
          className="drop-zone"
          style={{ cursor: 'pointer' }}
          onClick={() => batchFileInputRef.current?.click()}
          onDragOver={(e) => { e.preventDefault(); (e.currentTarget as HTMLDivElement).style.backgroundColor = '#e0e0e0' }}
          onDragLeave={(e) => { (e.currentTarget as HTMLDivElement).style.backgroundColor = '' }}
          onDrop={(e) => {
            e.preventDefault();
            (e.currentTarget as HTMLDivElement).style.backgroundColor = '';
            if (e.dataTransfer.files[0]) handleBatchFileSelect(e.dataTransfer.files[0])
          }}
        >
          {batchSelectedFile
            ? <><p style={{ fontWeight: 600 }}>📄 {batchSelectedFile.name}</p><p style={{ fontSize: '12px', color: '#666' }}>Click to change file</p></>
            : <><p>Drop Excel/CSV file here or click to choose</p><p style={{ fontSize: '12px', color: '#666' }}>Supported: .xlsx, .xls, .csv</p></>}
          <input ref={batchFileInputRef} type="file" accept=".xlsx,.xls,.csv" style={{ display: 'none' }}
            onChange={(e) => { if (e.target.files?.[0]) handleBatchFileSelect(e.target.files[0]) }} />
        </div>

        {/* Action buttons */}
        <div className="button-group" style={{ marginTop: '12px' }}>
          <button className="btn btn-primary" onClick={handleBatchProcess} disabled={isProcessing || !batchSelectedFile}>
            {isProcessing ? 'Processing...' : 'Process Batch'}
          </button>
          <button className="btn btn-secondary" onClick={handleBatchClear} disabled={isProcessing}>Clear Batch</button>
        </div>

        {/* Error */}
        {batchError && (
          <div style={{ color: 'red', marginTop: '12px', padding: '12px', backgroundColor: '#fee2e2' }}>
            <strong>Error:</strong> {batchError}
          </div>
        )}

        {/* Status */}
        {processingStatus && (
          <div style={{ marginTop: '12px', padding: '12px', backgroundColor: '#e3f2fd', color: '#1976d2' }}>
            {processingStatus}
          </div>
        )}

        {/* Summary */}
        {batchSummary && (
          <div style={{ marginTop: '16px' }}>
            {/* Row 1: Run ID + Elapsed */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '12px' }}>
              <article className="metric-box"><div className="metric-label">Run ID</div><div style={{ fontSize: '13px', fontWeight: 600, wordBreak: 'break-all', marginTop: '4px' }}>{batchSummary.batchRunId}</div></article>
              <article className="metric-box"><div className="metric-label">Elapsed</div><div className="metric-value">{(batchSummary.elapsedMs / 1000).toFixed(1)}s</div></article>
            </div>
            {/* Row 2: Rate + Success Rate */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '12px' }}>
              <article className="metric-box"><div className="metric-label">Rate</div><div className="metric-value">{batchSummary.processingRate}</div></article>
              <article className="metric-box"><div className="metric-label">Success Rate</div><div className="metric-value">{batchSummary.successRate}</div></article>
            </div>
            {/* Row 3: Total Rows / Processed / Succeeded / Failed / Average */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5,1fr)', gap: '12px', marginBottom: '12px' }}>
              <article className="metric-box"><div className="metric-label">Total Rows</div><div className="metric-value">{batchSummary.totalRows}</div></article>
              <article className="metric-box"><div className="metric-label">Processed</div><div className="metric-value">{batchSummary.processedRows}</div></article>
              <article className="metric-box"><div className="metric-label">Succeeded</div><div className="metric-value" style={{ color: '#059669' }}>{batchSummary.succeededRows}</div></article>
              <article className="metric-box"><div className="metric-label">Failed</div><div className="metric-value" style={{ color: '#dc2626' }}>{batchSummary.failedRows}</div></article>
              <article className="metric-box"><div className="metric-label">Average</div><div className="metric-value">{batchSummary.averageScore}</div></article>
            </div>
            {/* Row 4: Passed / Failed / Skipped checks */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: '12px', marginBottom: '12px' }}>
              <article className="metric-box" style={{ backgroundColor: '#f0fdf4', border: '1px solid #bbf7d0' }}>
                <div className="metric-label" style={{ color: '#15803d' }}>Passed Checks</div>
                <div className="metric-value" style={{ color: '#15803d' }}>{batchSummary.classBreakdown.pass}</div>
              </article>
              <article className="metric-box" style={{ backgroundColor: '#fef2f2', border: '1px solid #fecaca' }}>
                <div className="metric-label" style={{ color: '#dc2626' }}>Failed Checks</div>
                <div className="metric-value" style={{ color: '#dc2626' }}>{batchSummary.classBreakdown.fail}</div>
              </article>
              <article className="metric-box" style={{ backgroundColor: '#fffbeb', border: '1px solid #fde68a' }}>
                <div className="metric-label" style={{ color: '#d97706' }}>Skipped Checks</div>
                <div className="metric-value" style={{ color: '#d97706' }}>{batchSummary.classBreakdown.skip}</div>
              </article>
            </div>
            {/* Summary line */}
            <div style={{ padding: '10px 14px', backgroundColor: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: '8px', fontSize: '13px', color: '#475569' }}>
              {batchSummary.processedRows} of {batchSummary.totalRows} rows processed. Checks: {batchSummary.classBreakdown.pass} passed, {batchSummary.classBreakdown.fail} failed, {batchSummary.classBreakdown.skip} skipped.
            </div>
          </div>
        )}
      </section>
    </div>
  )
}
