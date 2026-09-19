# ADR 0007 — Serilog + OpenTelemetry com OTLP e Application Insights opcional

Status: Aceita · Data: 2026-09

## Contexto

Observabilidade é prioridade máxima do projeto. A aplicação precisa rodar em Azure (com Application Insights) e em VPS comum (sem serviços gerenciados), com a mesma imagem. É preciso correlacionar logs, traces e métricas por request e por mensagem do Outbox, e ter métricas de negócio por módulo.

## Decisão

Tudo em `Shared.Observability`, ligado pelo host com uma chamada:

- **Logs**: Serilog (`AddSerilog`) com `ReadFrom.Configuration`, enriquecedores (máquina, ambiente, thread, exceções, `Application`, `Version`), `CorrelationId` via `LogContext`, console em JSON compacto em produção e sink OTLP quando há endpoint.
- **Traces e métricas**: OpenTelemetry com instrumentações ASP.NET Core, HttpClient, EF Core, Npgsql, runtime e process, mais fontes próprias `GestaoEventos.*` capturadas por wildcard.
- **Por módulo**: `ModuleTelemetry` (`ActivitySource` + `Meter` `GestaoEventos.<Modulo>`), usado pelo `TelemetryUseCaseDecorator` para span por caso de uso e métricas `usecase.executions|failures|duration`. O Outbox tem `outbox.messages.processed|failed` e `outbox.delivery.latency`; o `QueryTagInterceptor`, `db.queries.untagged`.
- **Exportação por configuração**: `OTEL_EXPORTER_OTLP_ENDPOINT` liga `UseOtlpExporter()` (compose → Aspire Dashboard; produção → Collector/Grafana/Jaeger); `ApplicationInsights:ConnectionString` liga `UseAzureMonitor()`. Podem coexistir.
- **Correlação**: `X-Correlation-Id` aceito/devolvido; `traceparent` gravado em `OutboxMessage.TraceParent` para que o processamento assíncrono apareça no trace do request original.

## Consequências

Positivas:

- nenhum código de telemetria nos casos de uso; um módulo novo é coberto automaticamente;
- mesma imagem em Azure e VPS; a escolha de backend é variável de ambiente;
- em incidente, `traceId` do ProblemDetails leva ao log, ao trace, ao SQL com `TagWith` e ao registro de auditoria.

Negativas:

- dois pipelines de logs (Serilog → OTLP e ILogger → Azure Monitor) podem duplicar logs quando ambos os exportadores estão ativos; ajustar por ambiente;
- cardinalidade: tags `usecase` e `error.code` são finitas por desenho; nunca adicionar ids de entidade como tag de métrica;
- o Aspire Dashboard do compose não persiste dados; é ferramenta de dev/demonstração.

## Alternativas consideradas

- **Só Application Insights SDK**: amarra ao Azure; descartado pelo requisito de VPS.
- **Só ILogger + OpenTelemetry Logs (sem Serilog)**: viável no .NET 10, mas Serilog oferece enriquecedores, sinks e configuração por arquivo já conhecidos pela equipe.
- **Prometheus scrape em vez de OTLP push**: possível via exporter adicional; OTLP cobre os dois destinos exigidos com uma configuração.
