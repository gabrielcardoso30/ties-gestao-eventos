import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Trash2 } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '@/shared/api/http'
import { ConfirmDialog } from './ConfirmDialog'
import { DescriptionList, DetailCard, type ItemDescricao } from './DescriptionList'
import { ErrorState } from './ErrorState'
import { LoadingState } from './LoadingState'
import { PageHeader } from './PageHeader'
import { Button } from '@/shared/components/ui/button'

interface Props<T extends Record<string, unknown>> { endpoint: string; queryKey: string; backTo: string; editTo?: string; deleteEndpoint?: string; titulo: (data: T) => ReactNode; descricao?: (data: T) => ReactNode; itens: (data: T) => ItemDescricao[]; acoes?: (data: T) => ReactNode; children?: (data: T) => ReactNode }
/** Detalhe genérico com consulta, edição, exclusão confirmada e extensões específicas do módulo. */
export function EntityDetailPage<T extends Record<string, unknown>>({ endpoint, queryKey, backTo, editTo, deleteEndpoint, titulo, descricao, itens, acoes, children }: Props<T>) {
  const navigate = useNavigate(); const client = useQueryClient(); const [confirmar, setConfirmar] = useState(false)
  const query = useQuery({ queryKey: [queryKey, endpoint], queryFn: () => api.get<T>(endpoint) })
  const excluir = useMutation({ mutationFn: () => api.delete(deleteEndpoint!), onSuccess: async () => { await client.invalidateQueries({ queryKey: [queryKey] }); navigate(backTo) } })
  if (query.isLoading) return <LoadingState />; if (query.isError) return <ErrorState erro={query.error} />; const data = query.data!
  return <div className="space-y-6"><PageHeader titulo={titulo(data)} descricao={descricao?.(data)} voltarPara={backTo} acoes={<>{acoes?.(data)}{editTo && <Button asChild><Link to={editTo}><Pencil /> Editar</Link></Button>}{deleteEndpoint && <Button variant="destructive" onClick={() => setConfirmar(true)}><Trash2 /> Excluir</Button>}</>} /><DetailCard><DescriptionList itens={itens(data)} colunas={3} /></DetailCard>{children?.(data)}<ConfirmDialog open={confirmar} onOpenChange={setConfirmar} titulo="Excluir registro?" descricao="A exclusão é lógica e ficará registrada na auditoria." confirmarLabel="Excluir" variante="destructive" isPending={excluir.isPending} erro={excluir.error} onConfirmar={() => excluir.mutate()} /></div>
}
