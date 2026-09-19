import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'

type Primitivo = string | number | boolean | undefined

/**
 * Estado de listagem (busca, filtros, página) sincronizado com a query string da URL.
 * Alterar qualquer filtro (exceto `pagina`) volta para a página 1.
 */
export function useListParams<T extends Record<string, Primitivo>>(padroes: T) {
  const [searchParams, setSearchParams] = useSearchParams()

  const params = useMemo(() => {
    const resultado: Record<string, Primitivo> = { ...padroes }
    for (const chave of Object.keys(padroes)) {
      const bruto = searchParams.get(chave)
      if (bruto === null) continue
      const padrao = padroes[chave]
      if (typeof padrao === 'number') resultado[chave] = Number(bruto)
      else if (typeof padrao === 'boolean') resultado[chave] = bruto === 'true'
      else resultado[chave] = bruto
    }
    // Chaves não presentes nos padrões com valor undefined também são lidas como string.
    for (const [chave, valor] of searchParams.entries()) {
      if (!(chave in resultado)) resultado[chave] = valor
    }
    return resultado as T
  }, [searchParams, padroes])

  const definir = useCallback(
    (parcial: Partial<T>) => {
      setSearchParams(
        (atual) => {
          const proximo = new URLSearchParams(atual)
          for (const [chave, valor] of Object.entries(parcial)) {
            if (valor === undefined || valor === '' || valor === null) proximo.delete(chave)
            else proximo.set(chave, String(valor))
          }
          if (!('pagina' in parcial)) proximo.delete('pagina')
          return proximo
        },
        { replace: true },
      )
    },
    [setSearchParams],
  )

  const limpar = useCallback(() => setSearchParams({}, { replace: true }), [setSearchParams])

  return { params, definir, limpar }
}
