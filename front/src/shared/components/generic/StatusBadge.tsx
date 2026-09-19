import { cn } from '@/shared/lib/utils'
import type { Tom } from '@/shared/lib/enums'
import { ATIVO } from '@/shared/lib/enums'

const TONS: Record<Tom, string> = {
  neutral: 'bg-muted text-muted-foreground border-border',
  primary: 'bg-primary/10 text-primary border-primary/20 dark:bg-primary/20 dark:text-primary-foreground',
  info: 'bg-info/10 text-info border-info/20',
  success: 'bg-success/10 text-success border-success/20',
  warning: 'bg-warning/10 text-warning border-warning/25',
  danger: 'bg-destructive/10 text-destructive border-destructive/20',
  highlight: 'bg-highlight/15 text-highlight-foreground border-highlight/40 dark:text-highlight',
}

export interface StatusBadgeProps<T extends string> {
  /** Valor do enum retornado pela API. */
  value: T | null | undefined
  /** Mapa enum → rótulo pt-BR e tom (ver `shared/lib/enums.ts`). Valores fora do mapa são exibidos como vêm. */
  map: Partial<Record<T, { label: string; tom: Tom }>>
  className?: string
}

/** Badge de situação/enum com cor semântica. */
export function StatusBadge<T extends string>({ value, map, className }: StatusBadgeProps<T>) {
  if (!value) return <span className="text-muted-foreground">—</span>
  const cfg = map[value] ?? { label: value, tom: 'neutral' as Tom }
  return (
    <span
      data-slot="status-badge"
      className={cn(
        'inline-flex w-fit items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap',
        TONS[cfg.tom],
        className,
      )}
    >
      {cfg.label}
    </span>
  )
}

/** Badge Ativo/Inativo. */
export function AtivoBadge({ ativo, className }: { ativo: boolean; className?: string }) {
  return <StatusBadge value={ativo ? 'true' : 'false'} map={ATIVO} className={className} />
}
