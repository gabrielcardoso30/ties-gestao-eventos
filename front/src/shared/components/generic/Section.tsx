import type { ReactNode } from 'react'
import { useSearchParams } from 'react-router-dom'

import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs'
import { cn } from '@/shared/lib/utils'

export interface SectionProps {
  titulo?: ReactNode
  descricao?: ReactNode
  acoes?: ReactNode
  children: ReactNode
  className?: string
}

/** Seção de página com título (h2) e ações alinhadas. */
export function Section({ titulo, descricao, acoes, children, className }: SectionProps) {
  return (
    <section className={cn('space-y-3', className)}>
      {(titulo || acoes) && (
        <div className="flex flex-wrap items-start justify-between gap-2">
          <div>
            {titulo && <h2 className="text-lg font-semibold">{titulo}</h2>}
            {descricao && <p className="text-sm text-muted-foreground">{descricao}</p>}
          </div>
          {acoes && <div className="flex items-center gap-2">{acoes}</div>}
        </div>
      )}
      {children}
    </section>
  )
}

export interface Aba {
  value: string
  label: ReactNode
  conteudo: ReactNode
  /** Contador exibido ao lado do rótulo. */
  contador?: number
  oculta?: boolean
}

export interface PageTabsProps {
  abas: Aba[]
  /** Aba inicial (quando não sincronizada pela URL). */
  padrao?: string
  /** Nome do parâmetro de query para sincronizar a aba ativa com a URL (ex.: `aba`). */
  paramUrl?: string
  className?: string
}

/** Abas de página com sincronização opcional na URL. */
export function PageTabs({ abas, padrao, paramUrl, className }: PageTabsProps) {
  const [searchParams, setSearchParams] = useSearchParams()
  const visiveis = abas.filter((a) => !a.oculta)
  const primeira = visiveis[0]?.value
  const valorUrl = paramUrl ? searchParams.get(paramUrl) : null
  const ativa = (valorUrl && visiveis.some((a) => a.value === valorUrl) ? valorUrl : undefined) ?? padrao ?? primeira

  const onChange = (v: string) => {
    if (!paramUrl) return
    setSearchParams(
      (atual) => {
        const proximo = new URLSearchParams(atual)
        proximo.set(paramUrl, v)
        return proximo
      },
      { replace: true },
    )
  }

  if (visiveis.length === 0) return null

  return (
    <Tabs value={paramUrl ? ativa : undefined} defaultValue={paramUrl ? undefined : ativa} onValueChange={onChange} className={className}>
      <TabsList className="flex-wrap">
        {visiveis.map((a) => (
          <TabsTrigger key={a.value} value={a.value}>
            {a.label}
            {a.contador !== undefined && (
              <span className="ml-1 rounded-full bg-muted px-1.5 font-numeric text-[11px] text-muted-foreground">{a.contador}</span>
            )}
          </TabsTrigger>
        ))}
      </TabsList>
      {visiveis.map((a) => (
        <TabsContent key={a.value} value={a.value} className="pt-2">
          {a.conteudo}
        </TabsContent>
      ))}
    </Tabs>
  )
}
