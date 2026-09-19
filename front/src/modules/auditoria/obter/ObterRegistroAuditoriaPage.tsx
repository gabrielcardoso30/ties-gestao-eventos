import { useParams } from 'react-router-dom'
import type { ObterRegistroAuditoriaResponse } from '@/shared/api/types'
import { EntityDetailPage } from '@/shared/components/generic/EntityDetailPage'
import { DetailCard } from '@/shared/components/generic/DescriptionList'

function formatarJson(valor: string | null) {
  if (!valor) return '—'
  try { return JSON.stringify(JSON.parse(valor), null, 2) } catch { return valor }
}

export function ObterRegistroAuditoriaPage() {
  const { id } = useParams()
  return <EntityDetailPage<ObterRegistroAuditoriaResponse & Record<string, unknown>> endpoint={`/auditoria/registros/${id}`} queryKey="auditoria" backTo="/auditoria" titulo={dados => `${dados.modulo} · ${dados.operacao}`} descricao={dados => `Registro imutável ${dados.id}`} itens={dados => [
    { label: 'Entidade', valor: dados.entidadeNome },
    { label: 'Identificador', valor: dados.entidadeId },
    { label: 'Usuário', valor: dados.usuarioNome || 'sistema' },
    { label: 'Trace ID', valor: dados.traceId },
    { label: 'Ocorrido em', valor: new Date(dados.ocorridoEm).toLocaleString('pt-BR') },
    { label: 'Registrado em', valor: new Date(dados.registradoEm).toLocaleString('pt-BR') },
  ]} children={dados => <div className="grid gap-6 lg:grid-cols-2"><DetailCard titulo="Estado anterior"><pre className="overflow-auto whitespace-pre-wrap text-xs">{formatarJson(dados.dadosAnteriores)}</pre></DetailCard><DetailCard titulo="Estado novo"><pre className="overflow-auto whitespace-pre-wrap text-xs">{formatarJson(dados.dadosNovos)}</pre></DetailCard></div>} />
}
