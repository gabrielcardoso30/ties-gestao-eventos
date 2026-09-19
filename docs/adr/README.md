# Registros de decisão de arquitetura (ADR)

Formato MADR simplificado: **Contexto**, **Decisão**, **Consequências**, **Alternativas consideradas**. Status: *Aceita* salvo indicação. Uma decisão só é revertida por um novo ADR que substitua o anterior.

| # | Decisão | Status |
|---|---|---|
| [0001](0001-monolito-modular.md) | Monolito modular em vez de microsserviços | Aceita |
| [0002](0002-schema-por-modulo.md) | Um schema por módulo no mesmo PostgreSQL | Aceita |
| [0003](0003-comunicacao-entre-modulos.md) | Comunicação entre módulos: contratos síncronos em `Shared.Contracts` + eventos via Outbox | Aceita |
| [0004](0004-ef-core-sem-repositorio-tagwith.md) | EF Core sem repositório + `TagWith` obrigatório | Aceita |
| [0005](0005-result-pattern-problem-details.md) | Result pattern + ProblemDetails | Aceita |
| [0006](0006-identity-sobrescrito-jwt.md) | ASP.NET Core Identity sobrescrito com JWT próprio | Aceita |
| [0007](0007-serilog-opentelemetry.md) | Serilog + OpenTelemetry com OTLP e Application Insights opcional | Aceita |
| [0008](0008-guid-v7.md) | Guid v7 como chave primária | Aceita |
| [0009](0009-soft-delete-auditoria-interceptor.md) | Soft delete e auditoria automática por interceptor | Aceita |
| [0010](0010-minimal-apis-vertical-slices.md) | Minimal APIs com um endpoint por caso de uso e vertical slices | Aceita |
| [0011](0011-front-por-modulo-componentes-genericos.md) | Front por módulo/caso de uso com componentes genéricos | Aceita |
| [0012](0012-azure-e-vps-via-containers.md) | Execução em Azure e VPS via containers (mesma imagem, configuração por ambiente) | Aceita |

Como escrever um novo ADR: copie o formato de qualquer arquivo, numere sequencialmente, mantenha o texto curto e concreto (cite classes e arquivos), e atualize esta tabela.
