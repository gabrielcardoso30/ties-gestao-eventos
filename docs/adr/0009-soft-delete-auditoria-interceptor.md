# ADR 0009 — Soft delete e auditoria automática por interceptor

Status: Aceita · Data: 2026-09

## Contexto

Requisitos: toda entidade principal tem `CriadoEm/Por`, `AlteradoEm/Por`, `ExcluidoEm/Por`, `EstaAtivo`; exclusão é lógica; **toda ação gera registro de auditoria**. Fazer isso manualmente em cada caso de uso seria repetitivo e falharia silenciosamente quando alguém esquecesse.

## Decisão

Um único `SaveChangesInterceptor` compartilhado, `AuditoriaSaveChangesInterceptor` (`Shared.Data.Interceptors`), registrado em todo `DbContext` por `AddModuleDbContext`, que a cada `SaveChanges`:

1. preenche os campos de auditoria conforme o estado da entidade (`Added`/`Modified`), usando `ICurrentUser` e `TimeProvider`;
2. converte `Deleted` em `Modified` com `ExcluidoEm`, `ExcluidoPor`, `EstaAtivo=false` (**soft delete**);
3. gera um evento `EntidadeAlterada` (módulo, entidade, id, operação, dados anteriores/novos em JSON, usuário, `traceId`) por entidade incluída/alterada/excluída, quando `AuditChangesEnabled` (o contexto de Auditoria desliga);
4. coleta os eventos de integração dos agregados (`IEmissorDeEventos`) e grava tudo em `OutboxMessages` na mesma transação.

Na leitura, `ModuleDbContext` aplica o filtro global nomeado `SoftDelete` (`ExcluidoEm == null`) a toda entidade `IEntidadeAuditavel`; ignorar com `IgnoreQueryFilters([ModuleDbContext.SoftDeleteFilterName])`. Índices únicos são filtrados por `"ExcluidoEm" IS NULL`.

O módulo **Auditoria** consome `EntidadeAlterada` e persiste `RegistroAuditoria` imutável no schema `Auditoria`, idempotente pelo `Id` do evento.

## Consequências

Positivas:

- impossível esquecer auditoria ou soft delete: acontece no `SaveChanges`;
- trilha correlacionada por `traceId` com logs e traces;
- o negócio e a trilha são atômicos (Outbox na mesma transação); a persistência da trilha é assíncrona e não atrasa o request;
- restauração é um `UPDATE` que limpa `ExcluidoEm`.

Negativas:

- `DadosNovos` em inclusão contém todas as colunas, inclusive dados pessoais de Pessoas/Identidade: acesso restrito a administradores e retenção a definir (LGPD);
- soft delete exige atenção em índices únicos, comparações em memória e volume de dados "mortos";
- o interceptor é singleton (por causa do pooling): tudo que ele injeta precisa ser singleton (`CurrentUser` via `IHttpContextAccessor`);
- auditoria é eventual (segundos): consultas de auditoria imediatamente após a ação podem não ver o registro ainda.

## Alternativas consideradas

- **Triggers no PostgreSQL**: não conhecem o usuário da aplicação nem o `traceId`; descartado como mecanismo principal (pode complementar).
- **Temporal tables / extensões de histórico**: PostgreSQL não tem nativo; extensões adicionam dependência operacional.
- **Auditoria síncrona na mesma tabela/transação em outro schema**: acoplaria todos os módulos ao schema de Auditoria e violaria o ADR 0002.
- **Hard delete + tabela de lixeira**: mais complexo e perde a simplicidade do filtro global.
