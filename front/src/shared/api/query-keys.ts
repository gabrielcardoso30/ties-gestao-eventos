/**
 * Chaves do TanStack Query, agrupadas por módulo. Use sempre estas fábricas para invalidar de forma consistente.
 */
export const queryKeys = {
  identidade: {
    todos: ['identidade'] as const,
    usuarios: (params?: object) => ['identidade', 'usuarios', params ?? {}] as const,
    usuarioAtual: () => ['identidade', 'usuarios', 'me'] as const,
  },
  pessoas: {
    todos: ['pessoas'] as const,
    lista: (params?: object) => ['pessoas', 'lista', params ?? {}] as const,
    detalhe: (id: string) => ['pessoas', 'detalhe', id] as const,
  },
  locais: {
    todos: ['locais'] as const,
    lista: (params?: object) => ['locais', 'lista', params ?? {}] as const,
    detalhe: (id: string) => ['locais', 'detalhe', id] as const,
    salas: (id: string, params?: object) => ['locais', 'detalhe', id, 'salas', params ?? {}] as const,
  },
  eventos: {
    todos: ['eventos'] as const,
    lista: (params?: object) => ['eventos', 'lista', params ?? {}] as const,
    detalhe: (id: string) => ['eventos', 'detalhe', id] as const,
    inscricoes: (id: string, params?: object) => ['eventos', 'detalhe', id, 'inscricoes', params ?? {}] as const,
  },
  palestras: {
    todos: ['palestras'] as const,
    lista: (params?: object) => ['palestras', 'lista', params ?? {}] as const,
    detalhe: (id: string) => ['palestras', 'detalhe', id] as const,
    presencas: (id: string) => ['palestras', 'detalhe', id, 'presencas'] as const,
    certificado: (codigo: string) => ['palestras', 'certificados', codigo] as const,
  },
  auditoria: {
    todos: ['auditoria'] as const,
    lista: (params?: object) => ['auditoria', 'lista', params ?? {}] as const,
    detalhe: (id: string) => ['auditoria', 'detalhe', id] as const,
  },
}
