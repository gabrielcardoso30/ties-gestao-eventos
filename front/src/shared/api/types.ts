/**
 * Tipos TypeScript espelhando os DTOs e enums da API v1 (docs/spec/api-endpoints.md).
 * Campos em camelCase; enums serializados como string; datas ISO-8601 (DateTimeOffset).
 */

// ---------------------------------------------------------------------------
// Comum
// ---------------------------------------------------------------------------

/** Resultado paginado padrão de todas as listagens. */
export interface PagedResult<T> {
  itens: T[]
  pagina: number
  tamanhoPagina: number
  total: number
  totalPaginas: number
}

/** Parâmetros de paginação aceitos por todas as listagens (query string). */
export interface PagedRequest {
  pagina?: number
  tamanhoPagina?: number
  ordenarPor?: string
  direcao?: 'Asc' | 'Desc'
}

/** ProblemDetails (RFC 9457) conforme emitido pela API, com extensões `codigo` e `traceId`. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  codigo?: string
  traceId?: string
  errors?: Record<string, string[]>
}

// ---------------------------------------------------------------------------
// Identidade
// ---------------------------------------------------------------------------

export type Perfil = 'Administrador' | 'Organizador' | 'Participante'

export interface CriarSessaoRequest {
  usuarioEmail: string
  senha: string
}

export interface UsuarioSessao {
  id: string
  usuarioNome: string
  usuarioEmail: string
  perfis: Perfil[]
}

export interface CriarSessaoResponse {
  accessToken: string
  expiraEm: string
  usuario: UsuarioSessao
}

export interface RegistrarUsuarioRequest {
  usuarioNome: string
  usuarioEmail: string
  senha: string
  perfis: Perfil[]
}

export interface RegistrarUsuarioResponse {
  id: string
  usuarioNome: string
  usuarioEmail: string
  perfis: Perfil[]
}

export interface ObterUsuarioAtualResponse {
  id: string
  usuarioNome: string
  usuarioEmail: string
  perfis: Perfil[]
  ultimoAcessoEm: string | null
}

export interface ListarUsuariosRequest extends PagedRequest {
  busca?: string
  estaAtivo?: boolean
}

export interface ListarUsuariosItemResponse {
  id: string
  usuarioNome: string
  usuarioEmail: string
  perfis: Perfil[]
  estaAtivo: boolean
  ultimoAcessoEm: string | null
}

export interface AtualizarPerfisUsuarioRequest {
  perfis: Perfil[]
}

export interface AtualizarPerfisUsuarioResponse {
  id: string
  perfis: Perfil[]
}

// ---------------------------------------------------------------------------
// Pessoas
// ---------------------------------------------------------------------------

export interface CriarPessoaRequest {
  pessoaNome: string
  pessoaEmail: string
  pessoaTelefone?: string | null
  pessoaDocumento?: string | null
  pessoaEmpresa?: string | null
  pessoaCargo?: string | null
  pessoaMiniBio?: string | null
  pessoaFotoUrl?: string | null
}

export interface CriarPessoaResponse {
  id: string
  pessoaNome: string
  pessoaEmail: string
}

export interface AtualizarPessoaRequest extends CriarPessoaRequest {
  estaAtivo: boolean
}

export interface AtualizarPessoaResponse {
  id: string
  pessoaNome: string
  pessoaEmail: string
  estaAtivo: boolean
  alteradoEm: string | null
}

export interface ObterPessoaResponse {
  id: string
  pessoaNome: string
  pessoaEmail: string
  pessoaTelefone: string | null
  pessoaDocumento: string | null
  pessoaEmpresa: string | null
  pessoaCargo: string | null
  pessoaMiniBio: string | null
  pessoaFotoUrl: string | null
  estaAtivo: boolean
  criadoEm: string
  alteradoEm: string | null
}

export interface ListarPessoasRequest extends PagedRequest {
  busca?: string
  estaAtivo?: boolean
}

export interface ListarPessoasItemResponse {
  id: string
  pessoaNome: string
  pessoaEmail: string
  pessoaEmpresa: string | null
  pessoaCargo: string | null
  estaAtivo: boolean
}

// ---------------------------------------------------------------------------
// Locais
// ---------------------------------------------------------------------------

export type SalaTipo =
  | 'AmbienteUnico'
  | 'Auditorio'
  | 'SalaAula'
  | 'Laboratorio'
  | 'AreaRecreacao'
  | 'Coworking'
  | 'Outro'

export interface CriarLocalRequest {
  localNome: string
  localDescricao?: string | null
  enderecoLogradouro?: string | null
  enderecoNumero?: string | null
  enderecoBairro?: string | null
  enderecoCidade: string
  enderecoUf: string
  enderecoCep?: string | null
  /** Quando informado, o local nasce com a sala "Ambiente único" com esta capacidade. */
  capacidadeAmbienteUnico?: number | null
}

export interface CriarLocalResponse {
  id: string
  localNome: string
  salasQuantidade: number
}

export type AtualizarLocalRequest = Omit<CriarLocalRequest, 'capacidadeAmbienteUnico'>

