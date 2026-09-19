import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useState, type ReactNode } from 'react'

import { api } from '@/shared/api/http'
import type { PagedResult } from '@/shared/api/types'
import { EmptyState } from '@/shared/components/generic/EmptyState'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { PageHeader } from '@/shared/components/generic/PageHeader'
import { Badge } from '@/shared/components/ui/badge'
import { Card, CardContent } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'

export interface ResourceColumn<T> {
  titulo: string
  render: (item: T) => ReactNode
}

interface ResourceListPageProps<T> {
  modulo: string
  titulo: string
  descricao: string
  endpoint: string
  queryKey: string
  columns: ResourceColumn<T>[]
}

/** Listagem genérica usada por todos os módulos: busca, loading, erro, vazio, tabela e paginação. */
export function ResourceListPage<T extends { id: string }>({ modulo, titulo, descricao, endpoint, queryKey, columns }: ResourceListPageProps<T>) {
  const [busca, setBusca] = useState('')
  const resultado = useQuery({
    queryKey: [queryKey, busca],
    queryFn: ({ signal }) => api.get<PagedResult<T>>(endpoint, { busca, pagina: 1, tamanhoPagina: 20 }, signal),
  })

  return (
    <div className="space-y-6">
      <PageHeader titulo={titulo} descricao={descricao} trilha={[{ label: 'Início', to: '/' }, { label: modulo }]} />
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
        <Input value={busca} onChange={(e) => setBusca(e.target.value)} placeholder={`Buscar em ${titulo.toLowerCase()}`} className="pl-9" />
      </div>
      {resultado.isLoading ? <LoadingState /> : resultado.isError ? <ErrorState erro={resultado.error} onTentarNovamente={() => { void resultado.refetch() }} /> : !resultado.data?.itens.length ? (
        <EmptyState titulo={`Nenhum registro em ${titulo.toLowerCase()}`} descricao="Altere os filtros ou cadastre o primeiro registro pela API." />
      ) : (
        <Card className="overflow-hidden">
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-muted/70 text-left text-xs uppercase tracking-wide text-muted-foreground"><tr>{columns.map((c) => <th key={c.titulo} className="px-4 py-3">{c.titulo}</th>)}</tr></thead>
                <tbody>{resultado.data.itens.map((item) => <tr key={item.id} className="border-t hover:bg-muted/40">{columns.map((c) => <td key={c.titulo} className="px-4 py-3">{c.render(item)}</td>)}</tr>)}</tbody>
              </table>
            </div>
            <div className="flex items-center justify-between border-t px-4 py-3 text-xs text-muted-foreground">
              <span>{resultado.data.total} registro(s)</span><Badge variant="secondary">Página {resultado.data.pagina} de {resultado.data.totalPaginas || 1}</Badge>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
