import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

import { Card, CardContent } from '@/shared/components/ui/card'
import { Skeleton } from '@/shared/components/ui/skeleton'
import { formatarNumero } from '@/shared/lib/format'
import { cn } from '@/shared/lib/utils'

export interface StatTileProps {
  rotulo: string
  valor: number | string | null | undefined
  isLoading?: boolean
  /** Exibido quando a consulta falhou. */
  erro?: unknown
  icone?: ReactNode
  descricao?: ReactNode
  /** Link de destino ao clicar no tile. */
  to?: string
  className?: string
}

/** Indicador numérico (Space Grotesk) para dashboards. */
export function StatTile({ rotulo, valor, isLoading, erro, icone, descricao, to, className }: StatTileProps) {
  const conteudo = (
    <CardContent className="flex items-start justify-between gap-3">
      <div className="min-w-0 space-y-1">
        <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{rotulo}</p>
        {isLoading ? (
          <Skeleton className="h-8 w-16" />
        ) : erro ? (
          <p className="font-numeric text-2xl text-muted-foreground" title="Indisponível">
            —
          </p>
        ) : (
          <p className="font-numeric text-3xl font-semibold tabular-nums">{typeof valor === 'number' ? formatarNumero(valor) : (valor ?? '—')}</p>
        )}
        {descricao && <p className="text-xs text-muted-foreground">{descricao}</p>}
      </div>
      {icone && <div className="flex size-9 shrink-0 items-center justify-center rounded-md bg-accent text-accent-foreground">{icone}</div>}
    </CardContent>
  )
  const classes = cn('py-4 transition-colors', to && 'hover:border-primary/40', className)
  if (to) {
    return (
      <Link to={to} className="block rounded-xl focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none">
        <Card className={classes}>{conteudo}</Card>
      </Link>
    )
  }
  return <Card className={classes}>{conteudo}</Card>
}
