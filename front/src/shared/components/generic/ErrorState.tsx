import { AlertTriangle, RefreshCw, SearchX, ShieldAlert, WifiOff } from 'lucide-react'
import type { ReactNode } from 'react'

import { ApiError } from '@/shared/api/api-error'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/lib/utils'

export interface ErrorStateProps {
  erro: unknown
  /** Sobrescreve o título derivado do erro. */
  titulo?: string
  /** Sobrescreve a descrição derivada do erro. */
  descricao?: ReactNode
  onTentarNovamente?: () => void
  /** Versão inline (menor), para dentro de formulários e diálogos. */
  compacto?: boolean
  className?: string
}

function descreverErro(erro: unknown): { titulo: string; descricao: string; codigo?: string; traceId?: string; icone: ReactNode } {
  if (ApiError.isApiError(erro)) {
    const base = { codigo: erro.codigo, traceId: erro.traceId }
    if (erro.status === 0) return { ...base, titulo: erro.title, descricao: erro.detail ?? '', icone: <WifiOff className="size-5" /> }
    if (erro.status === 404)
      return { ...base, titulo: erro.title, descricao: erro.detail ?? 'O recurso solicitado não existe ou ainda não está disponível.', icone: <SearchX className="size-5" /> }
    if (erro.status === 403)
      return { ...base, titulo: 'Acesso negado', descricao: erro.detail ?? 'Você não tem permissão para acessar este recurso.', icone: <ShieldAlert className="size-5" /> }
    if (erro.status === 401)
      return { ...base, titulo: 'Sessão expirada', descricao: erro.detail ?? 'Entre novamente para continuar.', icone: <ShieldAlert className="size-5" /> }
    return { ...base, titulo: erro.title, descricao: erro.mensagem, icone: <AlertTriangle className="size-5" /> }
  }
  const mensagem = erro instanceof Error ? erro.message : String(erro ?? 'Erro desconhecido')
  return { titulo: 'Erro inesperado', descricao: mensagem, icone: <AlertTriangle className="size-5" /> }
}

/** Exibe um erro (`ApiError`/ProblemDetails ou genérico) com código e traceId para suporte. */
export function ErrorState({ erro, titulo, descricao, onTentarNovamente, compacto, className }: ErrorStateProps) {
  if (!erro) return null
  const info = descreverErro(erro)
  const detalhes = [info.codigo ? `Código: ${info.codigo}` : null, info.traceId ? `TraceId: ${info.traceId}` : null].filter(Boolean)

  if (compacto) {
    return (
      <div
        role="alert"
        className={cn('flex items-start gap-3 rounded-md border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive', className)}
      >
        <span className="mt-0.5 shrink-0">{info.icone}</span>
        <div className="min-w-0 flex-1 space-y-0.5">
          <p className="font-medium">{titulo ?? info.titulo}</p>
          {(descricao ?? info.descricao) && <p className="text-destructive/90">{descricao ?? info.descricao}</p>}
          {detalhes.length > 0 && <p className="font-numeric text-xs text-destructive/70">{detalhes.join(' · ')}</p>}
        </div>
        {onTentarNovamente && (
          <Button type="button" variant="ghost" size="xs" onClick={onTentarNovamente}>
            <RefreshCw /> Tentar novamente
          </Button>
        )}
      </div>
    )
  }

  return (
    <div role="alert" className={cn('flex flex-col items-center justify-center gap-3 rounded-lg border px-6 py-12 text-center', className)}>
      <div className="flex size-11 items-center justify-center rounded-full bg-destructive/10 text-destructive">{info.icone}</div>
      <div className="space-y-1">
        <p className="font-heading font-medium">{titulo ?? info.titulo}</p>
        {(descricao ?? info.descricao) && <p className="max-w-md text-sm text-muted-foreground">{descricao ?? info.descricao}</p>}
        {detalhes.length > 0 && <p className="font-numeric text-xs text-muted-foreground">{detalhes.join(' · ')}</p>}
      </div>
      {onTentarNovamente && (
        <Button type="button" variant="outline" size="sm" onClick={onTentarNovamente}>
          <RefreshCw /> Tentar novamente
        </Button>
      )}
    </div>
  )
}
