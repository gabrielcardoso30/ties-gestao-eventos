import type { FieldValues, Path, UseFormReturn } from 'react-hook-form'

import { ApiError } from '@/shared/api/api-error'

/** "LocalNome" → "localNome"; "Palestrantes[0].PessoaId" → "palestrantes.0.pessoaId" */
export function normalizarCaminho(chave: string): string {
  return chave
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .map((seg) => (seg ? seg[0]!.toLowerCase() + seg.slice(1) : seg))
    .join('.')
}

/**
 * Mapeia os erros de validação (400, `errors`) de um `ApiError` para os campos do formulário.
 * Erros de campos desconhecidos são agrupados em `root.servidor`. Retorna `true` se algum erro foi aplicado.
 */
export function aplicarErrosDaApi<T extends FieldValues>(erro: unknown, form: UseFormReturn<T>): boolean {
  if (!ApiError.isApiError(erro) || !erro.ehValidacao) return false
  const campos = new Set(Object.keys(form.getValues()))
  const desconhecidos: string[] = []
  let aplicado = false
  for (const [chave, mensagens] of Object.entries(erro.errors ?? {})) {
    const caminho = normalizarCaminho(chave)
    const raiz = caminho.split('.')[0]!
    const mensagem = mensagens.join(' ')
    if (campos.has(raiz)) {
      form.setError(caminho as Path<T>, { type: 'server', message: mensagem })
      aplicado = true
    } else {
      desconhecidos.push(mensagem)
    }
  }
  if (desconhecidos.length > 0) {
    form.setError('root.servidor', { type: 'server', message: desconhecidos.join(' ') })
    aplicado = true
  }
  return aplicado
}
