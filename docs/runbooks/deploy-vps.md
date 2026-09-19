# Runbook — Deploy em VPS

Objetivo: rodar a aplicação em um servidor Linux comum com Docker Compose, TLS por Caddy (ou Nginx), backup do PostgreSQL e rotação de logs. Mesma imagem do Azure (ADR 0012).

## Requisitos

Ubuntu 22.04/24.04 (ou similar), 2 vCPU / 4 GB para começar, Docker Engine + Compose v2, um domínio apontando para o IP (`api.exemplo.com.br`, `eventos.exemplo.com.br`), portas 80/443 abertas e **8080/5432/18888 fechadas** ao mundo (firewall `ufw`).

## Estrutura no servidor

```
/opt/gestao-eventos/
  docker-compose.yml          # do repositório
  docker-compose.prod.yml     # override: remove ports internas, adiciona proxy, restart policies
  .env                        # segredos (chmod 600)
  Caddyfile
  backups/
```

## `.env` de produção

```
JWT_SIGNING_KEY=<gerar: openssl rand -base64 48>
ADMIN_SENHA=<senha forte; remover após primeira subida>
POSTGRES_PASSWORD=<senha forte>
```

## Override de produção (`docker-compose.prod.yml`)

```yaml
services:
  postgres:
    ports: []                                   # não expor 5432
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    restart: unless-stopped
  otel:
    ports: ["127.0.0.1:18888:18888"]            # só via túnel SSH
    restart: unless-stopped
  api:
    ports: []                                   # só o proxy fala com a API
    restart: unless-stopped
    environment:
      ConnectionStrings__GestaoEventos: "Host=postgres;Port=5432;Database=gestao_eventos;Username=gestao;Password=${POSTGRES_PASSWORD};Maximum Pool Size=100"
      Database__MigrateOnStartup: "true"        # aceitável com 1 instância; o advisory lock protege se houver mais
      Cors__AllowedOrigins__0: https://eventos.exemplo.com.br
      Cors__AllowedOrigins__1: ""
    logging:
      driver: json-file
      options: { max-size: "50m", max-file: "5" }   # rotação de logs do Docker
  front:
    ports: []
    restart: unless-stopped
  caddy:
    image: caddy:2
    restart: unless-stopped
    ports: ["80:80", "443:443"]
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy-data:/data
    depends_on: [api, front]
volumes:
  caddy-data:
```

`Caddyfile` (TLS automático via Let's Encrypt):

```
api.exemplo.com.br {
    reverse_proxy api:8080
    encode gzip
}
eventos.exemplo.com.br {
    reverse_proxy front:80
    encode gzip
}
```

Caddy envia `X-Forwarded-For`/`X-Forwarded-Proto`; a API já usa `UseForwardedHeaders`. Com Nginx, use `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; proxy_set_header X-Forwarded-Proto $scheme;` e certbot.

## Subir e atualizar

```bash
cd /opt/gestao-eventos
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull   # se usar registry
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
docker compose logs -f api | head -50                                   # migrações, Outbox iniciado
curl -s https://api.exemplo.com.br/health/ready
```

Atualização: `git pull` (ou nova tag de imagem) + o mesmo `up -d --build`. A API reinicia em segundos; o Postgres não é tocado. Para rollback, volte ao commit/tag anterior e repita.

## Backup do PostgreSQL

Script diário via cron (`/etc/cron.d/gestao-eventos-backup`, 02:00):

```bash
#!/usr/bin/env bash
set -euo pipefail
cd /opt/gestao-eventos
DATA=$(date +%F)
docker compose exec -T postgres pg_dump -U gestao -d gestao_eventos -Fc > backups/gestao_eventos-$DATA.dump
find backups/ -name '*.dump' -mtime +14 -delete
# opcional: rclone copy backups/ remoto:gestao-eventos-backups
```

Restauração (teste trimestralmente):

```bash
docker compose exec -T postgres pg_restore -U gestao -d gestao_eventos --clean --if-exists < backups/gestao_eventos-<data>.dump
```

Para restaurar apenas um módulo: `pg_restore --schema=Palestras ...` (um schema por módulo facilita).

## Logs e telemetria

- Logs da API saem em JSON compacto no stdout; `docker compose logs api` ou `journalctl` se usar o driver `journald`. Rotação pelo `logging.options` acima.
- O Aspire Dashboard do compose **não persiste** dados; para produção na VPS, substitua o serviço `otel` por um OpenTelemetry Collector + Grafana/Tempo/Loki (ou Grafana Alloy) e mantenha `OTEL_EXPORTER_OTLP_ENDPOINT` apontando para ele. Acesso ao dashboard atual só por túnel: `ssh -L 18888:127.0.0.1:18888 usuario@vps`.

## Manutenção

| Tarefa | Frequência |
|---|---|
| `apt upgrade` + reinício se kernel | mensal |
| Atualizar imagens base (`docker compose build --pull`) | mensal ou em CVE crítica |
| Testar restauração de backup | trimestral |
| Limpar Outbox processado (ver [operacao-outbox.md](operacao-outbox.md)) | semanal (cron) |
| Verificar espaço em disco (`df -h`, volume `postgres-data`) | semanal |
| Revisar `Cors__AllowedOrigins`, chaves e senha do admin | a cada mudança de domínio/time |
