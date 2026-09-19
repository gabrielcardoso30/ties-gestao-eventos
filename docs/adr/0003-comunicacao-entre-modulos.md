# ADR 0003 — Comunicação entre módulos: contratos síncronos em `Shared.Contracts` + eventos via Outbox

Status: Aceita · Data: 2026-09

## Contexto

Módulos não se referenciam (ADR 0001), mas precisam de dados uns dos outros (Eventos valida `localId`; Palestras valida inscrição) e precisam reagir a fatos (Auditoria registra toda alteração). É necessário um mecanismo que funcione em processo hoje e sobreviva à extração de um módulo amanhã.

## Decisão

Dois canais, ambos declarados em `Shared.Contracts`:

1. **Síncrono, somente leitura**: interfaces `I<X>ModuleApi` (`ILocaisModuleApi`, `IPessoasModuleApi`, `IEventosModuleApi`, `IPalestrasModuleApi`) que devolvem *resumos* (`LocalResumo`, `SalaResumo`, `PessoaResumo`, `EventoResumo`). O módulo dono implementa (`LocaisModuleApi`, `internal`, scoped) com projeções mínimas e `TagWith("Modulo.ModuleApi.Metodo")`; o consumidor injeta a interface.
2. **Assíncrono, para fatos**: records `IntegrationEvent` (`LocalCriado`, `EventoPublicado`, `InscricaoRealizada`, `EntidadeAlterada`...). O agregado chama `RegistrarEvento`; o `AuditoriaSaveChangesInterceptor` grava em `OutboxMessages` na mesma transação; o `OutboxProcessor` entrega via `IIntegrationEventPublisher` aos `IIntegrationEventHandler<T>` registrados com `AddIntegrationEventHandler`.

Regras: Module APIs nunca escrevem; eventos são imutáveis e nomeados no passado; handlers são idempotentes; tipos de evento **só** em `Shared.Contracts` (o `IntegrationEventTypeRegistry` varre apenas esse assembly).

## Consequências

Positivas:

- validações cruzadas continuam simples e síncronas, no mesmo request e trace;
- efeitos colaterais (auditoria, futuras notificações) não atrasam o request nem quebram a transação;
- extração de um módulo troca a implementação da interface por um adaptador HTTP/gRPC e o publicador in-process por um broker, sem tocar em `UseCases/`;
- `Shared.Contracts` não tem dependência de infraestrutura e pode virar pacote.

Negativas:

- ciclos de contrato são possíveis (Eventos ↔ Palestras); aceitos porque não são ciclos de projeto;
- entrega at-least-once: handlers precisam de idempotência (Auditoria usa o `Id` do evento como PK);
- latência de entrega = `Outbox:PollingIntervalMs` (2 s) no pior caso quando não há trabalho contínuo.

## Alternativas consideradas

- **Referência direta entre módulos**: descartada; destrói a fronteira.
- **Mediator in-process para tudo (MediatR e similares)**: esconde quem depende de quem e não dá o "seam" de extração; descartado.
- **Broker desde o início (RabbitMQ/Service Bus)**: infraestrutura extra sem necessidade atual; a interface `IIntegrationEventPublisher` deixa a porta aberta.
- **Eventos de domínio sem Outbox (publicar após o commit)**: perde mensagens em falha entre commit e publicação; descartado.
