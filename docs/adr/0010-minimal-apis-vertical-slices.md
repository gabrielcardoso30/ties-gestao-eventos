# ADR 0010 — Minimal APIs com um endpoint por caso de uso e vertical slices

Status: Aceita · Data: 2026-09

## Contexto

Os requisitos pedem endpoints REST agrupados por módulo (um conjunto por módulo no Swagger), prefixo `api/v1/<modulo>`, sem verbos na URL, Swagger bem documentado em markdown e cada caso de uso em uma pasta com validator, use case, endpoint e DTOs. Controllers agrupam por recurso e tendem a crescer em classes grandes com dependências cruzadas.

## Decisão

- **Vertical slice por caso de uso**: `UseCases/<Nome>/` contém `<Nome>Request`, `<Nome>Response`, `<Nome>Validator` (FluentValidation), `<Nome>UseCase : IUseCase<Request, Response>` e `<Nome>Endpoint : IEndpoint`.
- **Minimal APIs**: `IEndpoint.Map(IEndpointRouteBuilder)` estático registra **uma** rota dentro do grupo do módulo criado por `MapModuleGroup(prefixo, Nome)` (`api/v1/<modulo>`, uma tag, autorização exigida por padrão, 401/403/500 documentados). `MapEndpointsFromAssembly` descobre todos os `IEndpoint` do módulo.
- Cada endpoint declara `WithName`, `WithSummary`, `WithDescription` (markdown), `Produces*`, `WithValidation<TRequest>()` e a política (`RequireAuthorization(Politicas.Gestao)` ou `AllowAnonymous()`).
- Ids de rota entram no request via `request with { XId = id }` sobre propriedades `[JsonIgnore]`, ou por `[AsParameters]` com `[FromRoute]`/`[FromQuery]` em consultas.
- `AddUseCasesFromAssembly` registra todo `IUseCase<,>` como scoped e o envolve no `TelemetryUseCaseDecorator`.
- OpenAPI nativo (`Microsoft.AspNetCore.OpenApi`) com transformers (`DocumentoTransformer`, `SegurancaOperationTransformer`) e Swagger UI apenas como visualizador.

## Consequências

Positivas:

- tudo de um caso de uso está em uma pasta; remover uma funcionalidade é apagar a pasta;
- endpoints com uma linha de corpo; nenhuma classe cresce com o módulo;
- descoberta por reflexão: nenhum registro manual ao adicionar caso de uso;
- Swagger organizado por módulo automaticamente (`IModule.Description` vira descrição da tag).

Negativas:

- reflexão na subida (`GetTypes()` por assembly) tem custo pequeno e único; AOT/trimming exigiriam source generators;
- código comum entre casos de uso do mesmo módulo deve ir para `Domain/` (agregado) ou `Shared/` do módulo, sob risco de duplicação;
- filtros de endpoint (validação) rodam antes de `request with { ... }`, então validators não veem ids de rota injetados dessa forma; validar existência no caso de uso.

## Alternativas consideradas

- **Controllers MVC**: mais cerimônia e agrupamento por recurso; descartado.
- **FastEndpoints / Carter**: resolvem algo parecido, mas adicionam dependência e convenções próprias; o projeto precisa de ~100 linhas em `Shared.Http` para o mesmo efeito.
- **MediatR para casos de uso**: `IUseCase<,>` direto é mais explícito e permite o decorator de telemetria sem pipeline behaviors.
