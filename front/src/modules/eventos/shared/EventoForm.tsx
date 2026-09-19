import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'

import { api } from '@/shared/api/http'
import type { ListarLocaisItemResponse, PagedResult } from '@/shared/api/types'
import { EntityFormPage, type EntityFormField } from '@/shared/components/generic/EntityFormPage'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'

const dataLocal = (valor: unknown) => valor ? new Date(String(valor)).toISOString().slice(0, 16) : ''
const transformar = (v: Record<string, unknown>) => ({ ...v, localId: v.localId || null, eventoLinkRemoto: v.eventoLinkRemoto || null, eventoCapacidadeMaxima: v.eventoCapacidadeMaxima || null, eventoDataInicio: new Date(String(v.eventoDataInicio)).toISOString(), eventoDataFim: new Date(String(v.eventoDataFim)).toISOString() })

export function EventoForm({ editando = false }: { editando?: boolean }) {
  const { id } = useParams()
  const locais = useQuery({ queryKey: ['locais', 'opcoes-evento'], queryFn: () => api.get<PagedResult<ListarLocaisItemResponse>>('/locais', { pagina: 1, tamanhoPagina: 100, estaAtivo: true }) })
  if (locais.isLoading) return <LoadingState />
  if (locais.isError) return <ErrorState erro={locais.error} onTentarNovamente={() => { void locais.refetch() }} />
  const fields: EntityFormField[] = [
    { name: 'eventoNome', label: 'Nome do evento', required: true },
    { name: 'eventoFormato', label: 'Formato', type: 'select', required: true, options: ['Presencial', 'Remoto', 'Hibrido'].map(value => ({ value, label: value })) },
    { name: 'eventoDataInicio', label: 'Início', type: 'datetime-local', required: true },
    { name: 'eventoDataFim', label: 'Fim', type: 'datetime-local', required: true },
    { name: 'localId', label: 'Local (presencial/híbrido)', type: 'select', options: locais.data!.itens.map(local => ({ value: local.id, label: `${local.localNome} · ${local.enderecoCidade}/${local.enderecoUf}` })) },
    { name: 'eventoLinkRemoto', label: 'Link remoto', type: 'url' },
    { name: 'eventoCapacidadeMaxima', label: 'Capacidade máxima', type: 'number' },
    { name: 'eventoDescricao', label: 'Descrição', type: 'textarea', full: true },
  ]
  if (editando) return <EntityFormPage titulo="Editar evento" descricao="Atualize formato, período, local e capacidade." voltarPara={`/eventos/${id}`} endpoint={`/eventos/${id}`} carregarDe={`/eventos/${id}`} mapearCarregado={d => ({ ...d, eventoDataInicio: dataLocal(d.eventoDataInicio), eventoDataFim: dataLocal(d.eventoDataFim) })} method="put" fields={fields} transformar={transformar} invalidar={['eventos']} aoSalvar={() => `/eventos/${id}`} />
  return <EntityFormPage titulo="Novo evento" descricao="Defina formato, período, local e capacidade." voltarPara="/eventos" endpoint="/eventos" method="post" fields={fields} initial={{ eventoFormato: 'Presencial' }} transformar={transformar} invalidar={['eventos']} aoSalvar={r => `/eventos/${r.id}`} />
}
