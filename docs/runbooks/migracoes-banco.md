# Runbook — Migrações de banco

Cada módulo tem suas migrações em `api/src/modules/Module.<X>/Migrations/`, seu `__EFMigrationsHistory` no próprio schema e sua `DesignTimeDbContextFactory` (`<X>DbContextFactory : DesignTimeDbContextFactoryBase<<X>DbContext>`). Detalhes de desenho em [`../spec/dados.md`](../spec/dados.md).

## Adicionar uma migração a um módulo existente

```bash
cd api
# banco de referência opcional (a fábrica de design time usa localhost:5432 do compose se a variável não existir)
export ConnectionStrings__GestaoEventos="Host=localhost;Port=5432;Database=gestao_eventos;Username=gestao;Password=gestao"

dotnet ef migrations add AdicionaSalaAndar \
  --project src/modules/Module.Locais \
  --startup-project src/modules/Module.Locais \
  --context LocaisDbContext \
  --output-dir Migrations
```

Revise o arquivo gerado: schema correto (`schema: "Locais"`), nomes PascalCase, índices filtrados para colunas únicas (`filter: "\"ExcluidoEm\" IS NULL"`), nenhuma FK para outro schema, colunas de texto com tamanho (`HasMaxLength`), enums como texto. Depois:

```bash
dotnet build
dotnet run --project src/hosts/Host.Api    # aplica na subida (Database:MigrateOnStartup=true em dev)
# ou explicitamente:
dotnet ef database update --project src/modules/Module.Locais --startup-project src/modules/Module.Locais --context LocaisDbContext
```

Regras: uma migração por mudança lógica; nunca edite uma migração já aplicada em outro ambiente; migrações destrutivas (drop de coluna/tabela) em duas etapas (parar de usar → remover na release seguinte).

## Primeira migração de um módulo novo

Igual ao anterior, com nome `Inicial`. O `Up` deve começar com `EnsureSchema` e criar `OutboxMessages` (o `ModuleDbContext` já inclui o `DbSet`, então o EF gera sozinho). Confirme no `Down` a remoção das tabelas do schema.

## Como as migrações são aplicadas

| Ambiente | Mecanismo |
|---|---|
| dev / compose / VPS com 1 instância | `Database:MigrateOnStartup=true`: `DatabaseMigrationHostedService` adquire `pg_advisory_lock(7202410)`, percorre os contextos registrados e aplica as pendentes; libera o lock |
| produção com réplicas / alta criticidade | `Database:MigrateOnStartup=false` e aplicação em pipeline (abaixo) |

O advisory lock impede que duas instâncias migrem ao mesmo tempo; a segunda espera e não encontra nada pendente.

## Aplicar em pipeline

Opção 1, script idempotente por módulo (recomendada; permite revisão por par e execução por DBA):

```bash
for m in Locais Pessoas Eventos Palestras Identidade Auditoria; do
  dotnet ef migrations script --idempotent \
    --project src/modules/Module.$m --startup-project src/modules/Module.$m \
    --context ${m}DbContext -o artifacts/migrations/$m.sql
done
# revisão → aprovação → aplicação
psql "$DATABASE_URL" -v ON_ERROR_STOP=1 -f artifacts/migrations/Locais.sql   # e os demais
```

Opção 2, job efêmero com a própria imagem:

```bash
docker run --rm \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__GestaoEventos="$CS" \
  -e Jwt__SigningKey="$JWT" \
  -e Database__MigrateOnStartup=true -e Outbox__Enabled=false \
  <registry>/gestao-eventos-api:<tag> &
# aguardar log "nenhuma migração pendente"/"aplicando..." de todos os schemas e encerrar o container
```

A opção 2 sobe o host inteiro (inclusive o seed do Identidade); a opção 1 é mais controlada.

## Verificar estado

```sql
SELECT 'Locais' AS schema, * FROM "Locais"."__EFMigrationsHistory"
UNION ALL SELECT 'Eventos', * FROM "Eventos"."__EFMigrationsHistory"
ORDER BY 1, "MigrationId";
```

Ou: `dotnet ef migrations list --project src/modules/Module.<X> --startup-project src/modules/Module.<X> --context <X>DbContext`.

## Rollback

1. Reverter a aplicação para a versão anterior (imagem/revisão) primeiro, se a nova versão depende do schema novo.
2. Reverter a migração do módulo afetado para a anterior:

```bash
dotnet ef database update <MigracaoAnterior> \
  --project src/modules/Module.Locais --startup-project src/modules/Module.Locais --context LocaisDbContext
# ou gerar o script: dotnet ef migrations script <Atual> <Anterior> --idempotent ... e aplicar com psql
```

3. Se a migração já foi aplicada em produção e precisa ser abandonada no código, gere uma **nova** migração que desfaça (não apague a antiga do repositório).
4. `Down` de migrações que removem dados não recupera dados: tenha backup antes (ver [deploy-vps.md](deploy-vps.md) / PITR no Azure).

## Migração de dados (não só schema)

Prefira `migrationBuilder.Sql(...)` na própria migração para transformações pequenas e idempotentes; para volumes grandes, um caso de uso administrativo ou script SQL revisado, executado fora do horário de pico. Sempre com `TagWith`/comentário identificando a operação para o DBA.

## Checklist antes de aplicar em produção

- [ ] Migração revisada por par; scripts gerados anexados ao PR.
- [ ] Backup recente e restaurável.
- [ ] Impacto de lock avaliado (ALTER em tabelas grandes; `CREATE INDEX CONCURRENTLY` exige `migrationBuilder.Sql` fora de transação, com `suppressTransaction: true`).
- [ ] Compatibilidade com a versão anterior da aplicação (deploy em duas etapas quando necessário).
- [ ] Janela e comunicação combinadas com a operação.
