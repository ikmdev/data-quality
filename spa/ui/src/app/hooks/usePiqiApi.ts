import { useCallback, useState } from 'react'
import { scoreAuditMessage, scoreMessage, type PiqiRequest, type PiqiResponse } from '../api/piqiClient'

interface UsePiqiApiResult {
  isLoading: boolean
  errorMessage: string | null
  response: PiqiResponse | null
  submitScoreMessage: (payload: PiqiRequest) => Promise<PiqiResponse>
  submitScoreAuditMessage: (payload: PiqiRequest) => Promise<PiqiResponse>
  clearApiState: () => void
}

export function usePiqiApi(): UsePiqiApiResult {
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [response, setResponse] = useState<PiqiResponse | null>(null)

  const clearApiState = useCallback(() => {
    setErrorMessage(null)
    setResponse(null)
  }, [])

  const runApiCall = useCallback(async (request: PiqiRequest, useAuditEndpoint: boolean) => {
    setIsLoading(true)
    setErrorMessage(null)
    setResponse(null)

    try {
      const result = useAuditEndpoint
        ? await scoreAuditMessage(request)
        : await scoreMessage(request)

      setResponse(result)
      return result
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unexpected error while calling PIQI API.'
      setErrorMessage(message)
      throw error
    } finally {
      setIsLoading(false)
    }
  }, [])

  const submitScoreMessage = useCallback((payload: PiqiRequest) => {
    return runApiCall(payload, false)
  }, [runApiCall])

  const submitScoreAuditMessage = useCallback((payload: PiqiRequest) => {
    return runApiCall(payload, true)
  }, [runApiCall])

  return {
    isLoading,
    errorMessage,
    response,
    submitScoreMessage,
    submitScoreAuditMessage,
    clearApiState,
  }
}
