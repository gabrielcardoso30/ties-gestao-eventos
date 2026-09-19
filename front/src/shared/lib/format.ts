import { format, formatDistanceToNow, isValid, parseISO } from 'date-fns'
import { ptBR } from 'date-fns/locale'

function paraData(valor: string | Date | null | undefined): Date | null {
  if (!valor) return null
  const d = typeof valor === 'string' ? parseISO(valor) : valor
  return isValid(d) ? d : null
}

/** 18/09/2026 */
export function formatarData(valor: string | Date | null | undefined): string {
  const d = paraData(valor)
  return d ? format(d, 'dd/MM/yyyy', { locale: ptBR }) : '—'
}

/** 18/09/2026 14:30 */
export function formatarDataHora(valor: string | Date | null | undefined): string {
  const d = paraData(valor)
  return d ? format(d, "dd/MM/yyyy 'às' HH:mm", { locale: ptBR }) : '—'
}

/** 14:30 */
export function formatarHora(valor: string | Date | null | undefined): string {
  const d = paraData(valor)
  return d ? format(d, 'HH:mm', { locale: ptBR }) : '—'
}

/** "18 de setembro de 2026" */
export function formatarDataExtenso(valor: string | Date | null | undefined): string {
  const d = paraData(valor)
  return d ? format(d, "d 'de' MMMM 'de' yyyy", { locale: ptBR }) : '—'
}

/** "há 3 dias" */
export function formatarRelativo(valor: string | Date | null | undefined): string {
  const d = paraData(valor)
  return d ? formatDistanceToNow(d, { locale: ptBR, addSuffix: true }) : '—'
}

/** Período "18/09/2026 09:00 – 18/09/2026 18:00" (omite a data final quando é o mesmo dia). */
export function formatarPeriodo(inicio: string | null | undefined, fim: string | null | undefined): string {
  const di = paraData(inicio)
  const df = paraData(fim)
  if (!di) return '—'
  if (!df) return formatarDataHora(di)
  const mesmoDia = format(di, 'yyyyMMdd') === format(df, 'yyyyMMdd')
  return mesmoDia
    ? `${format(di, 'dd/MM/yyyy HH:mm', { locale: ptBR })} – ${format(df, 'HH:mm', { locale: ptBR })}`
    : `${format(di, 'dd/MM/yyyy HH:mm', { locale: ptBR })} – ${format(df, 'dd/MM/yyyy HH:mm', { locale: ptBR })}`
}

export function formatarNumero(valor: number | null | undefined): string {
  if (valor === null || valor === undefined) return '—'
  return new Intl.NumberFormat('pt-BR').format(valor)
}

/** 90 → "1h30"; 45 → "45min" */
export function formatarMinutos(minutos: number | null | undefined): string {
  if (minutos === null || minutos === undefined) return '—'
  const h = Math.floor(minutos / 60)
  const m = Math.round(minutos % 60)
  if (h === 0) return `${m}min`
  return m === 0 ? `${h}h` : `${h}h${String(m).padStart(2, '0')}`
}

/** 12345678901 → 123.456.789-01 */
export function formatarCpf(valor: string | null | undefined): string {
  if (!valor) return '—'
  const d = valor.replace(/\D/g, '')
  if (d.length !== 11) return valor
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`
}

export function valorOuTraco(valor: string | number | null | undefined): string {
  if (valor === null || valor === undefined || valor === '') return '—'
  return String(valor)
}

/** Iniciais para avatar: "Gabriel Cardoso" → "GC" */
export function iniciais(nome: string | null | undefined): string {
  if (!nome) return '?'
  const partes = nome.trim().split(/\s+/)
  const primeira = partes[0]?.[0] ?? ''
  const ultima = partes.length > 1 ? (partes[partes.length - 1]?.[0] ?? '') : ''
  return (primeira + ultima).toUpperCase()
}
