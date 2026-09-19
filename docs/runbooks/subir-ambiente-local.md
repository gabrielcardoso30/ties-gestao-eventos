# Runbook — Subir o ambiente local

## Pré-requisitos

| Ferramenta | Versão | Para quê |
|---|---|---|
| Docker Desktop / Docker Engine + Compose v2 | recente | tudo em container |
| .NET SDK | 10.0.400+ (`api/global.json`, `rollForward: latestFeature`) | rodar/testar a API fora do container |
| Node.js + npm | LTS | front em modo dev |
| `dotnet-ef` | 10.x (`dotnet tool install -g dotnet-ef`) | migrações |

## Opção A: tudo com um comando

```bash
cd gestao-eventos
cp .env.example .env            # opcional; valores padrão servem para dev
docker compose up --build
```

O compose sobe, nesta ordem: `postgres` (healthcheck `pg_isready`), `otel` (Aspire Dashboard), `api` (espera o postgres ficar saudável; aplica migrações; semeia perfis e administrador), `front`.

Verifique:

```bash
curl -s http://localhost:5761/health/ready          # {"status":"Healthy"...}
open http://localhost:5761/swagger                  # API
open http://localhost:5760                          # front
open http://localhost:18888                         # Aspire Dashboard (logs, traces, métricas)
```

Logs esperados na subida da API: `Schema Locais: aplicando 1 migração(ões): 20260919021757_Inicial` (primeira vez), `Outbox iniciado para Auditoria, Eventos, Identidade, Locais, Palestras, Pessoas`.

Login inicial: `POST /api/v1/identidade/sessoes` com `admin@gestaoeventos.local` / `Admin@123456` (ou o `ADMIN_SENHA` do `.env`).

Parar: `docker compose down` (mantém o volume `postgres-data`); `docker compose down -v` apaga o banco.

## Opção B: API e front fora do container

```bash
docker compose up -d postgres otel

cd api
dotnet build                                   # compila GestaoEventos.slnx
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317   # opcional: telemetria no Aspire Dashboard
dotnet run --project src/hosts/Host.Api        # http://localhost:5761/swagger, ambiente Development

cd ../front
npm install
npm run dev                                    # http://localhost:5760 com proxy /api -> http://localhost:5761
```

Em `Development`, `appsettings.Development.json` define a chave JWT de desenvolvimento e a senha do administrador; a connection string padrão do `appsettings.json` aponta para `localhost:5432` (o postgres do compose). Para sobrescrever qualquer valor sem editar arquivos: variáveis de ambiente (`Jwt__SigningKey=...`) ou `dotnet user-secrets set "Jwt:SigningKey" "..." --project src/hosts/Host.Api`.

## Testes

```bash
cd api
dotnet test                                    # todos
dotnet test tests/Tests.Unit                   # rápidos, sem banco
dotnet test tests/Tests.Integration            # sobe PostgreSQL via Testcontainers (Docker precisa estar rodando)
dotnet test tests/Tests.Functional             # cenários Reqnroll sobre o host real
```

## Problemas comuns

| Sintoma | Causa provável | Ação |
|---|---|---|
| API encerra com `Jwt:SigningKey deve ter ao menos 32 caracteres` | rodando em `Production` sem `Jwt__SigningKey` | defina a variável (ou `ASPNETCORE_ENVIRONMENT=Development` fora do container) |
| `ConnectionStrings:GestaoEventos não configurada` | configuração não carregada | confira `appsettings.json`/variáveis; no compose já vem definido |
| `/health/ready` retorna 503 | postgres ainda subindo ou credenciais erradas | `docker compose logs postgres`; aguarde o healthcheck |
| Porta 5432/8080/5173/18888 em uso | outro serviço local | pare o serviço ou altere o mapeamento no compose |
| Swagger sem cadeado nas operações / 401 em tudo | token não informado | `Authorize` com o `accessToken` (sem `Bearer `) |
| Nada aparece no Aspire Dashboard rodando a API fora do container | `OTEL_EXPORTER_OTLP_ENDPOINT` não definido | exporte `http://localhost:4317` antes do `dotnet run` |
| Warning `Consulta sem TagWith detectada` | consulta nova sem `.TagWith` | adicione a tag no caso de uso |
| Testcontainers falha | Docker não está rodando ou sem permissão no socket | inicie o Docker; no Linux, adicione o usuário ao grupo `docker` |

## Resetar o banco local

```bash
docker compose down -v && docker compose up --build
```
