#!/usr/bin/env bash
# Popula a API com dados de demonstração para a palestra (idempotente o suficiente para rodar mais de uma vez:
# nomes/e-mails levam um sufixo aleatório quando SEED_SUFIXO não é informado).
#
# Uso:  ./scripts/demo-seed.sh [API_URL] [EMAIL] [SENHA]
#   API_URL padrão: http://localhost:8080   (docker compose)  | dev local: http://localhost:5080
#   Credenciais padrão: administrador inicial (admin@gestaoeventos.local / Admin@123456)
set -euo pipefail

API="${1:-${API_URL:-http://localhost:8080}}"
EMAIL="${2:-admin@gestaoeventos.local}"
SENHA="${3:-Admin@123456}"
SUFIXO="${SEED_SUFIXO:-$(date +%H%M%S)}"

command -v jq >/dev/null || { echo "jq é necessário (brew install jq)"; exit 1; }

say() { printf '\n\033[1;36m▶ %s\033[0m\n' "$*"; }
post() { curl -fsS -X POST "$API$1" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d "$2"; }
patch() { curl -fsS -X PATCH "$API$1" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d "$2"; }

say "Aguardando a API em $API"
for tentativa in $(seq 1 30); do
  curl -fsS "$API/health/ready" >/dev/null 2>&1 && break
  [ "$tentativa" -eq 30 ] && { echo "API não ficou pronta em 30 segundos"; exit 1; }
  sleep 1
done

say "Autenticando em $API como $EMAIL"
TOKEN=$(curl -fsS -X POST "$API/api/v1/identidade/sessoes" -H "Content-Type: application/json" \
  -d "{\"usuarioEmail\":\"$EMAIL\",\"senha\":\"$SENHA\"}" | jq -r '.accessToken')
[ "$TOKEN" != "null" ] && [ -n "$TOKEN" ] || { echo "Falha no login"; exit 1; }

say "Local com salas"
LOCAL_ID=$(post /api/v1/locais "{\"localNome\":\"Centro de Convenções Vila Velha $SUFIXO\",\"localDescricao\":\"Espaço principal dos eventos Globalsys\",\"enderecoLogradouro\":\"Av. Hugo Musso\",\"enderecoNumero\":\"1000\",\"enderecoBairro\":\"Praia da Costa\",\"enderecoCidade\":\"Vila Velha\",\"enderecoUf\":\"ES\",\"enderecoCep\":\"29101-280\"}" | jq -r '.id')
AUDITORIO_ID=$(post "/api/v1/locais/$LOCAL_ID/salas" '{"salaNome":"Auditório Principal","salaCapacidade":300,"salaTipo":"Auditorio","salaRecursos":"projetor 4K, som, transmissão"}' | jq -r '.id')
LAB_ID=$(post "/api/v1/locais/$LOCAL_ID/salas" '{"salaNome":"Laboratório Cloud","salaCapacidade":40,"salaTipo":"Laboratorio","salaRecursos":"40 estações, Wi-Fi dedicado"}' | jq -r '.id')
post "/api/v1/locais/$LOCAL_ID/salas" '{"salaNome":"Lounge de Networking","salaCapacidade":80,"salaTipo":"AreaRecreacao","salaRecursos":"café, pufes, tomadas"}' >/dev/null
echo "local=$LOCAL_ID auditorio=$AUDITORIO_ID lab=$LAB_ID"

say "Pessoas (palestrantes e participantes)"
pessoa() { post /api/v1/pessoas "{\"pessoaNome\":\"$1\",\"pessoaEmail\":\"$2+$SUFIXO@exemplo.com\",\"pessoaEmpresa\":\"$3\",\"pessoaCargo\":\"$4\",\"pessoaMiniBio\":\"$5\"}" | jq -r '.id'; }
GABRIEL=$(pessoa "Gabriel Cardoso" gabriel.cardoso "Globalsys" "Arquiteto de Software" "Fala sobre monolitos modulares e simplicidade que escala.")
ANA=$(pessoa "Ana Ribeiro" ana.ribeiro "Globalsys" "Engenheira de Dados" "Observabilidade e dados em produção.")
JOAO=$(pessoa "João Martins" joao.martins "Startup XPTO" "Desenvolvedor" "Participante assíduo de meetups.")
MARIA=$(pessoa "Maria Souza" maria.souza "Consultoria ABC" "Tech Lead" "Interessada em arquitetura evolutiva.")
PEDRO=$(pessoa "Pedro Lima" pedro.lima "Universidade" "Estudante" "Primeiro evento de tecnologia.")
echo "palestrantes: $GABRIEL $ANA | participantes: $JOAO $MARIA $PEDRO"

