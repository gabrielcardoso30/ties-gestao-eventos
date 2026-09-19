# Glossário

Regra geral do projeto: **domínio e negócio em português brasileiro; técnico em inglês.** Um nome de classe, propriedade, tabela ou coluna que represente um conceito do negócio é escrito em pt-BR (`Evento`, `PalestraTitulo`); um conceito de infraestrutura mantém o nome consagrado em inglês (`OutboxMessage`, `UseCase`, `Endpoint`).

## Regra de nomenclatura `EntidadeAtributo`

Toda propriedade que qualifica uma entidade começa pelo nome da entidade, seguido do atributo: `PessoaNome`, `EventoSituacao`, `SalaCapacidade`, `CertificadoCodigo`. Nunca `NomePessoa` ou `SituacaoEvento`. A regra vale para propriedades C#, colunas (PascalCase no PostgreSQL), campos JSON (camelCase: `pessoaNome`) e tipos do front. Campos de auditoria (`CriadoEm`, `AlteradoPor`, `ExcluidoEm`, `EstaAtivo`) e chaves (`Id`, `LocalId`, `PessoaId`) são exceções por convenção de infraestrutura.

## Linguagem ubíqua (negócio)

| Termo | Definição | Módulo dono |
|---|---|---|
| **Evento** | Acontecimento com nome, período (`EventoDataInicio`/`EventoDataFim`), formato (Presencial, Remoto, Híbrido), situação e, quando presencial ou híbrido, um Local. Agrega Inscrições. | Eventos |
| **Situação do Evento** | Estado do ciclo de vida: `Rascunho`, `Publicado`, `EmAndamento`, `Encerrado`, `Cancelado`. Transições controladas (ver [business-rules/eventos.md](business-rules/eventos.md)). | Eventos |
| **Formato do Evento** | `Presencial` (exige Local), `Remoto` (exige link), `Hibrido` (exige ambos). | Eventos |
| **Trilha** | Linha temática de um Evento que agrupa palestras por assunto. Um evento mantém uma ou mais trilhas e cada palestra pertence a exatamente uma delas. | Eventos |
| **Inscrição** | Vínculo de uma Pessoa (participante) a um Evento. Situações `Confirmada` ou `Cancelada`. Só uma inscrição confirmada por pessoa e evento. | Eventos |
| **Participante** | Pessoa com inscrição confirmada em um evento. Não é uma entidade própria: é um papel da Pessoa. | Eventos (papel) / Pessoas (dados) |
| **Palestra** | Sessão dentro de um Evento com título, período dentro do período do evento, Sala opcional, um ou mais Palestrantes e Conteúdos. | Palestras |
| **Palestrante** | Pessoa vinculada a uma Palestra com um papel (`Principal`, `Coautor`, `Mediador`). Entidade `PalestraPalestrante`. | Palestras (papel) / Pessoas (dados) |
| **Conteúdo da Palestra** | Material associado: `Slides`, `Pdf`, `Arquivo`, `Link`, `Video`, `Imagem`, com URL. | Palestras |
| **Presença** | Registro de que um participante inscrito no evento compareceu a uma palestra. Única por palestra e pessoa. | Palestras |
| **Certificado** | Comprovante de participação em uma palestra, emitido após presença e término da palestra. Possui `CertificadoCodigo` (12 caracteres) validável publicamente. | Palestras |
| **Carga horária** | Duração da palestra em minutos (`PalestraFim - PalestraInicio`); calculada, não persistida; copiada para o certificado. | Palestras |
| **Pessoa** | Indivíduo cadastrado (nome, e-mail, telefone, documento, empresa, cargo, minibio, foto). Palestrantes e participantes são Pessoas. | Pessoas |
| **Local** | Lugar físico onde eventos presenciais acontecem, com endereço e uma ou mais Salas. | Locais |
| **Sala / Ambiente** | Unidade alocável de um Local (auditório, sala de aula, laboratório, área de recreação, coworking). Um local de ambiente único é representado por uma sala do tipo `AmbienteUnico`. | Locais |
| **Capacidade total do Local** | Soma das capacidades das salas ativas. Usada como limite de inscrições quando o evento não define `EventoCapacidadeMaxima`. | Locais (cálculo) / Eventos (uso) |
| **Usuário** | Conta de acesso ao sistema (Identity sobrescrito). Diferente de Pessoa: um usuário opera o sistema; uma pessoa participa de eventos. | Identidade |
| **Perfil** | Papel de acesso: `Administrador`, `Organizador`, `Participante` (`PerfisPadrao`). | Identidade |
| **Sessão** | Autenticação bem-sucedida que resulta em um JWT (`POST /api/v1/identidade/sessoes`). | Identidade |
| **Registro de auditoria** | Linha imutável com módulo, entidade, id, operação, dados anteriores/novos, usuário e `traceId`. | Auditoria |
| **Exclusão lógica** | Registro marcado com `ExcluidoEm`/`ExcluidoPor` e `EstaAtivo=false`, oculto das consultas pelo filtro `SoftDelete`. Ver "Soft delete". | todos |
| **Ativo / Inativo** | `EstaAtivo` indica disponibilidade para uso (ex.: sala inativa não aloca palestras); é diferente de excluído. | todos |