export interface AtualizarLocalResponse {
  id: string
  localNome: string
  alteradoEm: string | null
}

export interface ObterLocalSalaResponse {
  id: string
  salaNome: string
  salaCapacidade: number
  salaTipo: SalaTipo
  salaRecursos: string | null
  estaAtivo: boolean
}

export interface ObterLocalResponse {
  id: string
  localNome: string
  localDescricao: string | null
  enderecoLogradouro: string | null
  enderecoNumero: string | null
  enderecoBairro: string | null
  enderecoCidade: string
  enderecoUf: string
  enderecoCep: string | null
  localCapacidadeTotal: number
  estaAtivo: boolean
  criadoEm: string
  alteradoEm: string | null
  salas: ObterLocalSalaResponse[]
}

export interface ListarLocaisRequest extends PagedRequest {
  busca?: string
  enderecoUf?: string
  estaAtivo?: boolean
}

export interface ListarLocaisItemResponse {
  id: string
  localNome: string
  enderecoCidade: string
  enderecoUf: string
  salasQuantidade: number
  localCapacidadeTotal: number
  estaAtivo: boolean
}

export interface ListarSalasRequest {
  estaAtivo?: boolean
}

export type ListarSalasItemResponse = ObterLocalSalaResponse

export interface AdicionarSalaRequest {
  salaNome: string
  salaCapacidade: number
  salaTipo: SalaTipo
  salaRecursos?: string | null
}

export interface AdicionarSalaResponse {
  id: string
  localId: string
  salaNome: string
  salaCapacidade: number
  salaTipo: SalaTipo
}

export interface AtualizarSalaRequest extends AdicionarSalaRequest {
  estaAtivo: boolean
}

export interface AtualizarSalaResponse extends AdicionarSalaResponse {
  estaAtivo: boolean
}

// ---------------------------------------------------------------------------
// Eventos
// ---------------------------------------------------------------------------

export type EventoFormato = 'Presencial' | 'Remoto' | 'Hibrido'
export type EventoSituacao = 'Rascunho' | 'Publicado' | 'EmAndamento' | 'Encerrado' | 'Cancelado'
export type InscricaoSituacao = 'Confirmada' | 'Cancelada'

export interface CriarEventoRequest {
  eventoNome: string
  eventoDescricao?: string | null
  eventoDataInicio: string
  eventoDataFim: string
  eventoFormato: EventoFormato
  localId?: string | null
  eventoLinkRemoto?: string | null
  eventoCapacidadeMaxima?: number | null
  trilhas?: CriarEventoTrilhaRequest[]
}

export interface CriarEventoTrilhaRequest { trilhaNome: string; trilhaDescricao?: string | null; trilhaCor?: string | null }
export interface ObterEventoTrilhaResponse extends CriarEventoTrilhaRequest { id: string; estaAtivo: boolean }

export interface CriarEventoResponse {
  id: string
  eventoNome: string
  eventoSituacao: EventoSituacao
}

export type AtualizarEventoRequest = CriarEventoRequest

export interface AtualizarEventoResponse {
  id: string
  eventoNome: string
  eventoSituacao: EventoSituacao
  alteradoEm: string | null
}

export interface AlterarSituacaoEventoRequest {
  eventoSituacao: EventoSituacao
  motivo?: string | null
}

export interface AlterarSituacaoEventoResponse {
  id: string
  eventoSituacao: EventoSituacao
}

export interface ObterEventoResponse {
  id: string
  eventoNome: string
  eventoDescricao: string | null
  eventoDataInicio: string
  eventoDataFim: string
  eventoFormato: EventoFormato
  eventoSituacao: EventoSituacao
  localId: string | null
  localNome: string | null
  eventoLinkRemoto: string | null
  eventoCapacidadeMaxima: number | null
  inscricoesConfirmadas: number
  eventoCancelamentoMotivo: string | null
  criadoEm: string
  alteradoEm: string | null
  trilhas: ObterEventoTrilhaResponse[]
}

export interface ListarEventosRequest extends PagedRequest {
  busca?: string
  eventoSituacao?: EventoSituacao
  eventoFormato?: EventoFormato
  dataInicioDe?: string
  dataInicioAte?: string
}

export interface ListarEventosItemResponse {
  id: string
  eventoNome: string
  eventoDataInicio: string
  eventoDataFim: string
  eventoFormato: EventoFormato
  eventoSituacao: EventoSituacao
  localId: string | null
  inscricoesConfirmadas: number
}

export interface InscreverParticipanteRequest {
  pessoaId: string
}

export interface InscreverParticipanteResponse {
  id: string
  eventoId: string
  pessoaId: string
  inscricaoSituacao: InscricaoSituacao
  inscricaoRealizadaEm: string
}

export interface ListarInscricoesRequest extends PagedRequest {
  inscricaoSituacao?: InscricaoSituacao
}

export interface ListarInscricoesItemResponse {
  id: string
  pessoaId: string
  pessoaNome: string
  pessoaEmail: string
  inscricaoSituacao: InscricaoSituacao
  inscricaoRealizadaEm: string
}

