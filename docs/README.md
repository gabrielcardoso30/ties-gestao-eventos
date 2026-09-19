# Documentação — Gestão de Eventos (monolito modular)

Toda a documentação do projeto em um só lugar. Convenções de código estão em [`../CLAUDE.md`](../CLAUDE.md); os requisitos originais em [`../descricao-inicial-projeto-gestao-eventos.md`](../descricao-inicial-projeto-gestao-eventos.md).

## Especificações (`spec/`)

| Documento | Conteúdo |
|---|---|
| [arquitetura.md](spec/arquitetura.md) | hosts/modules/shared, regras de dependência, `IModule` e descoberta, pipeline do host, ciclo de um request, contratos síncronos e assíncronos, extração de um módulo |
| [dados.md](spec/dados.md) | schema por módulo, PascalCase, Guid v7, auditoria, soft delete, `TagWith`, transações e retry, paginação, migrações, Outbox |
| [observabilidade.md](spec/observabilidade.md) | Serilog, OpenTelemetry, `ModuleTelemetry`, métricas, exportação OTLP e Application Insights, health checks, o que olhar em incidentes |
| [seguranca.md](spec/seguranca.md) | JWT/Identity, políticas, rate limiting, CORS, cabeçalhos, ProblemDetails, segredos, LGPD, checklist de produção |
| [api-endpoints.md](spec/api-endpoints.md) | contrato de todos os módulos (rotas, DTOs, códigos de erro) — fonte de verdade para implementação e front |

## Decisões de arquitetura (`adr/`)

Índice em [adr/README.md](adr/README.md). Formato MADR: contexto, decisão, consequências, alternativas.

## Regras de negócio (`business-rules/`)

| Documento | Conteúdo |
|---|---|
| [README.md](business-rules/README.md) | mapa do domínio (ER simplificado entre módulos, relacionamentos por Id sem FK cruzada) |
| [locais.md](business-rules/locais.md) | Local e Sala (módulo implementado, referência) |
| [pessoas.md](business-rules/pessoas.md) | Pessoa (palestrantes e participantes) |
| [eventos.md](business-rules/eventos.md) | Evento, Inscrição e máquina de estados |
| [palestras.md](business-rules/palestras.md) | Palestra, Palestrante, Conteúdo, Presença e Certificado |
| [identidade.md](business-rules/identidade.md) | Usuário, Perfil, sessão e políticas |
| [auditoria.md](business-rules/auditoria.md) | Registro de auditoria imutável |

## Runbooks (`runbooks/`)

| Documento | Quando usar |
|---|---|
| [subir-ambiente-local.md](runbooks/subir-ambiente-local.md) | primeiro dia no projeto; compose e execução fora do container |
| [deploy-azure.md](runbooks/deploy-azure.md) | Container Apps / App Service + Azure Database for PostgreSQL + Application Insights |
| [deploy-vps.md](runbooks/deploy-vps.md) | docker compose em VPS com Caddy/Nginx, backup e rotação de logs |
| [migracoes-banco.md](runbooks/migracoes-banco.md) | adicionar migração de um módulo, aplicar em pipeline, reverter |
| [operacao-outbox.md](runbooks/operacao-outbox.md) | mensagens presas, reprocessar, métricas |
| [incidentes.md](runbooks/incidentes.md) | erro 500 com `traceId`, banco indisponível, latência, rate limit |
| [adicionar-um-modulo.md](runbooks/adicionar-um-modulo.md) | passo a passo com checklist para um novo `Module.<Nome>` |

## Outros

- [glossary.md](glossary.md): linguagem ubíqua, termos técnicos mantidos em inglês e a regra `EntidadeAtributo`.
- `contracts/v1/openapi.yaml`: contrato OpenAPI v1 para o front. Enquanto o arquivo versionado não estiver consolidado, a API gera o documento equivalente em `/openapi/v1.yaml` e `/openapi/v1.json`.

## Estado da implementação

O módulo **Locais** está implementado e serve de referência (`api/src/modules/Module.Locais`). Os módulos Identidade, Pessoas, Eventos, Palestras e Auditoria estão sendo implementados a partir de [`spec/api-endpoints.md`](spec/api-endpoints.md); nesta documentação eles são descritos **pela spec**, que é o contrato.
