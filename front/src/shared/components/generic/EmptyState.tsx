import { Inbox } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '@/shared/lib/utils'

export interface EmptyStateProps {
  titulo: string
  descricao?: ReactNode
  icone?: ReactNode
  /** Botão/ação principal (ex.: "Novo local"). */
  acao?: ReactNode
  className?: string
}

/** Estado vazio para listas e abas sem registros. */
export function EmptyState({ titulo, descricao, icone, acao, className }: EmptyStateProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center gap-3 rounded-lg border border-dashed px-6 py-12 text-center', className)}>
      <div className="flex size-11 items-center justify-center rounded-full bg-muted text-muted-foreground">
        {icone ?? <Inbox className="size-5" />}
      </div>
      <div className="space-y-1">
        <p className="font-heading font-medium">{titulo}</p>
        {descricao && <p className="max-w-md text-sm text-muted-foreground">{descricao}</p>}
      </div>
      {acao}
    </div>
  )
}
