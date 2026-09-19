# Runbook — Incidentes

Guia de primeira resposta. Em todos os casos, anote o `traceId`/`X-Correlation-Id` reportado: ele liga a resposta HTTP, os logs, o trace e os registros de auditoria. Referências: [`../spec/observabilidade.md`](../spec/observabilidade.md), [`../spec/seguranca.md`](../spec/seguranca.md).

## 1. Cliente recebeu 500 com `traceId`

Resposta típica: `{ "status": 500, "title": "Erro interno", "codigo": "ErroInterno", "traceId": "4bf92f3577b34da6a3ce929d0e0e4736", "detail": "Ocorreu um erro inesperado. Informe o traceId ao suporte." }`

1. **Logs**: filtrar por `TraceId = <id>`. O `GlobalExceptionHandler` registra `Exceção não tratada. TraceId=...` com a stack completa. No Aspire/Loki/Log Analytics a propriedade se chama `TraceId`; no Application Insights, `operation_Id`.
2. **Trace**: abrir o trace pelo id. Ver o span do caso de uso (`Modulo.CasoDeUso`) com status `Error`, e os spans EF/Npgsql abaixo dele (o SQL traz o `-- Modulo.CasoDeUso`).
3. Classificar: `DbUpdateException` (constraint; ver [dados.md](../spec/dados.md) sobre índices únicos), `NpgsqlException` transitória (ver item 2), `InvalidOperationException` no processador (`Tipo de evento desconhecido`), `NullReferenceException` (bug).
4. Se for regra de negócio que virou exceção (deveria ser `Result`), abrir correção; enquanto isso, orientar o cliente.
5. `499` indica cliente que cancelou; não é erro do servidor.

## 2. Banco indisponível

Sintomas: `/health/ready` 503 (`postgres: Unhealthy`), logs com `NpgsqlException`/`Connection refused`, `Erro inesperado no ciclo do Outbox`, latência subindo antes dos erros.

1. Confirmar: `docker compose ps postgres` / painel do Flexible Server; `pg_isready`.
2. Falhas transitórias (failover, reinício) são absorvidas por `EnableRetryOnFailure(3)` e pelo bloco de `ExecuteInTransactionAsync`; picos curtos geram poucos 500.
3. Se o banco voltou e a API não: reiniciar a instância (o pool reconecta sozinho, mas o readiness pode ter derrubado o container).
4. Se o pool esgotou (`The connection pool has been exhausted`): verificar `Maximum Pool Size=100` por instância × número de instâncias vs `max_connections`; procurar consultas longas em `pg_stat_activity` (a tag mostra o caso de uso); considerar PgBouncer.
5. Durante a indisponibilidade o `OutboxProcessor` apenas loga e tenta no próximo ciclo; nada se perde.

## 3. Latência alta

1. Métrica `http.server.request.duration` por rota e `usecase.duration` por `usecase` para achar o caso de uso.
2. No trace do caso de uso lento, olhar os spans SQL: consulta única lenta (índice ausente? `EXPLAIN ANALYZE` com o SQL do span) ou muitas consultas (N+1; corrigir projeção).
3. No PostgreSQL: `pg_stat_activity` filtrando `query LIKE '-- Eventos.%'`; `pg_stat_statements` ordenado por `mean_exec_time`.
4. Verificar se o Outbox está competindo (lotes grandes de auditoria em horário de pico): `outbox.messages.processed` por minuto; ajustar `BatchSize`/intervalo ou mover para um `Host.Worker`.
5. Runtime: métricas de GC e thread pool (`AddRuntimeInstrumentation`); `DOTNET_gcServer=1` já está na imagem.
6. Lock no banco: `SELECT * FROM pg_locks WHERE NOT granted;` e `wait_event_type` em `pg_stat_activity`.

## 4. Muitos 429 (rate limit)

Resposta: `{ "status": 429, "title": "Muitas requisições", "type": ".../erros/LimiteRequisicoes" }`.

1. Identificar a partição: usuário autenticado (`Identity.Name`) ou IP. Se todos os anônimos aparecem com o **mesmo IP**, o proxy não está enviando `X-Forwarded-For` (ou a API não está atrás dele como esperado) e o limite está sendo compartilhado por todo mundo. Corrigir o proxy.
2. Se é um cliente legítimo (integração, front com polling agressivo): ajustar `RateLimiting__PermitLimit`/`WindowSeconds` (reinício necessário) ou corrigir o cliente.
3. Se é abuso: bloquear no proxy/firewall; o rate limiter da aplicação é a última camada, não a primeira.
4. Login: o limite específico do módulo Identidade protege contra força bruta; picos de 429 em `/sessoes` com lockouts (`Identidade.UsuarioBloqueado`) indicam ataque; revisar logs por IP.

## 5. Erros de negócio em massa (`usecase.failures`)

Não é incidente de infraestrutura, mas pode indicar bug ou dado inconsistente: `Eventos.LocalNaoEncontrado` em alta pode significar um local excluído ainda referenciado por eventos (RN-GER-006); `Palestras.SalaOcupada` pode ser front mostrando salas erradas. Agrupar por `error.code` e `module`, checar release recente.

## 6. Auditoria não aparece / eventos não consumidos

Ver [operacao-outbox.md](operacao-outbox.md). Primeiro `SELECT` de pendentes por schema; depois log do processador.

## 7. Falha na subida

| Log | Causa | Ação |
|---|---|---|
| `Jwt:SigningKey deve ter ao menos 32 caracteres` | segredo ausente | configurar `Jwt__SigningKey` |
| `ConnectionStrings:GestaoEventos não configurada` | configuração ausente | configurar |
| `Schema X: aplicando N migração(ões)` seguido de exceção | migração falhou | ver [migracoes-banco.md](migracoes-banco.md); a próxima instância tentará de novo (advisory lock) |
| `X não implementa Map estático` | endpoint sem `Map` público estático | corrigir código |
| trava em `pg_advisory_lock` | outra instância migrando ou sessão órfã segurando o lock | `SELECT pid FROM pg_locks WHERE locktype='advisory' AND objid=7202410;` → `pg_terminate_backend(pid)` se órfã |

## 8. Comunicação

Ao reportar internamente: `traceId`, horário (UTC e local), endpoint, `codigo`, usuário (Id, não e-mail, quando possível), e o que já foi verificado. Após resolver: registrar causa raiz e, se for o caso, uma regra nova em `business-rules/` ou um ADR.
