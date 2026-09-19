import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Plus, Save, Trash2 } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { api } from '@/shared/api/http'
import type { ListarEventosItemResponse, ListarPessoasItemResponse, ObterEventoResponse, ObterLocalResponse, ObterPalestraResponse, PagedResult, PalestrantePapel } from '@/shared/api/types'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { PageHeader } from '@/shared/components/generic/PageHeader'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'

type PalestranteSelecionado = { pessoaId: string; palestrantePapel: PalestrantePapel; existente?: boolean }
type Formulario = { eventoId: string; salaId: string; palestraTitulo: string; palestraDescricao: string; palestraInicio: string; palestraFim: string }
const papeis: PalestrantePapel[] = ['Principal', 'Coautor', 'Mediador']
const dataLocal = (valor: string) => new Date(valor).toISOString().slice(0, 16)

interface Props { palestraId?: string }

export function PalestraForm({ palestraId }: Props) {
  const editando = Boolean(palestraId); const navigate = useNavigate(); const client = useQueryClient()
  const [form, setForm] = useState<Formulario>({ eventoId: '', salaId: '', palestraTitulo: '', palestraDescricao: '', palestraInicio: '', palestraFim: '' })
  const [palestrantes, setPalestrantes] = useState<PalestranteSelecionado[]>([])
  const [novaPessoaId, setNovaPessoaId] = useState(''); const [novoPapel, setNovoPapel] = useState<PalestrantePapel>('Principal')
  const eventos = useQuery({ queryKey: ['eventos', 'opcoes'], queryFn: () => api.get<PagedResult<ListarEventosItemResponse>>('/eventos', { pagina: 1, tamanhoPagina: 100 }) })
  const pessoas = useQuery({ queryKey: ['pessoas', 'opcoes'], queryFn: () => api.get<PagedResult<ListarPessoasItemResponse>>('/pessoas', { pagina: 1, tamanhoPagina: 100, estaAtivo: true }) })
  const detalhe = useQuery({ queryKey: ['palestras', palestraId], queryFn: () => api.get<ObterPalestraResponse>(`/palestras/${palestraId}`), enabled: editando })
  const evento = useQuery({ queryKey: ['evento-opcao', form.eventoId], queryFn: () => api.get<ObterEventoResponse>(`/eventos/${form.eventoId}`), enabled: Boolean(form.eventoId) })
  const local = useQuery({ queryKey: ['local-opcao', evento.data?.localId], queryFn: () => api.get<ObterLocalResponse>(`/locais/${evento.data!.localId}`), enabled: Boolean(evento.data?.localId) })

  useEffect(() => { if (detalhe.data) { const d = detalhe.data; setForm({ eventoId: d.eventoId, salaId: d.salaId ?? '', palestraTitulo: d.palestraTitulo, palestraDescricao: d.palestraDescricao ?? '', palestraInicio: dataLocal(d.palestraInicio), palestraFim: dataLocal(d.palestraFim) }); setPalestrantes(d.palestrantes.map(p => ({ pessoaId: p.pessoaId, palestrantePapel: p.palestrantePapel, existente: true }))) } }, [detalhe.data])
  const pessoasDisponiveis = useMemo(() => pessoas.data?.itens.filter(p => !palestrantes.some(x => x.pessoaId === p.id)) ?? [], [pessoas.data, palestrantes])
  const alterar = (campo: keyof Formulario, valor: string) => setForm(atual => ({ ...atual, [campo]: valor, ...(campo === 'eventoId' ? { salaId: '' } : {}) }))
  const adicionar = () => { if (!novaPessoaId) return; setPalestrantes(lista => [...lista, { pessoaId: novaPessoaId, palestrantePapel: novoPapel }]); setNovaPessoaId(''); setNovoPapel('Principal') }
  const remover = (pessoaId: string) => setPalestrantes(lista => lista.filter(p => p.pessoaId !== pessoaId))
  const nomePessoa = (id: string) => pessoas.data?.itens.find(p => p.id === id)?.pessoaNome ?? detalhe.data?.palestrantes.find(p => p.pessoaId === id)?.pessoaNome ?? id

  const salvar = useMutation({ mutationFn: async () => {
    const corpo = { salaId: form.salaId || null, palestraTitulo: form.palestraTitulo, palestraDescricao: form.palestraDescricao || null, palestraInicio: new Date(form.palestraInicio).toISOString(), palestraFim: new Date(form.palestraFim).toISOString() }
    if (!editando) return api.post<Record<string, unknown>>('/palestras', { ...corpo, eventoId: form.eventoId, palestrantes: palestrantes.map(({ pessoaId, palestrantePapel }) => ({ pessoaId, palestrantePapel })) })
    await api.put(`/palestras/${palestraId}`, corpo)
    const anteriores = detalhe.data!.palestrantes
    const adicionados = palestrantes.filter(p => !anteriores.some(a => a.pessoaId === p.pessoaId))
    for (const p of adicionados) await api.post(`/palestras/${palestraId}/palestrantes`, { pessoaId: p.pessoaId, palestrantePapel: p.palestrantePapel })
    const removidos = anteriores.filter(a => !palestrantes.some(p => p.pessoaId === a.pessoaId))
    for (const p of removidos) await api.delete(`/palestras/${palestraId}/palestrantes/${p.pessoaId}`)
    return { id: palestraId }
  }, onSuccess: async response => { await client.invalidateQueries({ queryKey: ['palestras'] }); navigate(`/palestras/${String(response.id)}`) } })

  if (eventos.isLoading || pessoas.isLoading || detalhe.isLoading) return <LoadingState />
  const erroConsulta = eventos.error ?? pessoas.error ?? detalhe.error
  if (erroConsulta) return <ErrorState erro={erroConsulta} />
  return <div className="space-y-6"><PageHeader titulo={editando ? 'Editar palestra' : 'Nova palestra'} descricao="Selecione o evento, a sala e uma ou mais pessoas palestrantes." voltarPara={editando ? `/palestras/${palestraId}` : '/palestras'} /><form className="space-y-6" onSubmit={e => { e.preventDefault(); salvar.mutate() }}><Card><CardContent className="grid gap-5 sm:grid-cols-2"><Campo label="Título" required><Input aria-label="Título" value={form.palestraTitulo} onChange={e => alterar('palestraTitulo', e.target.value)} required /></Campo><Campo label="Evento" required><select aria-label="Evento" value={form.eventoId} onChange={e => alterar('eventoId', e.target.value)} disabled={editando} required className="h-9 w-full rounded-md border bg-background px-3 text-sm"><option value="">Selecione o evento</option>{eventos.data?.itens.map(e => <option key={e.id} value={e.id}>{e.eventoNome}</option>)}</select></Campo><Campo label="Local do evento"><select aria-label="Local do evento" value={evento.data?.localId ?? ''} disabled className="h-9 w-full rounded-md border bg-muted px-3 text-sm"><option value="">{evento.data?.eventoFormato === 'Remoto' ? 'Evento remoto' : 'Selecione o evento'}</option>{evento.data?.localId && <option value={evento.data.localId}>{evento.data.localNome}</option>}</select></Campo><Campo label="Sala"><select aria-label="Sala" value={form.salaId} onChange={e => alterar('salaId', e.target.value)} disabled={!evento.data?.localId} className="h-9 w-full rounded-md border bg-background px-3 text-sm"><option value="">Sem sala definida</option>{local.data?.salas.filter(s => s.estaAtivo).map(s => <option key={s.id} value={s.id}>{s.salaNome} · {s.salaCapacidade} pessoas</option>)}</select></Campo><Campo label="Início" required><Input aria-label="Início" type="datetime-local" value={form.palestraInicio} onChange={e => alterar('palestraInicio', e.target.value)} required /></Campo><Campo label="Fim" required><Input aria-label="Fim" type="datetime-local" value={form.palestraFim} onChange={e => alterar('palestraFim', e.target.value)} required /></Campo><div className="sm:col-span-2"><Label className="mb-2">Descrição</Label><Textarea aria-label="Descrição" value={form.palestraDescricao} onChange={e => alterar('palestraDescricao', e.target.value)} /></div></CardContent></Card><Card><CardHeader><CardTitle>Palestrantes ({palestrantes.length})</CardTitle></CardHeader><CardContent className="space-y-4"><div className="grid gap-3 sm:grid-cols-[1fr_180px_auto]"><select aria-label="Pessoa palestrante" value={novaPessoaId} onChange={e => setNovaPessoaId(e.target.value)} className="h-9 rounded-md border bg-background px-3 text-sm"><option value="">Selecione uma pessoa</option>{pessoasDisponiveis.map(p => <option key={p.id} value={p.id}>{p.pessoaNome} · {p.pessoaEmail}</option>)}</select><select aria-label="Papel do palestrante" value={novoPapel} onChange={e => setNovoPapel(e.target.value as PalestrantePapel)} className="h-9 rounded-md border bg-background px-3 text-sm">{papeis.map(p => <option key={p}>{p}</option>)}</select><Button type="button" variant="outline" onClick={adicionar} disabled={!novaPessoaId}><Plus /> Adicionar</Button></div><div className="divide-y rounded-md border">{palestrantes.map(p => <div key={p.pessoaId} className="flex items-center justify-between gap-3 p-3"><div><strong className="text-sm">{nomePessoa(p.pessoaId)}</strong><div className="text-xs text-muted-foreground">{p.palestrantePapel}</div></div><Button type="button" size="icon" variant="ghost" aria-label={`Remover ${nomePessoa(p.pessoaId)}`} onClick={() => remover(p.pessoaId)} disabled={palestrantes.length === 1}><Trash2 /></Button></div>)}</div>{palestrantes.length === 0 && <p className="text-sm text-destructive">Adicione ao menos um palestrante.</p>}</CardContent></Card>{salvar.error && <ErrorState erro={salvar.error} compacto />}<div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => navigate(editando ? `/palestras/${palestraId}` : '/palestras')}>Cancelar</Button><Button type="submit" variant="cta" disabled={salvar.isPending || palestrantes.length === 0}>{salvar.isPending ? <Loader2 className="animate-spin" /> : <Save />} Salvar</Button></div></form></div>
}

function Campo({ label, required, children }: { label: string; required?: boolean; children: React.ReactNode }) { return <div><Label className="mb-2">{label}{required ? ' *' : ''}</Label>{children}</div> }
