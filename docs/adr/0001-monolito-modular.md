# ADR 0001 — Monolito modular em vez de microsserviços

Status: Aceita · Data: 2026-09

## Contexto

O sistema de gestão de eventos tem seis áreas de negócio (Identidade, Pessoas, Locais, Eventos, Palestras, Auditoria) com regras que se cruzam com frequência: inscrever exige pessoa e evento válidos; publicar um evento exige palestra; presença exige inscrição. A equipe é pequena, o volume inicial é modesto e o requisito explícito é que a solução seja "mega escalável" sem se tornar cara de operar. Microsserviços resolveriam escala independente ao custo de rede, consistência eventual, múltiplos pipelines e observabilidade distribuída desde o primeiro dia.

## Decisão

Construir **um único deployable** (`Host.Api`) composto por módulos com fronteiras rígidas:

- cada `Module.<Nome>` tem `Domain/`, `UseCases/`, `Shared/`, seu `DbContext`, seu schema, seu grupo de endpoints e sua telemetria;
- um módulo **só referencia `Shared.*`**; nunca outro módulo (`Module.Locais.csproj` é a prova);
- comunicação entre módulos apenas por `Shared.Contracts` (ADR 0003);
- tudo o que é transversal fica em `Shared.*` e é composto pelo host (`AddModularWebHost`/`UseModularWebHost`).

A escalabilidade vem de: réplicas horizontais da mesma imagem (o Outbox suporta múltiplas instâncias por claim otimista), DbContext pooling, consultas projetadas e a possibilidade de extrair um módulo para um host próprio quando um gargalo real aparecer.

## Consequências

Positivas:

- uma transação de banco por caso de uso; nada de sagas para regras simples;
- um trace por request cobre todos os módulos envolvidos;
- um pipeline, uma imagem, um banco, um dashboard;
- refatorações entre módulos são detectadas pelo compilador;
- extração futura é um problema de infraestrutura, não de reescrita (ver `docs/spec/arquitetura.md`, seção 6).

Negativas / riscos:

- disciplina é necessária: a fronteira é de projeto, não de processo; um `ProjectReference` indevido quebra o modelo (mitigação: revisão e, futuramente, teste de arquitetura);
- todos os módulos compartilham o mesmo ciclo de deploy e o mesmo runtime; um bug de memória em um módulo afeta os demais;
- escala é do processo inteiro até que um módulo seja extraído.

## Alternativas consideradas

- **Microsserviços desde o início**: descartado pelo custo operacional e pela consistência eventual desnecessária para o domínio atual.
- **Monolito em camadas (Controllers/Services/Repositories)**: descartado porque não impõe fronteiras por negócio; o acoplamento cresce silenciosamente.
- **Modular com projetos únicos por camada** (um `Domain.dll` para tudo): descartado; a fronteira precisa ser por módulo para permitir extração.
