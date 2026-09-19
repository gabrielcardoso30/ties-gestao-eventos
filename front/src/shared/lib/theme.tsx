import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'

export type Tema = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'gestao-eventos.tema'

interface ThemeContextValue {
  tema: Tema
  temaResolvido: 'light' | 'dark'
  definirTema: (tema: Tema) => void
  alternar: () => void
}

const ThemeContext = createContext<ThemeContextValue | null>(null)

function lerTema(): Tema {
  try {
    const v = localStorage.getItem(STORAGE_KEY)
    return v === 'light' || v === 'dark' ? v : 'system'
  } catch {
    return 'system'
  }
}

function prefereEscuro(): boolean {
  return typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches
}

/** Provedor de tema claro/escuro (classe `dark` no `<html>`), persistido em localStorage. */
export function ThemeProvider({ children }: { children: ReactNode }) {
  const [tema, setTema] = useState<Tema>(lerTema)
  const [sistemaEscuro, setSistemaEscuro] = useState(prefereEscuro)

  useEffect(() => {
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = (e: MediaQueryListEvent) => setSistemaEscuro(e.matches)
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  const temaResolvido: 'light' | 'dark' = tema === 'system' ? (sistemaEscuro ? 'dark' : 'light') : tema

  useEffect(() => {
    document.documentElement.classList.toggle('dark', temaResolvido === 'dark')
  }, [temaResolvido])

  const definirTema = useCallback((novo: Tema) => {
    setTema(novo)
    try {
      localStorage.setItem(STORAGE_KEY, novo)
    } catch {
      /* ignore */
    }
  }, [])

  const alternar = useCallback(() => definirTema(temaResolvido === 'dark' ? 'light' : 'dark'), [definirTema, temaResolvido])

  const value = useMemo(() => ({ tema, temaResolvido, definirTema, alternar }), [tema, temaResolvido, definirTema, alternar])
  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme deve ser usado dentro de <ThemeProvider>')
  return ctx
}
