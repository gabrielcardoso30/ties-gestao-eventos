import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'

import { api } from '@/shared/api/http'
import type { ListarLocaisItemResponse, ObterEventoResponse, PagedResult } from '@/shared/api/types'
import { EntityFormPage, type EntityFormField } from '@/shared/components/generic/EntityFormPage'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { TrilhasManager } from './TrilhasManager'

const dataLocal = (valor: unknown) => valor ? new Date(String(valor)).toISOString().slice(0, 16) : ''
const transformar = (v: Record<string, unknown>) => {
  const { trilhaInicialNome, trilhaInicialDescricao, trilhaInicialCor, trilhas: _trilhas, ...dados } = v
  return { ...dados, localId: v.localId || null, eventoLinkRemoto: v.eventoLinkRemoto || null, eventoCapacidadeMaxima: v.eventoCapacidadeMaxima || null, eventoDataInicio: new Date(String(v.eventoDataInicio)).toISOString(), eventoDataFim: new Date(String(v.eventoDataFim)).toISOString(), ...(!v.id && trilhaInicialNome ? { trilhas: [{ trilhaNome: trilhaInicialNome, trilhaDescricao: trilhaInicialDescricao || null, trilhaCor: trilhaInicialCor || '#2563EB' }] } : {}) }
}

export function EventoForm({ editando = false }: { editando?: boolean }) {
  const { id } = useParams()
  const locais = useQuery({ queryKey: ['locais', 'opcoes-evento'], queryFn: () => api.get<PagedResult<ListarLocaisItemResponse>>('/locais', { pagina: 1, tamanhoPagina: 100, estaAtivo: true }) })
  const evento = useQuery({ queryKey: ['eventos', id], queryFn: () => api.get<ObterEventoResponse>(`/eventos/${id}`), enabled: editando && Boolean(id) })
  if (locais.isLoading || evento.isLoading) return <LoadingState />
  if (locais.isError || evento.isError) return <ErrorState erro={locais.error ?? evento.error} onTentarNovamente={() => { void locais.refetch(); void evento.refetch() }} />
  const fields: EntityFormField[] = [
    { name: 'eventoNome', label: 'Nome do evento', required: true },
    { name: 'eventoFormato', label: 'Formato', type: 'select', required: true, options: ['Presencial', 'Remoto', 'Hibrido'].map(value => ({ value, label: value })) },
    { name: 'eventoDataInicio', label: 'Início', type: 'datetime-local', required: true },
    { name: 'eventoDataFim', label: 'Fim', type: 'datetime-local', required: true },
    { name: 'localId', label: 'Local (presencial/híbrido)', type: 'select', options: locais.data!.itens.map(local => ({ value: local.id, label: `${local.localNome} · ${local.enderecoCidade}/${local.enderecoUf}` })) },
    { name: 'eventoLinkRemoto', label: 'Link remoto', type: 'url' },
    { name: 'eventoCapacidadeMaxima', label: 'Capacidade máxima', type: 'number' },
    { name: 'eventoDescricao', label: 'Descrição', type: 'textarea', full: true },
    ...(!editando ? [
      { name: 'trilhaInicialNome', label: 'Nome da trilha inicial', required: true },
      { name: 'trilhaInicialCor', label: 'Cor da trilha', type: 'color' as const },
      { name: 'trilhaInicialDescricao', label: 'Descrição da trilha', type: 'textarea' as const, full: true },
    ] : []),
  ]
  if (editando) return <div className="space-y-6"><EntityFormPage titulo="Editar evento" descricao="Atualize formato, período, local, capacidade e trilhas." voltarPara={`/eventos/${id}`} endpoint={`/eventos/${id}`} carregarDe={`/eventos/${id}`} mapearCarregado={d => ({ ...d, eventoDataInicio: dataLocal(d.eventoDataInicio), eventoDataFim: dataLocal(d.eventoDataFim) })} method="put" fields={fields} transformar={transformar} invalidar={['eventos']} aoSalvar={() => `/eventos/${id}`} />{evento.data && <TrilhasManager eventoId={evento.data.id} trilhas={evento.data.trilhas} />}</div>
  return <EntityFormPage titulo="Novo evento" descricao="Defina formato, período, local, capacidade e sua primeira trilha temática." voltarPara="/eventos" endpoint="/eventos" method="post" fields={fields} initial={{ eventoFormato: 'Presencial', trilhaInicialNome: 'Trilha única', trilhaInicialCor: '#2563EB' }} transformar={transformar} invalidar={['eventos']} aoSalvar={r => `/eventos/${r.id}`} />
}
