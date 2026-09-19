# Gestão de Eventos — Monolito Modular (projeto modelo)

Projeto de referência da palestra **"Monolito modular: construindo o futuro de forma simples"**.
ASP.NET Core 10 + PostgreSQL (`api/`) e React + Vite + shadcn (`front/`). Documentação em `docs/`.

## Comandos

```bash
docker compose up --build            # tudo: postgres, otel (Aspire Dashboard), api, front
cd api && dotnet build                # compila a solução (GestaoEventos.slnx)
cd api && dotnet run --project src/hosts/Host.Api   # http://localhost:5080/swagger (precisa do postgres do compose)
cd api && dotnet test                 # unit + integration (Testcontainers) + functional (Reqnroll)
cd api && dotnet ef migrations add <Nome> --project src/modules/Module.<X> --startup-project src/modules/Module.<X> --context <X>DbContext --output-dir Migrations
cd front && npm run dev               # http://localhost:5173 (proxy /api -> http://localhost:5080)
```

## Estrutura

```
api/src/hosts/Host.Api            # único host hoje (futuro Host.Worker reaproveita Shared.*)
api/src/modules/Module.<Nome>     # Locais, Pessoas, Eventos, Palestras, Identidade, Auditoria
api/src/shared/Shared.Contracts   # contratos entre módulos: interfaces síncronas (I<X>ModuleApi) + eventos de integração
api/src/shared/Shared.Data        # EntidadeBase, ModuleDbContext, interceptors (auditoria/soft delete/outbox), migrador, paginação
api/src/shared/Shared.Http        # Result/Error -> ProblemDetails, IEndpoint, IUseCase, ValidationFilter, decorator de telemetria
api/src/shared/Shared.Observability  # Serilog + OpenTelemetry (OTLP e Application Insights), ModuleTelemetry, correlação
api/src/shared/Shared.Messaging   # publicador in-process + OutboxProcessor
api/src/shared/Shared.WebHost     # IModule, descoberta de módulos, OpenAPI/Swagger, JWT, rate limit, CORS, health
api/tests/Tests.Unit | Tests.Integration | Tests.Functional (Reqnroll)
front/src/modules/<modulo>/<use-case>/   # páginas por módulo e caso de uso
front/src/shared/components/ui           # shadcn; front/src/shared/components/generic: componentes genéricos das telas
docs/spec | adr | business-rules | runbooks | glossary.md | contracts/v1/openapi.yaml
```

## Regras inegociáveis (módulo de referência: `api/src/modules/Module.Locais`)

1. **Módulo** = 3 pastas: `UseCases/`, `Domain/`, `Shared/`. Um módulo só referencia `Shared.*`; NUNCA outro `Module.*`.
   Comunicação entre módulos: síncrona via `I<X>ModuleApi` (Shared.Contracts) ou assíncrona via eventos de integração (Outbox).
2. **Caso de uso** = pasta `UseCases/<NomeCasoDeUso>/` com `<Nome>Request.cs`, `<Nome>Response.cs`, `<Nome>Validator.cs`, `<Nome>UseCase.cs`, `<Nome>Endpoint.cs`.
   `UseCase : IUseCase<Request, Response>` (scoped, envolvido pelo decorator de telemetria), `Endpoint : IEndpoint` (estático, mapeia 1 rota).
3. **Endpoints**: prefixo `api/v1/<modulo>` via `MapModuleGroup(prefixo, Nome)`; uma única tag por módulo; REST sem verbos na URL;
   `WithName`, `WithSummary`, `WithDescription` (markdown), `Produces*`, `WithValidation<TRequest>()`; autorização por política (`Politicas.Gestao`/`Administracao`) ou `AllowAnonymous()`.
   Ids de rota entram no request via `request with { XId = id }` (propriedade `[JsonIgnore]`).
4. **Result pattern**: casos de uso retornam `Result<T>`; erros em `Domain/<Modulo>Erros.cs` com código `Modulo.Motivo`; nunca exceção para regra de negócio.
5. **Dados**: sem repositório; EF Core direto no `<Modulo>DbContext : ModuleDbContext`; schema = nome do módulo; objetos PascalCase; PK Guid v7 (`EntidadeBase`).
   Toda consulta com `.TagWith("Modulo.CasoDeUso")`; leituras com `AsNoTracking()` + `Select` projetando só o necessário; escritas dentro de `db.ExecuteInTransactionAsync(...)`.
   Soft delete automático (`db.Set.Remove` vira `ExcluidoEm/ExcluidoPor`); campos `CriadoEm/CriadoPor/AlteradoEm/AlteradoPor/ExcluidoEm/ExcluidoPor/EstaAtivo` preenchidos pelo interceptor.
6. **Nomenclatura**: domínio/negócio em pt-BR, técnico em inglês. Propriedades no padrão `EntidadeAtributo` (`PessoaNome`, `EventoSituacao`, `SalaCapacidade`), nunca `NomePessoa`.
7. **Auditoria e Outbox**: eventos de integração são registrados no agregado (`RegistrarEvento`) e gravados no Outbox na mesma transação; toda alteração gera `EntidadeAlterada` automaticamente (consumida pelo módulo Auditoria).
8. **Observabilidade**: `ModuleTelemetry` por módulo (`GestaoEventos.<Modulo>`), logs estruturados com TraceId/CorrelationId; nunca logar segredos ou dados pessoais desnecessários.
9. **Segurança**: JWT Bearer; escrita exige `Politicas.Gestao` (Administrador/Organizador), administração exige `Politicas.Administracao`; validação de entrada sempre; ProblemDetails sem stack trace fora de Development.
10. **Front**: páginas em `src/modules/<modulo>/<use-case>/`; telas montadas apenas com componentes genéricos (`src/shared/components/generic`) sobre shadcn; tipos alinhados a `docs/contracts/v1/openapi.yaml`.
