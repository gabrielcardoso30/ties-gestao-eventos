import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Save } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { api } from '@/shared/api/http'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { PageHeader } from '@/shared/components/generic/PageHeader'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'

export interface EntityFormField {
  name: string
  label: string
  type?: 'text' | 'email' | 'password' | 'number' | 'datetime-local' | 'url' | 'color' | 'textarea' | 'select' | 'checkbox'
  required?: boolean
  placeholder?: string
  options?: Array<{ value: string; label: string }>
  full?: boolean
}

interface EntityFormPageProps {
  titulo: string
  descricao: string
  voltarPara: string
  endpoint: string
  method: 'post' | 'put'
  fields: EntityFormField[]
  initial?: Record<string, unknown>
  carregarDe?: string
  mapearCarregado?: (data: Record<string, unknown>) => Record<string, unknown>
  transformar?: (data: Record<string, unknown>) => unknown
  aoSalvar: (response: Record<string, unknown>) => string
  invalidar: string[]
}

/** Formulário genérico de criação/edição, usado pelas telas de todos os módulos. */
export function EntityFormPage({ titulo, descricao, voltarPara, endpoint, method, fields, initial = {}, carregarDe, mapearCarregado, transformar, aoSalvar, invalidar }: EntityFormPageProps) {
  const navigate = useNavigate(); const queryClient = useQueryClient(); const [values, setValues] = useState<Record<string, unknown>>(initial)
  const detalhe = useQuery({ queryKey: ['form', carregarDe], queryFn: () => api.get<Record<string, unknown>>(carregarDe!), enabled: !!carregarDe })
  useEffect(() => { if (detalhe.data) setValues(mapearCarregado ? mapearCarregado(detalhe.data) : detalhe.data) }, [detalhe.data])
  const salvar = useMutation({ mutationFn: () => api[method]<Record<string, unknown>>(endpoint, transformar ? transformar(values) : values), onSuccess: async (response) => { await Promise.all(invalidar.map(k => queryClient.invalidateQueries({ queryKey: [k] }))); navigate(aoSalvar(response)) } })
  const alterar = (name: string, value: unknown) => setValues(v => ({ ...v, [name]: value }))
  if (detalhe.isLoading) return <LoadingState />
  if (detalhe.isError) return <ErrorState erro={detalhe.error} onTentarNovamente={() => { void detalhe.refetch() }} />
  return <div className="space-y-6"><PageHeader titulo={titulo} descricao={descricao} voltarPara={voltarPara} /><Card><CardContent><form className="grid gap-5 sm:grid-cols-2" onSubmit={(e) => { e.preventDefault(); salvar.mutate() }}>{fields.map(field => <div key={field.name} className={field.full ? 'sm:col-span-2' : ''}><Label htmlFor={field.name} className="mb-2">{field.label}{field.required ? ' *' : ''}</Label>{field.type === 'textarea' ? <Textarea id={field.name} value={String(values[field.name] ?? '')} onChange={e => alterar(field.name, e.target.value)} placeholder={field.placeholder} required={field.required} /> : field.type === 'select' ? <select id={field.name} value={String(values[field.name] ?? '')} onChange={e => alterar(field.name, e.target.value || null)} required={field.required} className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm"><option value="">Selecione</option>{field.options?.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select> : field.type === 'checkbox' ? <Input id={field.name} type="checkbox" checked={Boolean(values[field.name])} onChange={e => alterar(field.name, e.target.checked)} className="size-4" /> : <Input id={field.name} type={field.type ?? 'text'} value={String(values[field.name] ?? '')} onChange={e => alterar(field.name, field.type === 'number' ? (e.target.value ? Number(e.target.value) : null) : e.target.value)} placeholder={field.placeholder} required={field.required} />}</div>)}{salvar.error && <div className="sm:col-span-2"><ErrorState erro={salvar.error} compacto /></div>}<div className="flex justify-end gap-2 sm:col-span-2"><Button type="button" variant="outline" onClick={() => navigate(voltarPara)}>Cancelar</Button><Button type="submit" variant="cta" disabled={salvar.isPending}>{salvar.isPending ? <Loader2 className="animate-spin" /> : <Save />} Salvar</Button></div></form></CardContent></Card></div>
}
