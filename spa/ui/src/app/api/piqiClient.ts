export interface PiqiRequest {
  dataProviderID: string
  dataSourceID: string
  messageID?: string
  piqiModelMnemonic: string
  evaluationRubricMnemonic: string
  messageData: string
}

export interface PiqiResponse {
  succeeded?: boolean
  httpStatus?: number
  [key: string]: unknown
}

export interface BatchAssessmentItemPayload {
  messageId: string
  rowNum: number
  dataClass: string
  attributeName: string
  attributeValue: string
  assessment: string
  status: 'Pass' | 'Fail' | 'Skip'
  reason: string
  effect: string
}

export interface BatchAssessmentPersistRequest {
  batchRunId: string
  items: BatchAssessmentItemPayload[]
}

export interface BatchSubmitRequest {
  rows: Array<Record<string, unknown>>
  provider: string
  source: string
  model: string
  rubric: string
}

export interface BatchSummary {
  batchRunId: string
  totalRows: number
  processedRows: number
  succeededRows: number
  failedRows: number
  averageScore: string
  classBreakdown: { pass: number; fail: number; skip: number }
  elapsedMs: number
  successRate: string
  processingRate: string
  completedAt?: string
}

export interface BatchAssessmentItem {
  messageId: string
  rowNum: number
  dataClass: string
  attributeName: string
  attributeValue: string
  rawAttributeValue?: string
  assessment: string
  status: 'Pass' | 'Fail' | 'Skip'
  reason: string
  effect: string
}

export interface BatchJobSubmitResponse extends PiqiResponse {
  succeeded: boolean
  jobId: string
  batchRunId: string
  message?: string
}

export interface BatchJobStatusResponse extends PiqiResponse {
  succeeded: boolean
  jobId: string
  state: 'waiting' | 'active' | 'completed' | 'failed' | 'delayed' | 'paused' | 'stuck'
  progress: number | object
  isCompleted: boolean
  isFailed: boolean
  result: {
    summary: BatchSummary
    assessmentItems: BatchAssessmentItem[]
  } | null
  error: string | null
}

const apiBaseUrl = import.meta.env.VITE_PIQI_API_BASE_URL as string | undefined

function getApiBaseUrl(): string {
  if (!apiBaseUrl || !apiBaseUrl.trim()) {
    throw new Error('Missing VITE_PIQI_API_BASE_URL. Add it to your .env file.')
  }

  return apiBaseUrl.replace(/\/$/, '')
}

async function postPiqiRequest(
  path: 'ScoreMessage' | 'ScoreAuditMessage' | 'BatchAssessmentResults' | 'SubmitBatchJob',
  payload: PiqiRequest | BatchAssessmentPersistRequest | BatchSubmitRequest
): Promise<PiqiResponse> {
  let response: Response
  try {
    response = await fetch(`${getApiBaseUrl()}/api/${path}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })
  } catch (error) {
    const details = error instanceof Error ? ` ${error.message}` : ''
    throw new Error(`Unable to reach PIQI API at ${getApiBaseUrl()}. Check that the backend is running and CORS allows this origin.${details}`)
  }

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`PIQI API call failed (${response.status}): ${errorText || response.statusText}`)
  }

  const responseBody = (await response.json()) as PiqiResponse
  return {
    ...responseBody,
    httpStatus: response.status,
  }
}

async function getPiqiRequest<TResponse>(path: string): Promise<TResponse> {
  let response: Response
  try {
    response = await fetch(`${getApiBaseUrl()}/api/${path}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
  } catch (error) {
    const details = error instanceof Error ? ` ${error.message}` : ''
    throw new Error(`Unable to reach PIQI API at ${getApiBaseUrl()}. Check that the backend is running and CORS allows this origin.${details}`)
  }

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`PIQI API call failed (${response.status}): ${errorText || response.statusText}`)
  }

  return (await response.json()) as TResponse
}

export function scoreMessage(payload: PiqiRequest): Promise<PiqiResponse> {
  return postPiqiRequest('ScoreMessage', payload)
}

export function scoreAuditMessage(payload: PiqiRequest): Promise<PiqiResponse> {
  return postPiqiRequest('ScoreAuditMessage', payload)
}

export function persistBatchAssessmentResults(payload: BatchAssessmentPersistRequest): Promise<PiqiResponse> {
  return postPiqiRequest('BatchAssessmentResults', payload)
}

export async function submitBatchJob(payload: BatchSubmitRequest): Promise<BatchJobSubmitResponse> {
  return (await postPiqiRequest('SubmitBatchJob', payload)) as BatchJobSubmitResponse
}

export function getBatchJobStatus(jobId: string): Promise<BatchJobStatusResponse> {
  return getPiqiRequest<BatchJobStatusResponse>(`BatchJobStatus/${encodeURIComponent(jobId)}`)
}
