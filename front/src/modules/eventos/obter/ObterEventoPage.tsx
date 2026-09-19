import { CalendarRange } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import type { ObterEventoResponse } from '@/shared/api/types'
import { EntityDetailPage } from '@/shared/components/generic/EntityDetailPage'
import { Button } from '@/shared/components/ui/button'
import { TrilhasManager } from '../shared/TrilhasManager'

export function ObterEventoPage() {
  const { id } = useParams()
  return <EntityDetailPage<ObterEventoResponse & Record<string, unknown>> endpoint={`/eventos/${id}`} queryKey="eventos" backTo="/eventos" editTo={`/eventos/${id}/editar`} deleteEndpoint={`/eventos/${id}`} titulo={x => x.eventoNome} descricao={x => x.eventoDescricao} acoes={() => <Button asChild variant="outline"><Link to={`/eventos/${id}/grade`}><CalendarRange /> Ver grade</Link></Button>} itens={x => [{ label: 'Situação', valor: x.eventoSituacao }, { label: 'Formato', valor: x.eventoFormato }, { label: 'Início', valor: new Date(x.eventoDataInicio).toLocaleString('pt-BR') }, { label: 'Fim', valor: new Date(x.eventoDataFim).toLocaleString('pt-BR') }, { label: 'Local', valor: x.localNome }, { label: 'Link remoto', valor: x.eventoLinkRemoto }, { label: 'Capacidade', valor: x.eventoCapacidadeMaxima, numerico: true }, { label: 'Inscritos confirmados', valor: x.inscricoesConfirmadas, numerico: true }, { label: 'Motivo do cancelamento', valor: x.eventoCancelamentoMotivo, larguraTotal: true }]}>{x => <TrilhasManager eventoId={x.id} trilhas={(x as ObterEventoResponse).trilhas} />}</EntityDetailPage>
}