say "Evento presencial (já em andamento, para permitir presença e certificado na demo)"
INICIO=$(date -u -v-3H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '-3 hours' +%Y-%m-%dT%H:%M:%SZ)
FIM=$(date -u -v+9H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '+9 hours' +%Y-%m-%dT%H:%M:%SZ)
EVENTO_ID=$(post /api/v1/eventos "{\"eventoNome\":\"TIES Tech Day $SUFIXO\",\"eventoDescricao\":\"Um dia sobre arquitetura simples que escala.\",\"eventoDataInicio\":\"$INICIO\",\"eventoDataFim\":\"$FIM\",\"eventoFormato\":\"Presencial\",\"localId\":\"$LOCAL_ID\",\"eventoCapacidadeMaxima\":300}" | jq -r '.id')
echo "evento=$EVENTO_ID"

say "Palestras (uma já encerrada, para emitir certificado)"
P1_INICIO=$(date -u -v-2H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '-2 hours' +%Y-%m-%dT%H:%M:%SZ)
P1_FIM=$(date -u -v-1H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '-1 hour' +%Y-%m-%dT%H:%M:%SZ)
P2_INICIO=$(date -u -v+1H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '+1 hour' +%Y-%m-%dT%H:%M:%SZ)
P2_FIM=$(date -u -v+2H +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '+2 hours' +%Y-%m-%dT%H:%M:%SZ)
PALESTRA1=$(post /api/v1/palestras "{\"eventoId\":\"$EVENTO_ID\",\"salaId\":\"$AUDITORIO_ID\",\"palestraTitulo\":\"Monolito modular: construindo o futuro de forma simples\",\"palestraDescricao\":\"Módulos, contratos, outbox e observabilidade sem a complexidade de microsserviços.\",\"palestraInicio\":\"$P1_INICIO\",\"palestraFim\":\"$P1_FIM\",\"palestrantes\":[{\"pessoaId\":\"$GABRIEL\",\"palestrantePapel\":\"Principal\"}]}" | jq -r '.id')
PALESTRA2=$(post /api/v1/palestras "{\"eventoId\":\"$EVENTO_ID\",\"salaId\":\"$LAB_ID\",\"palestraTitulo\":\"Observabilidade na prática com OpenTelemetry\",\"palestraDescricao\":\"Logs, traces e métricas correlacionados.\",\"palestraInicio\":\"$P2_INICIO\",\"palestraFim\":\"$P2_FIM\",\"palestrantes\":[{\"pessoaId\":\"$ANA\",\"palestrantePapel\":\"Principal\"},{\"pessoaId\":\"$GABRIEL\",\"palestrantePapel\":\"Mediador\"}]}" | jq -r '.id')
post "/api/v1/palestras/$PALESTRA1/conteudos" '{"conteudoTitulo":"Slides da palestra","conteudoTipo":"Slides","conteudoUrl":"https://example.com/slides-monolito-modular.pdf"}' >/dev/null
post "/api/v1/palestras/$PALESTRA1/conteudos" '{"conteudoTitulo":"Repositório de exemplo","conteudoTipo":"Link","conteudoUrl":"https://github.com/globalsys/gestao-eventos"}' >/dev/null
post "/api/v1/palestras/$PALESTRA2/conteudos" '{"conteudoTitulo":"Dashboard de exemplo","conteudoTipo":"Imagem","conteudoUrl":"https://example.com/dashboard.png"}' >/dev/null
echo "palestras: $PALESTRA1 $PALESTRA2"

say "Publicar evento e colocar em andamento"
patch "/api/v1/eventos/$EVENTO_ID/situacao" '{"eventoSituacao":"Publicado"}' | jq -c .
patch "/api/v1/eventos/$EVENTO_ID/situacao" '{"eventoSituacao":"EmAndamento"}' | jq -c .

say "Inscrições"
for P in $JOAO $MARIA $PEDRO; do post "/api/v1/eventos/$EVENTO_ID/inscricoes" "{\"pessoaId\":\"$P\"}" | jq -c '{id, inscricaoSituacao}'; done

say "Presenças na palestra encerrada e certificado"
post "/api/v1/palestras/$PALESTRA1/presencas" "{\"pessoaId\":\"$JOAO\"}" | jq -c '{id, presencaRegistradaEm}'
post "/api/v1/palestras/$PALESTRA1/presencas" "{\"pessoaId\":\"$MARIA\"}" | jq -c '{id, presencaRegistradaEm}'
CERT=$(post "/api/v1/palestras/$PALESTRA1/certificados" "{\"pessoaId\":\"$JOAO\"}")
echo "$CERT" | jq -c '{certificadoCodigo, pessoaNome, certificadoCargaHorariaMinutos}'
CODIGO=$(echo "$CERT" | jq -r '.certificadoCodigo')

say "Validação pública do certificado (sem token)"
curl -fsS "$API/api/v1/palestras/certificados/$CODIGO" | jq -c .

say "Auditoria (últimos registros)"
curl -fsS "$API/api/v1/auditoria/registros?tamanhoPagina=5" -H "Authorization: Bearer $TOKEN" | jq -c '.itens[] | {modulo, entidadeNome, operacao, usuarioNome}'

say "Pronto. Evento: $API/swagger | Front: http://localhost:5173/eventos/$EVENTO_ID | Certificado público: http://localhost:5173/certificados/$CODIGO"
