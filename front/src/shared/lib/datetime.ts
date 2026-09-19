import { format, formatISO, isValid, parseISO } from 'date-fns'

/** ISO (API) → valor de `<input type="datetime-local">` no fuso do navegador ("yyyy-MM-ddTHH:mm"). */
export function isoParaInputLocal(iso: string | null | undefined): string {
  if (!iso) return ''
  const d = parseISO(iso)
  return isValid(d) ? format(d, "yyyy-MM-dd'T'HH:mm") : ''
}

/** Valor de `<input type="datetime-local">` → ISO-8601 com offset local (ex.: 2026-09-18T14:30:00-03:00). */
export function inputLocalParaIso(valor: string | null | undefined): string | null {
  if (!valor) return null
  const d = new Date(valor)
  return isValid(d) ? formatISO(d) : null
}

/** Valor de `<input type="date">` → ISO no início do dia local. */
export function dataInputParaIsoInicio(valor: string | null | undefined): string | undefined {
  if (!valor) return undefined
  const d = new Date(`${valor}T00:00:00`)
  return isValid(d) ? formatISO(d) : undefined
}

/** Valor de `<input type="date">` → ISO no fim do dia local. */
export function dataInputParaIsoFim(valor: string | null | undefined): string | undefined {
  if (!valor) return undefined
  const d = new Date(`${valor}T23:59:59`)
  return isValid(d) ? formatISO(d) : undefined
}

/** Compara dois valores de datetime-local; verdadeiro quando `fim` é posterior a `inicio`. */
export function fimAposInicio(inicio: string | undefined, fim: string | undefined): boolean {
  if (!inicio || !fim) return true
  return new Date(fim).getTime() > new Date(inicio).getTime()
}
