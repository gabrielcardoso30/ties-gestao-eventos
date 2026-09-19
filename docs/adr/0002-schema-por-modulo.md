# ADR 0002 — Um schema por módulo no mesmo PostgreSQL

Status: Aceita · Data: 2026-09

## Contexto

Módulos precisam de isolamento de dados para que a fronteira lógica (ADR 0001) tenha um correspondente físico, sem o custo de operar seis bancos. O PostgreSQL oferece schemas como namespaces com permissões próprias dentro de um banco.

## Decisão

Um banco (`gestao_eventos`), uma connection string (`ConnectionStrings:GestaoEventos`), e **um schema por módulo** com o nome do módulo (`Locais`, `Pessoas`, `Eventos`, `Palestras`, `Identidade`, `Auditoria`):

- `ModuleDbContext` aplica `HasDefaultSchema(Schema)`;
- cada schema tem sua `__EFMigrationsHistory` (`MigrationsHistoryTable("__EFMigrationsHistory", schema)`) e sua tabela `OutboxMessages`;
- **nenhuma chave estrangeira cruza schemas**; referências entre módulos são `Guid` validados por Module APIs;
- migrações vivem no projeto do módulo e são aplicadas por contexto pelo `DatabaseMigrationHostedService`, sob `pg_advisory_lock`.

## Consequências

Positivas:

- `pg_dump --schema=X` move um módulo para outro banco quando for extraído;
- permissões por schema possibilitam um usuário de banco por módulo no futuro;
- histórico de migrações independente: módulos evoluem sem conflitos de ordenação;
- o DBA vê imediatamente a que módulo uma tabela pertence.

Negativas:

- sem FK entre módulos, integridade referencial entre eles é responsabilidade da aplicação (contratos síncronos + eventos de exclusão);
- joins entre módulos são proibidos no código; relatórios cruzados exigem projeções ou composição em memória;
- nomes PascalCase exigem aspas em SQL manual.

## Alternativas consideradas

- **Um banco por módulo desde o início**: seis bancos para operar, transações distribuídas para nada; descartado.
- **Um schema `public` com prefixo de tabela** (`Locais_Salas`): não dá isolamento de permissão nem histórico de migração separado; descartado.
- **Um único `DbContext` para tudo**: reintroduz o acoplamento que o ADR 0001 evita; descartado.
