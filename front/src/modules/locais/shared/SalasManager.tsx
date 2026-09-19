import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Loader2, Pencil, Plus, Save, Trash2 } from 'lucide-react'
import { useState } from 'react'

import { api } from '@/shared/api/http'
import type { ObterLocalSalaResponse, SalaTipo } from '@/shared/api/types'
import { ConfirmDialog } from '@/shared/components/generic/ConfirmDialog'
import { DetailCard } from '@/shared/components/generic/DescriptionList'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { Button } from '@/shared/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/shared/components/ui/dialog'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Switch } from '@/shared/components/ui/switch'
import { Textarea } from '@/shared/components/ui/textarea'

const tipos: Array<{ value: SalaTipo; label: string }> = [
  { value: 'AmbienteUnico', label: 'Ambiente único' }, { value: 'Auditorio', label: 'Auditório' },
  { value: 'SalaAula', label: 'Sala de aula' }, { value: 'Laboratorio', label: 'Laboratório' },
  { value: 'AreaRecreacao', label: 'Área de recreação' }, { value: 'Coworking', label: 'Coworking' }, { value: 'Outro', label: 'Outro' },
]
type FormSala = { salaNome: string; salaCapacidade: number | ''; salaTipo: SalaTipo; salaRecursos: string; estaAtivo: boolean }
const vazio: FormSala = { salaNome: '', salaCapacidade: '', salaTipo: 'SalaAula', salaRecursos: '', estaAtivo: true }

export function SalasManager({ localId, salas }: { localId: string; salas: ObterLocalSalaResponse[] }) {
  const client = useQueryClient(); const [aberto, setAberto] = useState(false); const [edicao, setEdicao] = useState<ObterLocalSalaResponse>(); const [form, setForm] = useState<FormSala>(vazio); const [excluir, setExcluir] = useState<ObterLocalSalaResponse>()
  const atualizarCache = () => client.invalidateQueries({ queryKey: ['locais'] })
  const salvar = useMutation({ mutationFn: () => edicao ? api.put(`/locais/${localId}/salas/${edicao.id}`, { ...form, salaRecursos: form.salaRecursos || null }) : api.post(`/locais/${localId}/salas`, { ...form, salaRecursos: form.salaRecursos || null, estaAtivo: undefined }), onSuccess: async () => { await atualizarCache(); setAberto(false); setEdicao(undefined); setForm(vazio) } })
  const remover = useMutation({ mutationFn: () => api.delete(`/locais/${localId}/salas/${excluir!.id}`), onSuccess: async () => { await atualizarCache(); setExcluir(undefined) } })
  const nova = () => { setEdicao(undefined); setForm(vazio); setAberto(true) }
  const editar = (sala: ObterLocalSalaResponse) => { setEdicao(sala); setForm({ salaNome: sala.salaNome, salaCapacidade: sala.salaCapacidade, salaTipo: sala.salaTipo, salaRecursos: sala.salaRecursos ?? '', estaAtivo: sala.estaAtivo }); setAberto(true) }
  const mudar = <K extends keyof FormSala>(campo: K, valor: FormSala[K]) => setForm(atual => ({ ...atual, [campo]: valor }))
  return <><DetailCard titulo={`Salas (${salas.length})`} descricao="Ambientes disponíveis para alocação das palestras." acoes={<Button type="button" size="sm" onClick={nova}><Plus /> Nova sala</Button>}><div className="divide-y">{salas.map(s => <div key={s.id} className="flex flex-col gap-3 py-3 sm:flex-row sm:items-center sm:justify-between"><div><div className="flex items-center gap-2"><strong>{s.salaNome}</strong>{!s.estaAtivo && <span className="rounded bg-muted px-2 py-0.5 text-xs text-muted-foreground">Inativa</span>}</div><div className="text-sm text-muted-foreground">{tipos.find(t => t.value === s.salaTipo)?.label} · {s.salaCapacidade} pessoas{s.salaRecursos ? ` · ${s.salaRecursos}` : ''}</div></div><div className="flex gap-2"><Button type="button" size="sm" variant="outline" onClick={() => editar(s)}><Pencil /> Editar</Button><Button type="button" size="sm" variant="ghost" className="text-destructive" onClick={() => setExcluir(s)}><Trash2 /> Excluir</Button></div></div>)}</div></DetailCard><Dialog open={aberto} onOpenChange={setAberto}><DialogContent><DialogHeader><DialogTitle>{edicao ? 'Editar sala' : 'Nova sala'}</DialogTitle><DialogDescription>Informe capacidade, tipo e recursos disponíveis no ambiente.</DialogDescription></DialogHeader><form className="grid gap-4 sm:grid-cols-2" onSubmit={e => { e.preventDefault(); salvar.mutate() }}><div className="sm:col-span-2"><Label htmlFor="salaNome" className="mb-2">Nome da sala *</Label><Input id="salaNome" value={form.salaNome} onChange={e => mudar('salaNome', e.target.value)} required /></div><div><Label htmlFor="salaCapacidade" className="mb-2">Capacidade *</Label><Input id="salaCapacidade" type="number" min={1} value={form.salaCapacidade} onChange={e => mudar('salaCapacidade', e.target.value ? Number(e.target.value) : '')} required /></div><div><Label htmlFor="salaTipo" className="mb-2">Tipo *</Label><select id="salaTipo" value={form.salaTipo} onChange={e => mudar('salaTipo', e.target.value as SalaTipo)} className="h-9 w-full rounded-md border bg-background px-3 text-sm">{tipos.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}</select></div><div className="sm:col-span-2"><Label htmlFor="salaRecursos" className="mb-2">Recursos</Label><Textarea id="salaRecursos" placeholder="Projetor, som, internet, acessibilidade..." value={form.salaRecursos} onChange={e => mudar('salaRecursos', e.target.value)} /></div>{edicao && <div className="flex items-center gap-3 sm:col-span-2"><Switch id="salaAtiva" checked={form.estaAtivo} onCheckedChange={valor => mudar('estaAtivo', valor)} /><Label htmlFor="salaAtiva">Sala ativa e disponível</Label></div>}{salvar.error && <div className="sm:col-span-2"><ErrorState erro={salvar.error} compacto /></div>}<DialogFooter className="sm:col-span-2"><Button type="button" variant="outline" onClick={() => setAberto(false)}>Cancelar</Button><Button type="submit" disabled={salvar.isPending}>{salvar.isPending ? <Loader2 className="animate-spin" /> : <Save />} Salvar sala</Button></DialogFooter></form></DialogContent></Dialog><ConfirmDialog open={Boolean(excluir)} onOpenChange={aberto => !aberto && setExcluir(undefined)} titulo="Excluir sala?" descricao="A sala será excluída logicamente. Um local precisa permanecer com pelo menos uma sala." confirmarLabel="Excluir sala" variante="destructive" isPending={remover.isPending} erro={remover.error} onConfirmar={() => remover.mutate()} /></>
}
