# ADR 0012 — Execução em Azure e VPS via containers (mesma imagem, configuração por ambiente)

Status: Aceita · Data: 2026-09

## Contexto

A aplicação precisa rodar tanto no Azure quanto em uma VPS comum. Divergências de build por ambiente geram "funciona no meu ambiente" e dobram o trabalho de pipeline. Segredos e endpoints mudam por ambiente; o código não deveria.

## Decisão

- **Uma imagem** por serviço, construída pelo `api/src/hosts/Host.Api/Dockerfile` (multi-stage: `sdk:10.0` → `aspnet:10.0`, porta 8080, usuário não-root `app`, `DOTNET_gcServer=1`, `HEALTHCHECK` em `/health/live`) e pelo `front/Dockerfile` (build estático servido por nginx com proxy `/api`).
- **Toda variação é configuração**: connection string, `Jwt__*`, `Cors__*`, `RateLimiting__*`, `Outbox__*`, `Database__MigrateOnStartup`, `OTEL_EXPORTER_OTLP_ENDPOINT`/`ApplicationInsights__ConnectionString`. O provedor de configuração do .NET lê variáveis de ambiente com `__` como separador de seção.
- **Local e VPS**: `docker-compose.yml` (postgres 17, Aspire Dashboard, api, front) com `.env` para segredos; na VPS, um proxy (Caddy/Nginx) termina TLS na frente do compose.
- **Azure**: mesma imagem em Container Apps (ou App Service for Containers) + Azure Database for PostgreSQL Flexible Server + Application Insights; segredos em Key Vault/secrets do serviço; migrações em pipeline.
- `UseForwardedHeaders` está ligado para funcionar atrás de qualquer proxy/ingress.

## Consequências

Positivas:

- o artefato testado é o artefato implantado, em qualquer destino;
- trocar de nuvem ou voltar para VPS é mudar variáveis e o runbook, não o código;
- desenvolvimento local reproduz produção (mesmo Postgres 17, mesma imagem).

Negativas:

- serviços gerenciados específicos (Key Vault, Managed Identity) são integrados por configuração/ambiente, não pelo código; recursos que exigem SDK (ex.: Managed Identity para o Postgres via token) precisam de decisão futura;
- na VPS, backup, TLS, rotação de logs e atualização de imagens são responsabilidade do time (ver `docs/runbooks/deploy-vps.md`).

## Alternativas consideradas

- **Deploy por `dotnet publish` direto em App Service / systemd**: dois caminhos de build; descartado.
- **Kubernetes desde o início**: infraestrutura desproporcional para o tamanho atual; Container Apps entrega o essencial gerenciado, e o compose basta na VPS.
- **Imagens diferentes por ambiente** (com `appsettings.<Env>.json` embutidos e segredos): viola o princípio de segredos fora da imagem; descartado.
