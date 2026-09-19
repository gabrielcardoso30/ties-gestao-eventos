import { toast } from 'sonner'

import { ApiError } from '@/shared/api/api-error'

/** Exibe um toast de erro padronizado com título, detalhe e código do ProblemDetails. */
export function notificarErro(erro: unknown, titulo?: string) {
  if (ApiError.isApiError(erro)) {
    const descricao = [erro.detail ?? (erro.ehValidacao ? erro.mensagem : undefined), erro.codigo ? `Código: ${erro.codigo}` : undefined]
      .filter(Boolean)
      .join(' · ')
    toast.error(titulo ?? erro.title, { description: descricao || undefined })
    return
  }
  const mensagem = erro instanceof Error ? erro.message : 'Erro inesperado'
  toast.error(titulo ?? 'Erro inesperado', { description: mensagem })
}

export function notificarSucesso(titulo: string, descricao?: string) {
  toast.success(titulo, { description: descricao })
}
