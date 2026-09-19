# Especificação da API v1 — endpoints e DTOs por módulo

Fonte de verdade para implementação dos módulos e do front. Convenções gerais em `CLAUDE.md`.
Todos os campos JSON em camelCase (o .NET serializa `PessoaNome` como `pessoaNome`). Enums serializam como string.
Datas são `DateTimeOffset` ISO-8601. Listagens retornam `PagedResult<T> { itens[], pagina, tamanhoPagina, total, totalPaginas }`
e aceitam `pagina` (>=1) e `tamanhoPagina` (1..100) na query string.

Políticas: **[anon]** anônimo · **[auth]** autenticado · **[gestao]** `Politicas.Gestao` (Administrador ou Organizador) · **[admin]** `Politicas.Administracao`.

Erros: ProblemDetails com `codigo` (`Modulo.Motivo`) e `traceId`. 400 validação (`errors`), 401, 403, 404 NotFound, 409 Conflict, 422 BusinessRule.

---

## Identidade — `api/v1/identidade` (schema `Identidade`)

Entidades (ASP.NET Core Identity sobrescrito, `Guid` keys, tabelas PascalCase no schema `Identidade`):
`Usuario : IdentityUser<Guid>, IEntidadeAuditavel, IEmissorDeEventos` (+ `UsuarioNome`, `UltimoAcessoEm`) → tabela `Usuarios`;
`Perfil : IdentityRole<Guid>` (+ `PerfilDescricao`) → `Perfis`; `UsuarioPerfil : IdentityUserRole<Guid>` → `UsuarioPerfis`;
`UsuarioClaim` → `UsuarioClaims`; `UsuarioLogin` → `UsuarioLogins`; `UsuarioToken` → `UsuarioTokens`; `PerfilClaim` → `PerfilClaims`.
`IdentidadeDbContext : IdentityDbContext<Usuario, Perfil, Guid, UsuarioClaim, UsuarioPerfil, UsuarioLogin, PerfilClaim, UsuarioToken>` — **precisa também** herdar o comportamento de `ModuleDbContext` (schema, Outbox, soft delete). Como não há herança múltipla, `IdentidadeDbContext` herda de `IdentityDbContext<...>` e replica o essencial: `HasDefaultSchema("Identidade")`, `DbSet<OutboxMessage>` configurado igual ao `ModuleDbContext` (extraia a configuração do Outbox para um método estático reutilizável em `Shared.Data` se necessário — permitido alterar `Shared.Data` APENAS para isso), filtro de soft delete em `Usuario`, e registro via `AddModuleDbContext`-equivalente (pode criar `AddIdentityModuleDbContext` dentro do módulo replicando a lógica: pool, interceptors, `IOutboxStore`, registry).
Seed na subida (hosted service do módulo, roda após o migrador): perfis `Administrador`, `Organizador`, `Participante` (de `PerfisPadrao`) e o administrador inicial de `Identidade:AdministradorInicial` (`Email`, `Nome`, `Senha`) — só cria se não existir; senha só via configuração/variável de ambiente.
Senhas: política mínima 8 caracteres, maiúscula, minúscula, dígito e símbolo. Lockout após 5 tentativas (5 min). JWT emitido com `JwtOptions` (Shared.WebHost): claims `sub`, `name`, `email`, `role` (uma por perfil), `jti`; HS256.
Login atualiza `UltimoAcessoEm` e registra `UsuarioAutenticado`; registro de usuário registra `UsuarioRegistrado` (eventos em `Shared.Contracts.Identidade`).

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| POST | `/sessoes` | anon | `CriarSessaoRequest { usuarioEmail, senha }` | 200 `CriarSessaoResponse { accessToken, expiraEm, usuario: { id, usuarioNome, usuarioEmail, perfis[] } }`; 401 `Identidade.CredenciaisInvalidas`; 423→use 401 `Identidade.UsuarioBloqueado` |
| POST | `/usuarios` | admin | `RegistrarUsuarioRequest { usuarioNome, usuarioEmail, senha, perfis[] }` | 201 `RegistrarUsuarioResponse { id, usuarioNome, usuarioEmail, perfis[] }`; 409 `Identidade.EmailJaCadastrado`; 422 `Identidade.PerfilInvalido` / `Identidade.SenhaFraca` |
| GET | `/usuarios/me` | auth | — | 200 `ObterUsuarioAtualResponse { id, usuarioNome, usuarioEmail, perfis[], ultimoAcessoEm }` |
| GET | `/usuarios` | admin | `ListarUsuariosRequest { busca?, estaAtivo?, pagina, tamanhoPagina }` | `PagedResult<ListarUsuariosItemResponse { id, usuarioNome, usuarioEmail, perfis[], estaAtivo, ultimoAcessoEm }>` |
| PUT | `/usuarios/{id}/perfis` | admin | `AtualizarPerfisUsuarioRequest { perfis[] }` | 200 `AtualizarPerfisUsuarioResponse { id, perfis[] }`; 404 `Identidade.UsuarioNaoEncontrado` |