## Termos técnicos mantidos em inglês

| Termo | Significado no projeto | Onde |
|---|---|---|
| **Host** | Processo executável que compõe `Shared.*` e carrega os módulos. Hoje só `Host.Api`; um `Host.Worker` futuro reutilizaria as mesmas peças. | `api/src/hosts` |
| **Module** | Unidade de negócio com `Domain/`, `UseCases/`, `Shared/` e schema próprio; implementa `IModule`. | `api/src/modules/Module.<Nome>` |
| **Shared** | Bibliotecas transversais (`Shared.Contracts`, `Data`, `Http`, `Observability`, `Messaging`, `WebHost`). Um módulo só depende delas. | `api/src/shared` |
| **UseCase** | Classe `IUseCase<TRequest, TResponse>` que executa um caso de uso e devolve `Result<T>`. Sempre envolvida pelo `TelemetryUseCaseDecorator`. | `Shared.Http.Endpoints` |
| **Endpoint** | Classe `IEndpoint` com `Map` estático que registra exatamente uma rota no grupo do módulo. | `Shared.Http.Endpoints` |
| **Vertical slice** | Pasta `UseCases/<Nome>/` com Request, Response, Validator, UseCase e Endpoint juntos. | módulos |
| **Result / Error** | Padrão de retorno sem exceções para regra de negócio; `Error.Code` = `Modulo.Motivo`. | `Shared.Http.Results` |
| **ProblemDetails** | Corpo de erro RFC 9457 com extensões `codigo` e `traceId`. | `ResultHttpExtensions`, `GlobalExceptionHandler` |
| **ValidationFilter** | Filtro de endpoint que roda o validator FluentValidation e devolve 400 antes do caso de uso. | `Shared.Http.Validation` |
| **Module API** | Interface `I<X>ModuleApi` em `Shared.Contracts`: contrato síncrono de um módulo para os outros. | `Shared.Contracts` |
| **Integration Event** | Record imutável (`IntegrationEvent`) que um módulo publica para os demais, via Outbox. | `Shared.Contracts.Integracao` |
| **Outbox** | Tabela `OutboxMessages` em cada schema, gravada na mesma transação do negócio e processada pelo `OutboxProcessor`. | `Shared.Data.Outbox`, `Shared.Messaging` |
| **Handler** | `IIntegrationEventHandler<TEvent>` registrado por um módulo com `AddIntegrationEventHandler`. | `Shared.Messaging` |
| **Interceptor** | `SaveChangesInterceptor`/`DbCommandInterceptor` do EF Core: `AuditoriaSaveChangesInterceptor` e `QueryTagInterceptor`. | `Shared.Data.Interceptors` |
| **Soft delete** | Filtro global nomeado `SoftDelete` (`ExcluidoEm IS NULL`) + conversão de `Remove` em update pelo interceptor. | `ModuleDbContext` |
| **TagWith** | Comentário SQL `-- Modulo.CasoDeUso` em toda consulta, visível ao DBA em `pg_stat_activity`. | todos os casos de uso |
| **DbContext pooling** | `AddDbContextPool` por módulo; obriga serviços injetados nos interceptors a serem singleton (`CurrentUser` usa `IHttpContextAccessor`). | `AddModuleDbContext` |
| **Guid v7** | UUID ordenável por tempo usado como PK (`Guid.CreateVersion7()`), gerado na aplicação. | `EntidadeBase` |
| **PagedResult / PagedRequest** | Envelope e parâmetros padrão de paginação (`pagina`, `tamanhoPagina` ≤ 100). | `Shared.Contracts.Common` |
| **CorrelationId** | Cabeçalho `X-Correlation-Id` aceito e devolvido em toda resposta; propriedade nos logs e tag no trace. | `CorrelationIdMiddleware` |
| **TraceId / traceparent** | Identificadores W3C do OpenTelemetry; o `traceparent` é gravado na mensagem do Outbox para ligar o processamento ao request original. | `Shared.Observability`, `OutboxMessage.TraceParent` |
| **ModuleTelemetry** | `ActivitySource` + `Meter` nomeados `GestaoEventos.<Modulo>`. | `Shared.Observability.Telemetria` |
| **OTLP** | Protocolo de exportação do OpenTelemetry; destino no compose é o Aspire Dashboard. | `OTEL_EXPORTER_OTLP_ENDPOINT` |
| **Policy** | Política de autorização: `Politicas.Gestao` (Administrador ou Organizador) e `Politicas.Administracao` (Administrador). | `Shared.Contracts.Identidade` |
| **Advisory lock** | `pg_advisory_lock` usado pelo `DatabaseMigrationHostedService` para evitar migrações concorrentes. | `Shared.Data.Migracao` |
