import type { CriarSessaoResponse, Perfil, UsuarioSessao } from '@/shared/api/types'

/**
 * Armazenamento da sessão (token + usuário) em localStorage.
 *
 * Formato da chave `gestao-eventos.auth`:
 * ```json
 * { "accessToken": "<jwt>", "expiraEm": "2026-01-01T00:00:00Z", "usuario": { "id": "...", "usuarioNome": "...", "usuarioEmail": "...", "perfis": ["Administrador"] } }
 * ```
 * Para desenvolvimento, é possível injetar somente `{ "accessToken": "<jwt>" }`: os dados do usuário são derivados
 * das claims do JWT (`sub`, `name`, `email`, `role`, `exp`).
 */
export const AUTH_STORAGE_KEY = 'gestao-eventos.auth'

export interface Sessao {
  accessToken: string
  expiraEm: string
  usuario: UsuarioSessao
}

type Listener = (sessao: Sessao | null) => void

const listeners = new Set<Listener>()
let cache: Sessao | null | undefined

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parte = token.split('.')[1]
    if (!parte) return null
    const base64 = parte.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    )
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

/** Normaliza o que está no storage (sessão completa ou apenas token) para uma `Sessao`. */
function normalizar(bruto: unknown): Sessao | null {
  if (!bruto || typeof bruto !== 'object') return null
  const obj = bruto as Partial<Sessao> & { accessToken?: string }
  if (!obj.accessToken) return null

  if (obj.usuario && obj.expiraEm) {
    return { accessToken: obj.accessToken, expiraEm: obj.expiraEm, usuario: obj.usuario }
  }

  const claims = decodeJwtPayload(obj.accessToken)
  if (!claims) return null
  const roleClaim = claims['role'] ?? claims['roles'] ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
  const perfis = (Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : []) as Perfil[]
  const exp = typeof claims['exp'] === 'number' ? new Date(claims['exp'] * 1000).toISOString() : (obj.expiraEm ?? '')
  return {
    accessToken: obj.accessToken,
    expiraEm: exp,
    usuario: obj.usuario ?? {
      id: String(claims['sub'] ?? ''),
      usuarioNome: String(claims['name'] ?? claims['email'] ?? 'Usuário'),
      usuarioEmail: String(claims['email'] ?? ''),
      perfis,
    },
  }
}

function ler(): Sessao | null {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY)
    if (!raw) return null
    const sessao = normalizar(JSON.parse(raw))
    if (sessao?.expiraEm && new Date(sessao.expiraEm).getTime() <= Date.now()) {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      return null
    }
    return sessao
  } catch {
    return null
  }
}

function notificar() {
  for (const l of listeners) l(cache ?? null)
}

export const authStore = {
  obter(): Sessao | null {
    if (cache === undefined) cache = ler()
    return cache
  },
  obterToken(): string | null {
    return authStore.obter()?.accessToken ?? null
  },
  entrar(resposta: CriarSessaoResponse) {
    const sessao: Sessao = { accessToken: resposta.accessToken, expiraEm: resposta.expiraEm, usuario: resposta.usuario }
    try {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(sessao))
    } catch {
      /* storage indisponível: mantém apenas em memória */
    }
    cache = sessao
    notificar()
  },
  sair() {
    try {
      localStorage.removeItem(AUTH_STORAGE_KEY)
    } catch {
      /* ignore */
    }
    cache = null
    notificar()
  },
  /** Relê o storage (ex.: alterado em outra aba ou injetado manualmente). */
  recarregar() {
    cache = ler()
    notificar()
  },
  assinar(listener: Listener): () => void {
    listeners.add(listener)
    return () => listeners.delete(listener)
  },
}

if (typeof window !== 'undefined') {
  window.addEventListener('storage', (e) => {
    if (e.key === AUTH_STORAGE_KEY || e.key === null) authStore.recarregar()
  })
}