Rate limit adicional no login (política de rate limiting específica, ex.: 10/min por IP) é desejável.

---

## Pessoas — `api/v1/pessoas` (schema `Pessoas`)

`Pessoa : EntidadeBase { PessoaNome, PessoaEmail (único entre ativos, lower-case), PessoaTelefone?, PessoaDocumento? (CPF só dígitos, único entre ativos), PessoaEmpresa?, PessoaCargo?, PessoaMiniBio? (2000), PessoaFotoUrl? }`.
Palestrantes e participantes são Pessoas; o papel nasce do relacionamento (Palestras/Eventos). Eventos: `PessoaCriada`, `PessoaExcluida`.
Implementa `IPessoasModuleApi` (Shared.Contracts): `ObterPessoaResumoAsync`, `ObterPessoasResumoAsync` (batch, apenas ativos não excluídos).

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| POST | `` | gestao | `CriarPessoaRequest { pessoaNome, pessoaEmail, pessoaTelefone?, pessoaDocumento?, pessoaEmpresa?, pessoaCargo?, pessoaMiniBio?, pessoaFotoUrl? }` | 201 `CriarPessoaResponse { id, pessoaNome, pessoaEmail }`; 409 `Pessoas.EmailJaCadastrado`, `Pessoas.DocumentoJaCadastrado` |
| PUT | `/{id}` | gestao | `AtualizarPessoaRequest { mesmos campos + estaAtivo }` | 200 `AtualizarPessoaResponse { id, pessoaNome, pessoaEmail, estaAtivo, alteradoEm }`; 404 `Pessoas.PessoaNaoEncontrada` |
| DELETE | `/{id}` | gestao | — | 204 |
| GET | `/{id}` | auth | — | 200 `ObterPessoaResponse { id, pessoaNome, pessoaEmail, pessoaTelefone, pessoaDocumento, pessoaEmpresa, pessoaCargo, pessoaMiniBio, pessoaFotoUrl, estaAtivo, criadoEm, alteradoEm }` |
| GET | `` | auth | `ListarPessoasRequest { busca? (nome/email/empresa), estaAtivo?, pagina, tamanhoPagina }` | `PagedResult<ListarPessoasItemResponse { id, pessoaNome, pessoaEmail, pessoaEmpresa, pessoaCargo, estaAtivo }>` |

---

## Eventos — `api/v1/eventos` (schema `Eventos`)

`Evento : EntidadeBase { EventoNome, EventoDescricao? (4000), EventoDataInicio, EventoDataFim, EventoFormato (enum Presencial|Remoto|Hibrido), LocalId?, EventoLinkRemoto?, EventoSituacao (enum Rascunho|Publicado|EmAndamento|Encerrado|Cancelado), EventoCapacidadeMaxima?, EventoCancelamentoMotivo? }` com coleção `Inscricoes`.
`Inscricao : EntidadeBase { EventoId, PessoaId, InscricaoSituacao (enum Confirmada|Cancelada), InscricaoRealizadaEm, InscricaoCanceladaEm? }`. Índice único `(EventoId, PessoaId)` filtrado por `InscricaoSituacao = 'Confirmada'`.

