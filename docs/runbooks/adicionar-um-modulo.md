# Runbook — Adicionar um módulo

Use `api/src/modules/Module.Locais` como gabarito. O exemplo abaixo cria `Module.Patrocinadores` (schema `Patrocinadores`, rota `api/v1/patrocinadores`). Regras do `CLAUDE.md` valem integralmente.

## 1. Projeto

```bash
cd api/src/modules
mkdir -p Module.Patrocinadores/{Domain,UseCases,Shared/Configuracoes,Migrations}
cp Module.Locais/Module.Locais.csproj Module.Patrocinadores/Module.Patrocinadores.csproj
sed -i '' 's/Module.Locais/Module.Patrocinadores/' Module.Patrocinadores/Module.Patrocinadores.csproj
```

O csproj referencia apenas `Shared.Contracts`, `Shared.Data`, `Shared.Http`, `Shared.Observability`, `Shared.Messaging`, `Shared.WebHost` e o `Microsoft.EntityFrameworkCore.Design` (design time). **Nunca** outro `Module.*`.

Adicione ao `api/GestaoEventos.slnx` (pasta `/src/modules/`) e ao `Host.Api.csproj`:

```xml
<ProjectReference Include="../../modules/Module.Patrocinadores/Module.Patrocinadores.csproj" />
```

A referência no host só serve para copiar o DLL; `ModuleDiscovery` encontra `Module.*.dll` sozinho.

## 2. Domínio (`Domain/`)

- `Patrocinador : EntidadeBase` com construtor privado, propriedades `private set`, método estático `Criar(...)`, métodos de mutação que devolvem `Result`/`Result<T>` para regras, e `RegistrarEvento(new PatrocinadorCriado(...))` onde fizer sentido.
- Propriedades no padrão `EntidadeAtributo` (`PatrocinadorNome`, `PatrocinadorCota`).
- `PatrocinadoresErros.cs` com `Error.NotFound/Conflict/BusinessRule("Patrocinadores.Motivo", "mensagem")`.
- Enums com nomes em pt-BR (`PatrocinadorCota { Ouro, Prata, Bronze }`), persistidos como texto pela convenção.

## 3. `Shared/` do módulo

| Arquivo | Conteúdo |
|---|---|
| `PatrocinadoresDbContext.cs` | `sealed class PatrocinadoresDbContext(DbContextOptions<PatrocinadoresDbContext> options) : ModuleDbContext(options)` com `SchemaName = "Patrocinadores"`, `Schema => SchemaName`, `DbSet`s, `OnModelCreating` com `ApplyConfigurationsFromAssembly` + `base.OnModelCreating`; e `PatrocinadoresDbContextFactory : DesignTimeDbContextFactoryBase<PatrocinadoresDbContext>` |
| `Configuracoes/PatrocinadorConfiguration.cs` | `ConfigurarEntidadeBase("Patrocinadores")`, `HasMaxLength`, índices únicos com `HasFilter("\"ExcluidoEm\" IS NULL")` |
| `PatrocinadoresTelemetry.cs` | `internal static class` com `ModuleTelemetry Instance = new(PatrocinadoresDbContext.SchemaName)` |
| `PatrocinadoresModuleApi.cs` | só se outro módulo precisar consultar; implementa `IPatrocinadoresModuleApi` de `Shared.Contracts` com projeções `AsNoTracking` + `TagWith("Patrocinadores.ModuleApi.X")` |
| `PatrocinadoresModule.cs` | `IModule`: `Name`, `RoutePrefix`, `Description` (markdown), `ConfigureServices` e `MapEndpoints` iguais ao `LocaisModule` |

`ConfigureServices` mínimo:

```csharp
builder.AddModuleDbContext<PatrocinadoresDbContext>(PatrocinadoresDbContext.SchemaName);
builder.Services.AddUseCasesFromAssembly(typeof(PatrocinadoresModule).Assembly, PatrocinadoresTelemetry.Instance);
builder.Services.AddModuleValidators(typeof(PatrocinadoresModule).Assembly);
// opcional: builder.Services.AddScoped<IPatrocinadoresModuleApi, PatrocinadoresModuleApi>();
// opcional: builder.Services.AddIntegrationEventHandler<EventoCancelado, EventoCanceladoHandler>();
```

## 4. Contratos (`Shared.Contracts/Patrocinadores/`)

