import type {
  ConteudoTipo,
  EventoFormato,
  EventoSituacao,
  InscricaoSituacao,
  PalestrantePapel,
  Perfil,
  SalaTipo,
} from '@/shared/api/types'

/** Tom visual usado por `StatusBadge`. */
export type Tom = 'neutral' | 'primary' | 'info' | 'success' | 'warning' | 'danger' | 'highlight'

export interface OpcaoEnum<T extends string = string> {
  value: T
  label: string
  tom?: Tom
}

export type MapaEnum<T extends string> = Record<T, { label: string; tom: Tom }>

export const EVENTO_SITUACAO: MapaEnum<EventoSituacao> = {
  Rascunho: { label: 'Rascunho', tom: 'neutral' },
  Publicado: { label: 'Publicado', tom: 'primary' },
  EmAndamento: { label: 'Em andamento', tom: 'success' },
  Encerrado: { label: 'Encerrado', tom: 'info' },
  Cancelado: { label: 'Cancelado', tom: 'danger' },
}

export const EVENTO_FORMATO: MapaEnum<EventoFormato> = {
  Presencial: { label: 'Presencial', tom: 'neutral' },
  Remoto: { label: 'Remoto', tom: 'neutral' },
  Hibrido: { label: 'Híbrido', tom: 'neutral' },
}

export const INSCRICAO_SITUACAO: MapaEnum<InscricaoSituacao> = {
  Confirmada: { label: 'Confirmada', tom: 'success' },
  Cancelada: { label: 'Cancelada', tom: 'danger' },
}

export const PALESTRANTE_PAPEL: MapaEnum<PalestrantePapel> = {
  Principal: { label: 'Principal', tom: 'primary' },
  Coautor: { label: 'Coautor', tom: 'neutral' },
  Mediador: { label: 'Mediador', tom: 'info' },
}

export const CONTEUDO_TIPO: MapaEnum<ConteudoTipo> = {
  Slides: { label: 'Slides', tom: 'neutral' },
  Pdf: { label: 'PDF', tom: 'neutral' },
  Arquivo: { label: 'Arquivo', tom: 'neutral' },
  Link: { label: 'Link', tom: 'neutral' },
  Video: { label: 'Vídeo', tom: 'neutral' },
  Imagem: { label: 'Imagem', tom: 'neutral' },
}

export const SALA_TIPO: MapaEnum<SalaTipo> = {
  AmbienteUnico: { label: 'Ambiente único', tom: 'primary' },
  Auditorio: { label: 'Auditório', tom: 'neutral' },
  SalaAula: { label: 'Sala de aula', tom: 'neutral' },
  Laboratorio: { label: 'Laboratório', tom: 'neutral' },
  AreaRecreacao: { label: 'Área de recreação', tom: 'neutral' },
  Coworking: { label: 'Coworking', tom: 'neutral' },
  Outro: { label: 'Outro', tom: 'neutral' },
}

export const PERFIL: MapaEnum<Perfil> = {
  Administrador: { label: 'Administrador', tom: 'primary' },
  Organizador: { label: 'Organizador', tom: 'info' },
  Participante: { label: 'Participante', tom: 'neutral' },
}

export const ATIVO: MapaEnum<'true' | 'false'> = {
  true: { label: 'Ativo', tom: 'success' },
  false: { label: 'Inativo', tom: 'neutral' },
}

/** Operações de auditoria (valores emitidos pelo interceptor do EF Core). */
export const OPERACAO_AUDITORIA: Record<string, { label: string; tom: Tom }> = {
  Added: { label: 'Inclusão', tom: 'success' },
  Insert: { label: 'Inclusão', tom: 'success' },
  Inserted: { label: 'Inclusão', tom: 'success' },
  Criacao: { label: 'Inclusão', tom: 'success' },
  Modified: { label: 'Alteração', tom: 'info' },
  Update: { label: 'Alteração', tom: 'info' },
  Updated: { label: 'Alteração', tom: 'info' },
  Alteracao: { label: 'Alteração', tom: 'info' },
  Deleted: { label: 'Exclusão', tom: 'danger' },
  Delete: { label: 'Exclusão', tom: 'danger' },
  Exclusao: { label: 'Exclusão', tom: 'danger' },
  SoftDelete: { label: 'Exclusão lógica', tom: 'danger' },
}

export const MODULOS = ['Identidade', 'Pessoas', 'Locais', 'Eventos', 'Palestras', 'Auditoria'] as const

export const UFS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI',
  'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO',
] as const

/** Converte um mapa de enum em opções para `<select>`. */
export function opcoesDe<T extends string>(mapa: MapaEnum<T>): OpcaoEnum<T>[] {
  return (Object.keys(mapa) as T[]).map((value) => ({ value, label: mapa[value].label, tom: mapa[value].tom }))
}

export function rotuloDe<T extends string>(mapa: Partial<Record<T, { label: string }>>, valor: T | null | undefined): string {
  if (!valor) return '—'
  return mapa[valor]?.label ?? valor
}
