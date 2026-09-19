import type { ProblemDetails } from './types'

/**
 * Erro da API normalizado a partir de um ProblemDetails (RFC 9457).
 * `codigo` segue "Modulo.Motivo" (ex.: "Locais.LocalPrecisaDeUmaSala"); `errors` traz erros de validação por campo (400).
 */
export class ApiError extends Error {
  readonly status: number
  readonly codigo: string
  readonly title: string
  readonly detail?: string
  readonly errors?: Record<string, string[]>
  readonly traceId?: string
  readonly type?: string

  constructor(status: number, problem: ProblemDetails = {}) {
    const title = problem.title ?? ApiError.tituloPadrao(status)
    super(problem.detail ?? title)
    this.name = 'ApiError'
    this.status = status
    this.codigo = problem.codigo ?? ApiError.codigoPadrao(status)
    this.title = title
    this.detail = problem.detail
    this.errors = problem.errors
    this.traceId = problem.traceId
    this.type = problem.type
  }

  /** Erro 400 com detalhes por campo. */
  get ehValidacao(): boolean {
    return this.status === 400 && !!this.errors && Object.keys(this.errors).length > 0
  }

  get ehNaoEncontrado(): boolean {
    return this.status === 404
  }

  get ehNaoAutorizado(): boolean {
    return this.status === 401
  }

  get ehProibido(): boolean {
    return this.status === 403
  }

  /** Texto principal para exibição ao usuário. */
  get mensagem(): string {
    if (this.ehValidacao && !this.detail) {
      return Object.values(this.errors ?? {}).flat()[0] ?? this.title
    }
    return this.detail ?? this.title
  }

  static isApiError(erro: unknown): erro is ApiError {
    return erro instanceof ApiError
  }

  static tituloPadrao(status: number): string {
    switch (status) {
      case 0:
        return 'Não foi possível conectar ao servidor'
      case 400:
        return 'Requisição inválida'
      case 401:
        return 'Não autenticado'
      case 403:
        return 'Acesso negado'
      case 404:
        return 'Recurso não encontrado'
      case 409:
        return 'Conflito'
      case 422:
        return 'Regra de negócio violada'
      case 429:
        return 'Muitas requisições'
      default:
        return status >= 500 ? 'Erro interno' : 'Erro inesperado'
    }
  }

  static codigoPadrao(status: number): string {
    switch (status) {
      case 0:
        return 'RedeIndisponivel'
      case 401:
        return 'NaoAutenticado'
      case 403:
        return 'AcessoNegado'
      case 404:
        return 'NaoEncontrado'
      default:
        return `Http${status}`
    }
  }
}
