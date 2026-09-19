import type { ReactNode } from 'react'

import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { cn } from '@/shared/lib/utils'

export interface ItemDescricao {
  label: string
  valor: ReactNode
  /** Ocupa toda a largura da grade. */
  larguraTotal?: boolean
  /** Renderiza o valor com fonte numérica (Space Grotesk). */
  numerico?: boolean
}

export interface DescriptionListProps {
  itens: ItemDescricao[]
  colunas?: 1 | 2 | 3 | 4
  className?: string
}

/** Lista de pares rótulo/valor em grade responsiva. */
export function DescriptionList({ itens, colunas = 2, className }: DescriptionListProps) {
  const cols = { 1: 'sm:grid-cols-1', 2: 'sm:grid-cols-2', 3: 'sm:grid-cols-3', 4: 'sm:grid-cols-2 lg:grid-cols-4' }[colunas]
  return (
    <dl className={cn('grid grid-cols-1 gap-x-6 gap-y-4', cols, className)}>
      {itens.map((item) => (
        <div key={item.label} className={cn('min-w-0 space-y-1', item.larguraTotal && 'sm:col-span-full')}>
          <dt className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{item.label}</dt>
          <dd className={cn('text-sm break-words text-foreground', item.numerico && 'font-numeric text-base')}>
            {item.valor === null || item.valor === undefined || item.valor === '' ? <span className="text-muted-foreground">—</span> : item.valor}
          </dd>
        </div>
      ))}
    </dl>
  )
}

export interface DetailCardProps {
  titulo?: ReactNode
  descricao?: ReactNode
  acoes?: ReactNode
  children: ReactNode
  className?: string
  contentClassName?: string
}

/** Card de detalhe com título, descrição opcional e ações no canto. */
export function DetailCard({ titulo, descricao, acoes, children, className, contentClassName }: DetailCardProps) {
  return (
    <Card className={className}>
      {(titulo || acoes) && (
        <CardHeader>
          {titulo && <CardTitle className="font-heading text-base">{titulo}</CardTitle>}
          {descricao && <CardDescription>{descricao}</CardDescription>}
          {acoes && <CardAction>{acoes}</CardAction>}
        </CardHeader>
      )}
      <CardContent className={contentClassName}>{children}</CardContent>
    </Card>
  )
}