Regras (erros com prefixo `Eventos.`):
- `EventoDataFim > EventoDataInicio` (validator). Presencial/Híbrido exigem `localId` existente (`ILocaisModuleApi`, senão 422 `LocalNaoEncontrado`); Remoto exige `eventoLinkRemoto` e `localId` nulo; Híbrido exige ambos (422 `FormatoInconsistente`).
- Transições (`PATCH /{id}/situacao`): Rascunho→Publicado (exige ≥1 palestra via `IPalestrasModuleApi.ContarPalestrasDoEventoAsync`, senão 422 `EventoSemPalestras`); Publicado→EmAndamento; Publicado|EmAndamento→Encerrado; Rascunho|Publicado|EmAndamento→Cancelado (motivo obrigatório: 422 `MotivoCancelamentoObrigatorio`). Qualquer outra: 422 `TransicaoSituacaoInvalida`. Publicar emite `EventoPublicado`; cancelar emite `EventoCancelado`.
- Atualizar só em Rascunho ou Publicado (422 `EventoNaoPodeSerAlterado`). Excluir só Rascunho ou Cancelado (422 `EventoNaoPodeSerExcluido`).
- Inscrever: evento Publicado ou EmAndamento (422 `EventoNaoAceitaInscricoes`); pessoa existe (`IPessoasModuleApi`, 422 `PessoaNaoEncontrada`); sem inscrição confirmada prévia (409 `PessoaJaInscrita`); capacidade = `EventoCapacidadeMaxima` ou, se nula e há local, `LocalCapacidadeTotal` do `LocalResumo`; se atingida 422 `CapacidadeEsgotada`. Emite `InscricaoRealizada`.
- Cancelar inscrição (DELETE): muda situação para Cancelada (não é soft delete) e emite `InscricaoCancelada`; 404 `InscricaoNaoEncontrada`; já cancelada → 422 `InscricaoJaCancelada`.
Implementa `IEventosModuleApi`: `ObterEventoResumoAsync` (formato/situação como string do enum), `InscricaoConfirmadaExisteAsync`.

**Trilhas** (detalhes em [`trilhas.md`](trilhas.md)): um evento possui trilhas (`Trilha : EntidadeBase { EventoId, TrilhaNome, TrilhaDescricao?, TrilhaCor? }`); toda palestra pertence a uma trilha do seu evento.
`CriarEventoRequest` aceita `trilhas: [{ trilhaNome, trilhaDescricao?, trilhaCor? }]` opcional e `ObterEventoResponse` devolve `trilhas: [{ id, trilhaNome, trilhaDescricao, trilhaCor, estaAtivo }]`.

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| GET | `/{id}/trilhas` | auth | — | `IReadOnlyList<ListarTrilhasItemResponse { id, eventoId, trilhaNome, trilhaDescricao, trilhaCor, estaAtivo }>` |
| POST | `/{id}/trilhas` | gestao | `AdicionarTrilhaRequest { trilhaNome, trilhaDescricao?, trilhaCor? }` | 201 `AdicionarTrilhaResponse { id, eventoId, trilhaNome, trilhaDescricao, trilhaCor }` |
| PUT | `/{id}/trilhas/{trilhaId}` | gestao | `AtualizarTrilhaRequest { trilhaNome, trilhaDescricao?, trilhaCor?, estaAtivo }` | 200 `AtualizarTrilhaResponse { ... }` |
| DELETE | `/{id}/trilhas/{trilhaId}` | gestao | — | 204 |

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| POST | `` | gestao | `CriarEventoRequest { eventoNome, eventoDescricao?, eventoDataInicio, eventoDataFim, eventoFormato, localId?, eventoLinkRemoto?, eventoCapacidadeMaxima? }` | 201 `CriarEventoResponse { id, eventoNome, eventoSituacao }` |
| PUT | `/{id}` | gestao | `AtualizarEventoRequest { mesmos campos }` | 200 `AtualizarEventoResponse { id, eventoNome, eventoSituacao, alteradoEm }` |
| PATCH | `/{id}/situacao` | gestao | `AlterarSituacaoEventoRequest { eventoSituacao, motivo? }` | 200 `AlterarSituacaoEventoResponse { id, eventoSituacao }` |
| DELETE | `/{id}` | gestao | — | 204 |
| GET | `/{id}` | auth | — | 200 `ObterEventoResponse { id, eventoNome, eventoDescricao, eventoDataInicio, eventoDataFim, eventoFormato, eventoSituacao, localId, localNome?, eventoLinkRemoto, eventoCapacidadeMaxima, inscricoesConfirmadas, eventoCancelamentoMotivo, criadoEm, alteradoEm }` (`localNome` via `ILocaisModuleApi`) |
| GET | `` | auth | `ListarEventosRequest { busca?, eventoSituacao?, eventoFormato?, dataInicioDe?, dataInicioAte?, pagina, tamanhoPagina }` | `PagedResult<ListarEventosItemResponse { id, eventoNome, eventoDataInicio, eventoDataFim, eventoFormato, eventoSituacao, localId, inscricoesConfirmadas }>` ordenado por `eventoDataInicio` desc |
| POST | `/{id}/inscricoes` | auth | `InscreverParticipanteRequest { pessoaId }` | 201 `InscreverParticipanteResponse { id, eventoId, pessoaId, inscricaoSituacao, inscricaoRealizadaEm }` |
| DELETE | `/{id}/inscricoes/{inscricaoId}` | auth | — | 204 |
| GET | `/{id}/inscricoes` | auth | `ListarInscricoesRequest { inscricaoSituacao?, pagina, tamanhoPagina }` | `PagedResult<ListarInscricoesItemResponse { id, pessoaId, pessoaNome, pessoaEmail, inscricaoSituacao, inscricaoRealizadaEm }>` (nomes via `IPessoasModuleApi.ObterPessoasResumoAsync` em lote) |

