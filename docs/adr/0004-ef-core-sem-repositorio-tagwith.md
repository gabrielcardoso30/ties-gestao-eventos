# ADR 0004 — EF Core sem repositório + `TagWith` obrigatório

Status: Aceita · Data: 2026-09

## Contexto

O EF Core já é uma unidade de trabalho com repositórios genéricos (`DbSet<T>`), change tracking, projeções e transações. Uma camada de repositório própria costuma duplicar isso, esconder o SQL e dificultar projeções eficientes. Ao mesmo tempo, o DBA precisa identificar de onde vem cada consulta em produção.

## Decisão

- Casos de uso recebem o `<Modulo>DbContext` diretamente e usam LINQ; **não há interface de repositório**.
- Leituras: `AsNoTracking()` + `Select` para o `Response` (somente colunas necessárias).
- Escritas: agregado carregado com tracking, regra no domínio, `db.ExecuteInTransactionAsync(...)` + `SaveChangesAsync`.
- **Toda consulta** leva `.TagWith("Modulo.CasoDeUso[.Etapa]")`. O `QueryTagInterceptor` mede e loga consultas sem tag (`db.queries.untagged`), tornando a regra auditável.
- Testabilidade vem de testes de integração com Testcontainers (banco real) e de manter regras no agregado (testáveis sem banco).

## Consequências

Positivas:

- menos código e nenhuma abstração que "vaza" (`IQueryable` em repositório é o pior dos mundos);
- SQL gerado é previsível e rastreável por caso de uso (`pg_stat_activity`, spans do OpenTelemetry);
- projeções diretas evitam N+1 e tráfego desnecessário.

Negativas:

- unit tests de casos de uso não mockam o banco; a estratégia é integração com PostgreSQL real (mais lento, mais fiel);
- a disciplina de `TagWith`/`AsNoTracking`/`Select` depende de revisão; o interceptor cobre só a tag.

## Alternativas consideradas

- **Repositório + Unit of Work próprios**: duplicação do EF; descartado.
- **Dapper para leitura, EF para escrita**: mais rápido em cenários extremos, mas dois modelos para manter; pode ser adotado pontualmente no futuro dentro de um caso de uso, sem mudar a arquitetura.
- **Bloquear consultas sem tag (lançar exceção no interceptor)**: rígido demais para consultas internas do EF; optou-se por métrica + warning.
