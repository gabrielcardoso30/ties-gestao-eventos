# ADR 0005 — Result pattern + ProblemDetails

Status: Aceita · Data: 2026-09

## Contexto

Regras de negócio violadas (nome duplicado, capacidade esgotada, transição inválida) são fluxo esperado, não exceções. Exceções para controle de fluxo custam caro, poluem logs e obrigam o endpoint a conhecer tipos de exceção. O front e os dashboards precisam de códigos estáveis para reagir a cada erro.

## Decisão

- Casos de uso retornam `Result<TResponse>` (`Shared.Http.Results`). Falhas são `Error(Code, Message, Type)` declarados em `Domain/<Modulo>Erros.cs` com código `Modulo.Motivo` (`Locais.LocalNomeDuplicado`).
- `ErrorType` mapeia para HTTP em `ResultHttpExtensions.ToProblem`: Validation 400, NotFound 404, Conflict 409, BusinessRule 422, Unauthorized 401, Forbidden 403, Failure 500.
- Toda falha vira **ProblemDetails (RFC 9457)** com `type` (`https://gestao-eventos.globalsys.com.br/erros/<codigo>`), `title`, `detail`, `instance`, e extensões `codigo` e `traceId`.
- Validação de entrada acontece antes do caso de uso (`ValidationFilter` + FluentValidation) e devolve `ValidationProblemDetails` com `errors`.
- Exceções de verdade (bugs, infraestrutura) são tratadas uma vez pelo `GlobalExceptionHandler`: 500 sem stack trace fora de Development.

## Consequências

Positivas:

- endpoints ficam de uma linha: `(await useCase.HandleAsync(...)).ToHttpResult()`;
- o decorator de telemetria registra `error.code` sem try/catch;
- o front trata `codigo` em vez de fazer parsing de mensagem;
- logs limpos: erro de negócio é `Information`, exceção é `Error`.

Negativas:

- disciplina para não lançar exceções de negócio; código legado ou bibliotecas podem exigir adaptação;
- `Result` sem `Match` obrigatório permite esquecer de checar `IsFailure` (mitigação: acesso a `Value` em falha lança `InvalidOperationException`).

## Alternativas consideradas

- **Exceções de domínio + middleware que traduz**: simples de começar, caro em produção e opaco no fluxo; descartado.
- **Bibliotecas de Result (FluentResults, ErrorOr, OneOf)**: funcionais, mas o projeto precisa de uma implementação pequena e sob controle; escrita própria com ~60 linhas.
- **Códigos numéricos de erro**: menos legíveis; o formato `Modulo.Motivo` é autoexplicativo e agrupável.
