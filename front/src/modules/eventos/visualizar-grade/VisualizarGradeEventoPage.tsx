import { useQuery } from '@tanstack/react-query'
import { Clock, Mic2, Users } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'

import { api } from '@/shared/api/http'
import type { ListarPalestrasItemResponse, ObterEventoResponse, PagedResult } from '@/shared/api/types'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { PageHeader } from '@/shared/components/generic/PageHeader'
import { ScheduleGrid, type ScheduleEntry, type ScheduleSlot } from '@/shared/components/generic/ScheduleGrid'

interface GradePalestra extends ListarPalestrasItemResponse, ScheduleEntry {}

const formatarHora = (data: string) => new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' }).format(new Date(data))
const formatarDia = (data: string) => new Intl.DateTimeFormat('pt-BR', { weekday: 'short', day: '2-digit', month: '2-digit' }).format(new Date(data))
const slotId = (palestra: ListarPalestrasItemResponse) => `${palestra.palestraInicio}|${palestra.palestraFim}`

async function listarTodasAsPalestras(eventoId: string): Promise<ListarPalestrasItemResponse[]> {
  const primeira = await api.get<PagedResult<ListarPalestrasItemResponse>>('/palestras', { eventoId, pagina: 1, tamanhoPagina: 100 })
  if (primeira.totalPaginas <= 1) return primeira.itens
  const demais = await Promise.all(Array.from({ length: primeira.totalPaginas - 1 }, (_, index) => api.get<PagedResult<ListarPalestrasItemResponse>>('/palestras', { eventoId, pagina: index + 2, tamanhoPagina: 100 })))
  return [primeira, ...demais].flatMap(pagina => pagina.itens)
}

export function VisualizarGradeEventoPage() {
  const { id = '' } = useParams()
  const evento = useQuery({ queryKey: ['eventos', id, 'grade', 'evento'], queryFn: () => api.get<ObterEventoResponse>(`/eventos/${id}`), enabled: Boolean(id) })
  const palestras = useQuery({ queryKey: ['eventos', id, 'grade', 'palestras'], queryFn: () => listarTodasAsPalestras(id), enabled: Boolean(id) })

  if (evento.isLoading || palestras.isLoading) return <LoadingState />
  if (evento.isError) return <ErrorState erro={evento.error} />
  if (palestras.isError) return <ErrorState erro={palestras.error} />

  const dadosEvento = evento.data!
  const columns = dadosEvento.trilhas.map(trilha => ({ id: trilha.id, label: trilha.trilhaNome, color: trilha.trilhaCor }))
  const entries: GradePalestra[] = palestras.data!.map(palestra => ({ ...palestra, columnId: palestra.trilhaId, slotId: slotId(palestra) }))
  const slots: ScheduleSlot[] = [...new Map(palestras.data!.map(palestra => [slotId(palestra), palestra])).values()]
    .sort((a, b) => new Date(a.palestraInicio).getTime() - new Date(b.palestraInicio).getTime())
    .map(palestra => ({ id: slotId(palestra), label: <><span className="block capitalize">{formatarDia(palestra.palestraInicio)}</span><span className="mt-1 flex items-center gap-1 text-xs"><Clock className="size-3" /> {formatarHora(palestra.palestraInicio)}–{formatarHora(palestra.palestraFim)}</span></> }))

  return (
    <div className="space-y-6">
      <PageHeader titulo={`Grade de ${dadosEvento.eventoNome}`} descricao="Programação organizada por horário e trilha." voltarPara={`/eventos/${id}`} trilha={[{ label: 'Eventos', to: '/eventos' }, { label: dadosEvento.eventoNome, to: `/eventos/${id}` }, { label: 'Grade' }]} />
      {slots.length === 0 ? (
        <div className="rounded-xl border border-dashed p-10 text-center text-sm text-muted-foreground">Nenhuma palestra foi programada para este evento.</div>
      ) : (
        <ScheduleGrid columns={columns} slots={slots} entries={entries} renderEntry={palestra => (
          <Link to={`/palestras/${palestra.id}`} className="block rounded-lg border bg-background p-3 transition-colors hover:border-primary/50 hover:bg-accent/40">
            <strong className="block text-sm leading-snug">{palestra.palestraTitulo}</strong>
            <span className="mt-2 flex flex-wrap gap-x-3 gap-y-1 text-xs text-muted-foreground">
              {palestra.salaId && <span>Sala definida</span>}
              <span className="flex items-center gap-1"><Mic2 className="size-3" /> Palestra</span>
              <span className="flex items-center gap-1"><Users className="size-3" /> {palestra.palestrantesQuantidade} palestrante(s)</span>
            </span>
          </Link>
        )} />
      )}
    </div>
  )
}
