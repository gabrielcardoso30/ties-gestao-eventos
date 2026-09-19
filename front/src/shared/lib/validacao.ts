import { z } from 'zod'

/** Valida CPF (11 dígitos, aceita máscara). */
export function cpfValido(valor: string): boolean {
  const d = valor.replace(/\D/g, '')
  if (d.length !== 11 || /^(\d)\1+$/.test(d)) return false
  const calc = (base: string, fator: number) => {
    let soma = 0
    for (const ch of base) soma += Number(ch) * fator--
    const resto = (soma * 10) % 11
    return resto === 10 ? 0 : resto
  }
  return calc(d.slice(0, 9), 10) === Number(d[9]) && calc(d.slice(0, 10), 11) === Number(d[10])
}

/** Campo de texto opcional: string vazia vira `undefined`. */
export const textoOpcional = (max?: number) => {
  const base = max ? z.string().max(max, `Máximo de ${max} caracteres`) : z.string()
  return base.optional().or(z.literal(''))
}

export const textoObrigatorio = (max?: number) => {
  const base = z.string().trim().min(1, 'Campo obrigatório')
  return max ? base.max(max, `Máximo de ${max} caracteres`) : base
}

export const emailObrigatorio = z.string().trim().min(1, 'Campo obrigatório').email('E-mail inválido')

export const urlOpcional = z
  .string()
  .trim()
  .url('URL inválida')
  .max(2000, 'Máximo de 2000 caracteres')
  .optional()
  .or(z.literal(''))

export const cpfOpcional = z
  .string()
  .trim()
  .optional()
  .or(z.literal(''))
  .refine((v) => !v || cpfValido(v), 'CPF inválido')

export const dataHoraObrigatoria = z.string().min(1, 'Informe data e hora')

export const guidObrigatorio = z.string().min(1, 'Selecione uma opção')

export const senhaForte = z
  .string()
  .min(8, 'Mínimo de 8 caracteres')
  .regex(/[A-Z]/, 'Precisa de uma letra maiúscula')
  .regex(/[a-z]/, 'Precisa de uma letra minúscula')
  .regex(/\d/, 'Precisa de um dígito')
  .regex(/[^A-Za-z0-9]/, 'Precisa de um símbolo')

/** Converte '' em null e mantém o restante — usado ao montar o corpo enviado à API. */
export function vazioParaNulo<T extends string | number | undefined | null>(valor: T): Exclude<T, ''> | null {
  if (valor === '' || valor === undefined) return null
  return valor as Exclude<T, ''>
}

/** Remove máscara de CPF antes de enviar. */
export function somenteDigitos(valor: string | null | undefined): string | null {
  if (!valor) return null
  const d = valor.replace(/\D/g, '')
  return d || null
}
