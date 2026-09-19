import { createContext, useCallback, useContext, useMemo, useSyncExternalStore, type ReactNode } from 'react'

import type { CriarSessaoResponse, Perfil, UsuarioSessao } from '@/shared/api/types'
import { authStore, type Sessao } from './auth-store'

export interface AuthContextValue {
  sessao: Sessao | null
  usuario: UsuarioSessao | null
  autenticado: boolean
  entrar: (resposta: CriarSessaoResponse) => void
  sair: () => void
  /** Verdadeiro se o usuário possui ao menos um dos perfis informados. */
  temPerfil: (...perfis: Perfil[]) => boolean
  /** Administrador ou Organizador: pode executar operações de escrita (Politicas.Gestao). */
  ehGestor: boolean
  /** Administrador (Politicas.Administracao). */
  ehAdministrador: boolean
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const sessao = useSyncExternalStore(authStore.assinar, authStore.obter, () => null)

  const temPerfil = useCallback(
    (...perfis: Perfil[]) => {
      const meus = sessao?.usuario.perfis ?? []
      return perfis.some((p) => meus.includes(p))
    },
    [sessao],
  )

  const value = useMemo<AuthContextValue>(
    () => ({
      sessao,
      usuario: sessao?.usuario ?? null,
      autenticado: !!sessao,
      entrar: authStore.entrar,
      sair: authStore.sair,
      temPerfil,
      ehGestor: temPerfil('Administrador', 'Organizador'),
      ehAdministrador: temPerfil('Administrador'),
    }),
    [sessao, temPerfil],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de <AuthProvider>')
  return ctx
}
