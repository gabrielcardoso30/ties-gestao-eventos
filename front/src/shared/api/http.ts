import { authStore } from '@/shared/auth/auth-store'
import { ApiError } from './api-error'
import type { ProblemDetails } from './types'

export { ApiError }

/** Base relativa: em dev o Vite faz proxy de /api; em produção o nginx faz proxy reverso para a API. */
export const API_BASE = '/api/v1'

export type QueryParams = Record<string, string | number | boolean | null | undefined>

export interface HttpOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  query?: QueryParams
  signal?: AbortSignal
  /** Quando `true`, um 401 não encerra a sessão (usado no login). */
  ignorarNaoAutenticado?: boolean
}

function montarQuery(query?: QueryParams): string {
  if (!query) return ''
  const params = new URLSearchParams()
  for (const [chave, valor] of Object.entries(query)) {
    if (valor === undefined || valor === null || valor === '') continue
    params.set(chave, String(valor))
  }
  const s = params.toString()
  return s ? `?${s}` : ''
}

function gerarCorrelationId(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) return crypto.randomUUID()
  return `${Date.now().toString(16)}-${Math.random().toString(16).slice(2)}`
}

async function lerProblem(response: Response): Promise<ProblemDetails> {
  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('json')) {
    try {
      return (await response.json()) as ProblemDetails
    } catch {
      return {}
    }
  }
  return {}
}

/**
 * Wrapper de `fetch` para a API: base `/api/v1`, `Authorization: Bearer`, `X-Correlation-Id` por requisição,
 * JSON automático e conversão de erros (ProblemDetails) em `ApiError`. Um 401 encerra a sessão local.
 */
export async function http<T>(path: string, options: HttpOptions = {}): Promise<T> {
  const { method = 'GET', body, query, signal, ignorarNaoAutenticado } = options
  const headers = new Headers({ Accept: 'application/json', 'X-Correlation-Id': gerarCorrelationId() })
  if (body !== undefined) headers.set('Content-Type', 'application/json')
  const token = authStore.obterToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}${montarQuery(query)}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    })
  } catch (erro) {
    if (erro instanceof DOMException && erro.name === 'AbortError') throw erro
    throw new ApiError(0, { title: 'Não foi possível conectar ao servidor', detail: 'Verifique sua conexão e tente novamente.' })
  }

  if (response.status === 401 && !ignorarNaoAutenticado) {
    authStore.sair()
  }

  if (!response.ok) {
    const problem = await lerProblem(response)
    if (!problem.traceId) {
      const traceparent = response.headers.get('traceparent')
      if (traceparent) problem.traceId = traceparent.split('-')[1]
    }
    throw new ApiError(response.status, problem)
  }

  if (response.status === 204 || response.headers.get('content-length') === '0') {
    return undefined as T
  }
  const texto = await response.text()
  return (texto ? JSON.parse(texto) : undefined) as T
}

export const api = {
  get: <T>(path: string, query?: QueryParams, signal?: AbortSignal) => http<T>(path, { method: 'GET', query, signal }),
  post: <T>(path: string, body?: unknown, options?: Omit<HttpOptions, 'method' | 'body'>) =>
    http<T>(path, { ...options, method: 'POST', body }),
  put: <T>(path: string, body?: unknown) => http<T>(path, { method: 'PUT', body }),
  patch: <T>(path: string, body?: unknown) => http<T>(path, { method: 'PATCH', body }),
  delete: <T = void>(path: string) => http<T>(path, { method: 'DELETE' }),
}
