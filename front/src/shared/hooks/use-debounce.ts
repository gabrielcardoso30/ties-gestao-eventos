import { useEffect, useState } from 'react'

/** Retorna o valor após `atraso` ms sem alterações. */
export function useDebounce<T>(valor: T, atraso = 300): T {
  const [debounced, setDebounced] = useState(valor)
  useEffect(() => {
    const t = setTimeout(() => setDebounced(valor), atraso)
    return () => clearTimeout(t)
  }, [valor, atraso])
  return debounced
}
