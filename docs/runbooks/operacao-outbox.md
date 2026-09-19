# Runbook — Operação do Outbox

O Outbox garante que eventos de integração e a trilha de auditoria não se percam. Cada schema tem sua tabela `OutboxMessages`; o `OutboxProcessor` (dentro do `Host.Api`) processa todas. Desenho em [`../spec/dados.md`](../spec/dados.md), seção 10.

## Parâmetros (`appsettings.json` → seção `Outbox`)

| Chave | Padrão | Efeito |
|---|---|---|
| `Outbox:Enabled` | `true` | desliga o processador (mensagens continuam sendo gravadas) |
| `Outbox:PollingIntervalMs` | `2000` | espera entre ciclos quando não há trabalho |
| `Outbox:BatchSize` | `50` | mensagens por schema por ciclo |
| `Outbox:LockSeconds` | `60` | duração do claim; deve ser maior que o tempo de processar um lote |
| `Outbox:MaxAttempts` | `10` | após isso a mensagem é "estacionada" (`LockedUntil` = +365 dias) |

## Sinais de saúde

| Métrica / log | Normal | Atenção |
|---|---|---|
| `outbox.delivery.latency` (ms) | < 5 000 | crescendo continuamente: processador parado ou lento |
| `outbox.messages.failed` | 0 | qualquer valor: ver o erro na tabela |
| log `Outbox iniciado para ...` na subida | presente | ausente: `Outbox:Enabled=false` ou host não subiu |
| log `Falha ao processar mensagem {MessageId} ({Type}) do módulo {Modulo}, tentativa {Tentativa}` | ausente | presente: handler falhando; ver stack no log |
| log `Erro inesperado no ciclo do Outbox` | ausente | banco indisponível ou bug no processador |

## Consultas úteis (substitua o schema)

Pendentes por schema e idade:

```sql
SELECT count(*) AS pendentes, min("OccurredOn") AS mais_antiga, max("Attempts") AS max_tentativas
FROM "Eventos"."OutboxMessages"
WHERE "ProcessedOn" IS NULL;
```

Mensagens com erro:

```sql
SELECT "Id", "Type", "Attempts", "LockedUntil", left("Error", 300) AS erro, "OccurredOn"
FROM "Eventos"."OutboxMessages"
WHERE "ProcessedOn" IS NULL AND "Attempts" > 0
ORDER BY "OccurredOn";
```

Mensagens estacionadas (atingiram `MaxAttempts`):

```sql
SELECT "Id", "Type", "Attempts", "Error"
FROM "Eventos"."OutboxMessages"
WHERE "ProcessedOn" IS NULL AND "LockedUntil" > now() + interval '30 days';
```

Visão consolidada de todos os schemas:

```sql
SELECT 'Locais' s, count(*) FILTER (WHERE "ProcessedOn" IS NULL) pend FROM "Locais"."OutboxMessages"
UNION ALL SELECT 'Pessoas', count(*) FILTER (WHERE "ProcessedOn" IS NULL) FROM "Pessoas"."OutboxMessages"
UNION ALL SELECT 'Eventos', count(*) FILTER (WHERE "ProcessedOn" IS NULL) FROM "Eventos"."OutboxMessages"
UNION ALL SELECT 'Palestras', count(*) FILTER (WHERE "ProcessedOn" IS NULL) FROM "Palestras"."OutboxMessages"
UNION ALL SELECT 'Identidade', count(*) FILTER (WHERE "ProcessedOn" IS NULL) FROM "Identidade"."OutboxMessages";
```

## Cenários

### Mensagens presas com lock (instância morreu no meio do lote)

Nada a fazer: o claim expira em `LockSeconds` e outra instância (ou a mesma, ao voltar) reprocessa. Se quiser acelerar:

```sql
UPDATE "Eventos"."OutboxMessages" SET "LockedUntil" = NULL
WHERE "ProcessedOn" IS NULL AND "LockedUntil" < now() + interval '1 minute' AND "Attempts" = 0;
```

### Handler falhando repetidamente

1. Ler `Error` e o log com a stack (`MessageId`). Causas típicas: bug no handler, violação de constraint no schema destino, tipo de evento não resolvido (`Tipo de evento desconhecido`: o evento não está em `Shared.Contracts` ou a versão da imagem é antiga).
2. Corrigir e implantar.
3. **Reprocessar** as mensagens (o backoff pode estar em minutos ou, se estacionadas, em um ano):

```sql
UPDATE "Eventos"."OutboxMessages"
SET "LockedUntil" = NULL, "Attempts" = 0, "Error" = NULL
WHERE "ProcessedOn" IS NULL AND "Attempts" > 0;
```

Handlers são idempotentes (Auditoria usa o `Id` do evento como PK), então reprocessar é seguro. Em ambiente produtivo, execute com revisão por par.

### Mensagem venenosa que nunca deve ser entregue

Marcar como processada com anotação:

```sql
UPDATE "Eventos"."OutboxMessages"
SET "ProcessedOn" = now(), "Error" = 'descartada manualmente por <nome> em <data>: <motivo>'
WHERE "Id" = '<uuid>';
```

Registre a decisão (a auditoria não cobre a própria tabela do Outbox).

### Processador parado, mas a API responde

Verifique `Outbox:Enabled`; no Azure, garanta mínimo de 1 réplica; olhe `Erro inesperado no ciclo do Outbox` no log. Reiniciar a instância reinicia o `BackgroundService`.

### Latência alta com muitos eventos

Aumente `BatchSize` (até algumas centenas) e reduza `PollingIntervalMs`; se o gargalo é o handler, otimize-o. Como o claim é otimista, mais réplicas da API dividem o trabalho. Alternativa estrutural: `Host.Worker` dedicado com `AddSharedMessaging` e `Outbox:Enabled=false` na API.

## Limpeza de processadas

A tabela cresce indefinidamente; agende (cron na VPS, job no Azure):

```sql
DELETE FROM "Eventos"."OutboxMessages" WHERE "ProcessedOn" < now() - interval '30 days';
-- repetir para cada schema; em tabelas grandes, apagar em lotes (LIMIT via CTE) para não segurar lock
```

Retenção sugerida: 30 dias (a trilha permanente está em `Auditoria.RegistrosAuditoria`, não no Outbox).

## Observar no trace

Cada mensagem processada gera o span `outbox process <Evento>` com tags `messaging.module`, `messaging.message.id`, `messaging.attempts`, pendurado no trace do request original (via `TraceParent`), e um span `consume <Evento>` por handler. Procurar pelo `traceId` do request mostra todo o caminho.