Somente se houver comunicação com outros módulos: `IPatrocinadoresModuleApi` + records `*Resumo`, e eventos `sealed record PatrocinadorCriado(...) : IntegrationEvent`. Eventos **precisam** estar nesse assembly para o `IntegrationEventTypeRegistry` resolvê-los.

## 5. Casos de uso (`UseCases/<Nome>/`)

Cinco arquivos por caso de uso: `CriarPatrocinadorRequest.cs`, `...Response.cs`, `...Validator.cs` (FluentValidation com `MensagensValidacao`), `...UseCase.cs` (`IUseCase<Request, Response>`, `internal sealed`, recebe o `DbContext` e Module APIs), `...Endpoint.cs` (`IEndpoint`, `internal sealed`, `Map` estático).

Checklist de cada caso de uso:

- [ ] leitura: `TagWith("Patrocinadores.<CasoDeUso>")`, `AsNoTracking()`, `Select` para o Response;
- [ ] escrita: agregado carregado, regra no domínio, `db.ExecuteInTransactionAsync(...)` + `SaveChangesAsync`;
- [ ] erros como `Result` com `PatrocinadoresErros.*`; nunca exceção;
- [ ] endpoint: rota REST sem verbo, `WithName`, `WithSummary`, `WithDescription` (markdown), `WithValidation<TRequest>()`, `Produces*`, `RequireAuthorization(Politicas.Gestao)` para escrita (leitura herda "autenticado" do grupo), `AllowAnonymous()` só com justificativa;
- [ ] ids de rota via `request with { PatrocinadorId = id }` em propriedade `[JsonIgnore]`, ou `[AsParameters]` em consultas;
- [ ] listagens com `ToPagedResultAsync` após `OrderBy`.

## 6. Migração

```bash
cd api
dotnet build
dotnet ef migrations add Inicial \
  --project src/modules/Module.Patrocinadores --startup-project src/modules/Module.Patrocinadores \
  --context PatrocinadoresDbContext --output-dir Migrations
```

Confira `EnsureSchema("Patrocinadores")`, tabela `OutboxMessages` no schema, índices `IX_*_ExcluidoEm`. Suba a API: o `DatabaseMigrationHostedService` aplica e o log mostra `Schema Patrocinadores: aplicando 1 migração(ões)`.

## 7. Testes

- `Tests.Unit`: adicionar `ProjectReference` ao módulo; testar agregado (invariantes) e validators sem banco; casos de uso com Module APIs substituídas por NSubstitute quando não tocam o banco.
- `Tests.Integration`: cenários HTTP contra `WebApplicationFactory<Program>` + Testcontainers, cobrindo cada endpoint (feliz, 400, 404/409/422, 401/403).
- `Tests.Functional`: um `.feature` Reqnroll por fluxo de negócio relevante.

## 8. Documentação e contrato

- [ ] Seção do módulo em `docs/spec/api-endpoints.md` (rotas, DTOs, erros) e regeneração/atualização de `docs/contracts/v1/openapi.yaml` (a partir de `/openapi/v1.yaml`).
- [ ] `docs/business-rules/patrocinadores.md` com `RN-PAT-001...`, invariantes e códigos de erro; linha na tabela e no ER de `docs/business-rules/README.md`.
- [ ] Termos novos em `docs/glossary.md`.
- [ ] ADR se houver decisão nova.

## 9. Front

- [ ] `front/src/modules/patrocinadores/<use-case>/` para cada caso de uso, usando apenas `src/shared/components/generic`.
- [ ] Tipos alinhados ao contrato v1; tratamento de `codigo` dos ProblemDetails do módulo.
- [ ] Rota e item de menu.

## 10. Verificação final

- [ ] `dotnet build` sem warnings novos; `dotnet test` verde.
- [ ] Swagger mostra **uma** tag `Patrocinadores` com descrição em markdown e cadeado nas operações protegidas.
- [ ] Nenhum warning `Consulta sem TagWith detectada` ao exercitar os endpoints.
- [ ] Aspire Dashboard mostra spans `Patrocinadores.<CasoDeUso>` e métricas `usecase.*` com `module=Patrocinadores`.
- [ ] `Auditoria.RegistrosAuditoria` recebe `Modulo = 'Patrocinadores'` após uma escrita.
- [ ] `Module.Patrocinadores.csproj` não referencia nenhum `Module.*`.