---

## Palestras — `api/v1/palestras` (schema `Palestras`)

`Palestra : EntidadeBase { EventoId, SalaId?, PalestraTitulo, PalestraDescricao? (4000), PalestraInicio, PalestraFim }` com coleções `Palestrantes`, `Conteudos`, `Presencas`, `Certificados`.
`PalestraPalestrante : EntidadeBase { PalestraId, PessoaId, PalestrantePapel (enum Principal|Coautor|Mediador) }` — único `(PalestraId, PessoaId)` entre ativos.
`PalestraConteudo : EntidadeBase { PalestraId, ConteudoTitulo, ConteudoTipo (enum Slides|Pdf|Arquivo|Link|Video|Imagem), ConteudoUrl (2000), ConteudoDescricao? }`.
`Presenca : EntidadeBase { PalestraId, PessoaId, PresencaRegistradaEm }` — único `(PalestraId, PessoaId)` entre ativos.
`Certificado : EntidadeBase { PalestraId, PessoaId, CertificadoCodigo (único, 12 chars A-Z0-9 sem ambíguos), CertificadoEmitidoEm, CertificadoCargaHorariaMinutos }`.
`PalestraCargaHorariaMinutos` = `(PalestraFim - PalestraInicio).TotalMinutes` (calculado, não persistido).

Regras (erros `Palestras.`):
- Evento existe e situação ∉ {Encerrado, Cancelado} para criar/alterar (`IEventosModuleApi`; 422 `EventoNaoEncontrado` / `EventoNaoAceitaPalestras`). Período da palestra dentro do período do evento (422 `PeriodoForaDoEvento`); `PalestraFim > PalestraInicio` (validator).
- `salaId` opcional; se informado: sala existe, ativa e `SalaResumo.LocalId == EventoResumo.LocalId` (422 `SalaNaoPertenceAoLocal`); sem sobreposição de horário com outra palestra ativa na mesma sala (409 `SalaOcupada`).
- Criar exige ≥1 palestrante (validator); cada pessoa existe (`IPessoasModuleApi`, 422 `PessoaNaoEncontrada`); pessoa não repetida (409 `PalestranteJaVinculado`); não remover o último (422 `PalestraPrecisaDePalestrante`).
- Presença: pessoa com inscrição confirmada no evento (`IEventosModuleApi.InscricaoConfirmadaExisteAsync`, 422 `ParticipanteNaoInscrito`); única (409 `PresencaJaRegistrada`). Emite `PresencaRegistrada`.
- Certificado: exige presença (422 `PresencaNaoRegistrada`) e palestra encerrada (`PalestraFim <= agora`, 422 `PalestraNaoEncerrada`). Idempotente: se já emitido, retorna 200 com o existente. Emite `CertificadoEmitido`.
- Excluir palestra: soft delete em cascata lógica de palestrantes/conteúdos (presenças e certificados permanecem). Criar emite `PalestraCriada`.
Implementa `IPalestrasModuleApi.ContarPalestrasDoEventoAsync` (palestras ativas não excluídas).

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| POST | `` | gestao | `CriarPalestraRequest { eventoId, trilhaId, salaId?, palestraTitulo, palestraDescricao?, palestraInicio, palestraFim, palestrantes: [{ pessoaId, palestrantePapel }] }` | 201 `CriarPalestraResponse { id, eventoId, palestraTitulo }` |
| PUT | `/{id}` | gestao | `AtualizarPalestraRequest { trilhaId, salaId?, palestraTitulo, palestraDescricao?, palestraInicio, palestraFim }` | 200 `AtualizarPalestraResponse { id, palestraTitulo, alteradoEm }` |
| DELETE | `/{id}` | gestao | — | 204 |
| GET | `/{id}` | auth | — | 200 `ObterPalestraResponse { id, eventoId, eventoNome, trilhaId, trilhaNome, salaId, salaNome, palestraTitulo, palestraDescricao, palestraInicio, palestraFim, palestraCargaHorariaMinutos, palestrantes: [{ pessoaId, pessoaNome, palestrantePapel }], conteudos: [{ id, conteudoTitulo, conteudoTipo, conteudoUrl, conteudoDescricao }], presencasQuantidade, certificadosQuantidade, criadoEm, alteradoEm }` |
| GET | `` | auth | `ListarPalestrasRequest { eventoId?, busca?, pagina, tamanhoPagina }` | `PagedResult<ListarPalestrasItemResponse { id, eventoId, trilhaId, salaId, palestraTitulo, palestraInicio, palestraFim, palestrantesQuantidade, presencasQuantidade }>` ordenado por `palestraInicio` |
| POST | `/{id}/palestrantes` | gestao | `AdicionarPalestranteRequest { pessoaId, palestrantePapel }` | 201 `AdicionarPalestranteResponse { palestraId, pessoaId, palestrantePapel }` |
| DELETE | `/{id}/palestrantes/{pessoaId}` | gestao | — | 204 |
| POST | `/{id}/conteudos` | gestao | `AdicionarConteudoRequest { conteudoTitulo, conteudoTipo, conteudoUrl, conteudoDescricao? }` | 201 `AdicionarConteudoResponse { id, palestraId, conteudoTitulo, conteudoTipo, conteudoUrl }` |
| DELETE | `/{id}/conteudos/{conteudoId}` | gestao | — | 204 |
| POST | `/{id}/presencas` | gestao | `RegistrarPresencaRequest { pessoaId }` | 201 `RegistrarPresencaResponse { id, palestraId, pessoaId, presencaRegistradaEm }` |
| GET | `/{id}/presencas` | auth | — | 200 `IReadOnlyList<ListarPresencasItemResponse { id, pessoaId, pessoaNome, presencaRegistradaEm, certificadoEmitido }>` |
| POST | `/{id}/certificados` | auth | `EmitirCertificadoRequest { pessoaId }` | 201 (ou 200 se já existia) `EmitirCertificadoResponse { id, certificadoCodigo, palestraId, palestraTitulo, pessoaId, pessoaNome, certificadoEmitidoEm, certificadoCargaHorariaMinutos }` |
| GET | `/certificados/{codigo}` | anon | — | 200 `ValidarCertificadoResponse { certificadoCodigo, palestraTitulo, eventoNome, pessoaNome, palestraInicio, certificadoEmitidoEm, certificadoCargaHorariaMinutos }`; 404 `CertificadoNaoEncontrado` |

