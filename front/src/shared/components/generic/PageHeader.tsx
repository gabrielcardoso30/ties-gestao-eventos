import { ArrowLeft } from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/shared/components/ui/breadcrumb'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/lib/utils'

export interface Trilha {
  label: string
  /** Sem `to`, o item é a página atual. */
  to?: string
}

export interface PageHeaderProps {
  titulo: ReactNode
  descricao?: ReactNode
  /** Botões à direita (ações da página). */
  acoes?: ReactNode
  /** Link "voltar" exibido acima do título. */
  voltarPara?: string
  trilha?: Trilha[]
  /** Conteúdo extra abaixo do título (ex.: badges de situação). */
  meta?: ReactNode
  className?: string
}

/** Cabeçalho padrão de página: trilha, título (Poppins), descrição e ações. */
export function PageHeader({ titulo, descricao, acoes, voltarPara, trilha, meta, className }: PageHeaderProps) {
  return (
    <header className={cn('flex flex-col gap-3', className)}>
      {trilha && trilha.length > 0 && (
        <Breadcrumb>
          <BreadcrumbList>
            {trilha.map((item, i) => (
              <BreadcrumbItem key={`${item.label}-${i}`}>
                {item.to ? (
                  <>
                    <BreadcrumbLink asChild>
                      <Link to={item.to}>{item.label}</Link>
                    </BreadcrumbLink>
                    {i < trilha.length - 1 && <BreadcrumbSeparator />}
                  </>
                ) : (
                  <BreadcrumbPage>{item.label}</BreadcrumbPage>
                )}
              </BreadcrumbItem>
            ))}
          </BreadcrumbList>
        </Breadcrumb>
      )}
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div className="min-w-0 space-y-1">
          {voltarPara && (
            <Button asChild variant="ghost" size="xs" className="-ml-2 text-muted-foreground">
              <Link to={voltarPara}>
                <ArrowLeft /> Voltar
              </Link>
            </Button>
          )}
          <div className="flex flex-wrap items-center gap-3">
            <h1 className="text-2xl font-semibold leading-tight text-foreground">{titulo}</h1>
            {meta}
          </div>
          {descricao && <p className="text-sm text-muted-foreground">{descricao}</p>}
        </div>
        {acoes && <div className="flex shrink-0 flex-wrap items-center gap-2">{acoes}</div>}
      </div>
    </header>
  )
}