// ---------------------------------------------------------------------------
// Palestras
// ---------------------------------------------------------------------------

export type PalestrantePapel = 'Principal' | 'Coautor' | 'Mediador'
export type ConteudoTipo = 'Slides' | 'Pdf' | 'Arquivo' | 'Link' | 'Video' | 'Imagem'

export interface PalestranteRequest {
  pessoaId: string
  palestrantePapel: PalestrantePapel
}

export interface CriarPalestraRequest {
  eventoId: string
  trilhaId: string
  salaId?: string | null
  palestraTitulo: string
  palestraDescricao?: string | null
  palestraInicio: string
  palestraFim: string
  palestrantes: PalestranteRequest[]
}

export interface CriarPalestraResponse {
  id: string
  eventoId: string
  palestraTitulo: string
}

export interface AtualizarPalestraRequest {
  trilhaId: string
  salaId?: string | null
  palestraTitulo: string
  palestraDescricao?: string | null
  palestraInicio: string
  palestraFim: string
}

export interface AtualizarPalestraResponse {
  id: string
  palestraTitulo: string
  alteradoEm: string | null
}

export interface PalestraPalestranteResponse {
  pessoaId: string
  pessoaNome: string
  palestrantePapel: PalestrantePapel
}

export interface PalestraConteudoResponse {
  id: string
  conteudoTitulo: string
  conteudoTipo: ConteudoTipo
  conteudoUrl: string
  conteudoDescricao: string | null
}

export interface ObterPalestraResponse {
  id: string
  eventoId: string
  eventoNome: string
  trilhaId: string
  trilhaNome: string
  salaId: string | null
  salaNome: string | null
  palestraTitulo: string
  palestraDescricao: string | null
  palestraInicio: string
  palestraFim: string
  palestraCargaHorariaMinutos: number
  palestrantes: PalestraPalestranteResponse[]
  conteudos: PalestraConteudoResponse[]
  presencasQuantidade: number
  certificadosQuantidade: number
  criadoEm: string
  alteradoEm: string | null
}

export interface ListarPalestrasRequest extends PagedRequest {
  eventoId?: string
  busca?: string
}

export interface ListarPalestrasItemResponse {
  id: string
  eventoId: string
  trilhaId: string
  salaId: string | null
  palestraTitulo: string
  palestraInicio: string
  palestraFim: string
  palestrantesQuantidade: number
  presencasQuantidade: number
}

export type AdicionarPalestranteRequest = PalestranteRequest

export interface AdicionarPalestranteResponse {
  palestraId: string
  pessoaId: string
  palestrantePapel: PalestrantePapel
}

export interface AdicionarConteudoRequest {
  conteudoTitulo: string
  conteudoTipo: ConteudoTipo
  conteudoUrl: string
  conteudoDescricao?: string | null
}

export interface AdicionarConteudoResponse {
  id: string
  palestraId: string
  conteudoTitulo: string
  conteudoTipo: ConteudoTipo
  conteudoUrl: string
}

export interface RegistrarPresencaRequest {
  pessoaId: string
}

export interface RegistrarPresencaResponse {
  id: string
  palestraId: string
  pessoaId: string
  presencaRegistradaEm: string
}

export interface ListarPresencasItemResponse {
  id: string
  pessoaId: string
  pessoaNome: string
  presencaRegistradaEm: string
  certificadoEmitido: boolean
}

export interface EmitirCertificadoRequest {
  pessoaId: string
}

export interface EmitirCertificadoResponse {
  id: string
  certificadoCodigo: string
  palestraId: string
  palestraTitulo: string
  pessoaId: string
  pessoaNome: string
  certificadoEmitidoEm: string
  certificadoCargaHorariaMinutos: number
}

export interface ValidarCertificadoResponse {
  certificadoCodigo: string
  palestraTitulo: string
  eventoNome: string
  pessoaNome: string
  palestraInicio: string
  certificadoEmitidoEm: string
  certificadoCargaHorariaMinutos: number
}

// ---------------------------------------------------------------------------
// Auditoria
// ---------------------------------------------------------------------------

export interface ListarRegistrosAuditoriaRequest extends PagedRequest {
  modulo?: string
  entidadeNome?: string
  entidadeId?: string
  usuarioId?: string
  operacao?: string
  ocorridoDe?: string
  ocorridoAte?: string
}

export interface ListarRegistrosAuditoriaItemResponse {
  id: string
  modulo: string
  entidadeNome: string
  entidadeId: string
  operacao: string
  usuarioNome: string | null
  traceId: string | null
  ocorridoEm: string
}

export interface ObterRegistroAuditoriaResponse {
  id: string
  modulo: string
  entidadeNome: string
  entidadeId: string
  operacao: string
  /** JSON serializado (string) com o estado anterior da entidade. */
  dadosAnteriores: string | null
  /** JSON serializado (string) com o estado novo da entidade. */
  dadosNovos: string | null
  usuarioId: string | null
  usuarioNome: string | null
  traceId: string | null
  ocorridoEm: string
  registradoEm: string
}