---

## Auditoria — `api/v1/auditoria` (schema `Auditoria`)

`RegistroAuditoria` (NÃO herda `EntidadeBase`; é imutável): `{ Id (Guid v7), Modulo, EntidadeNome, EntidadeId, Operacao, DadosAnteriores? (jsonb), DadosNovos? (jsonb), UsuarioId?, UsuarioNome?, TraceId?, OcorridoEm, RegistradoEm }`.
`AuditoriaDbContext.AuditChangesEnabled => false` (evita recursão). Handler `EntidadeAlteradaHandler : IIntegrationEventHandler<EntidadeAlterada>` (registrado com `AddIntegrationEventHandler`) persiste o registro; idempotente por `Id` do evento (use o `Id` do evento como PK do registro).
Índices: `(Modulo, EntidadeNome, EntidadeId)`, `OcorridoEm`, `UsuarioId`.

| Método | Rota | Política | Request | Response |
|---|---|---|---|---|
| GET | `/registros` | admin | `ListarRegistrosAuditoriaRequest { modulo?, entidadeNome?, entidadeId?, usuarioId?, operacao?, ocorridoDe?, ocorridoAte?, pagina, tamanhoPagina }` | `PagedResult<ListarRegistrosAuditoriaItemResponse { id, modulo, entidadeNome, entidadeId, operacao, usuarioNome, traceId, ocorridoEm }>` ordenado por `ocorridoEm` desc |
| GET | `/registros/{id}` | admin | — | 200 `ObterRegistroAuditoriaResponse { id, modulo, entidadeNome, entidadeId, operacao, dadosAnteriores (string JSON), dadosNovos (string JSON), usuarioId, usuarioNome, traceId, ocorridoEm, registradoEm }` |

---

## Locais — `api/v1/locais` (schema `Locais`) — IMPLEMENTADO (referência)

Ver `api/src/modules/Module.Locais` e `docs/contracts/v1/openapi.yaml`.
