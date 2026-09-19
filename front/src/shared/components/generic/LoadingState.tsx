import { Skeleton } from '@/shared/components/ui/skeleton'
import { cn } from '@/shared/lib/utils'

export interface LoadingStateProps {
  /** Quantidade de linhas de skeleton. */
  linhas?: number
  className?: string
}

/** Skeleton genérico para páginas/cards em carregamento. */
export function LoadingState({ linhas = 4, className }: LoadingStateProps) {
  return (
    <div className={cn('space-y-3', className)} aria-busy="true" aria-live="polite">
      {Array.from({ length: linhas }).map((_, i) => (
        <Skeleton key={i} className={cn('h-4', i % 3 === 0 ? 'w-2/3' : i % 3 === 1 ? 'w-full' : 'w-1/2')} />
      ))}
    </div>
  )
}
