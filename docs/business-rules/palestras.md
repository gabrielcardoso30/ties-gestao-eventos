# Regras de negócio — Palestras

Módulo em implementação; fonte: [`../spec/api-endpoints.md`](../spec/api-endpoints.md). Rota `api/v1/palestras`, schema `Palestras`, tabelas `Palestras`, `PalestraPalestrantes`, `PalestraConteudos`, `Presencas`, `Certificados`.

## Agregado

**Palestra** (raiz): `EventoId`, `TrilhaId`, `SalaId?`, `PalestraTitulo`, `PalestraDescricao?` (4000), `PalestraInicio`, `PalestraFim`, com coleções:

- **PalestraPalestrante**: `PalestraId`, `PessoaId`, `PalestrantePapel` (`Principal` | `Coautor` | `Mediador`); única `(PalestraId, PessoaId)` entre ativos.
- **PalestraConteudo**: `PalestraId`, `ConteudoTitulo`, `ConteudoTipo` (`Slides` | `Pdf` | `Arquivo` | `Link` | `Video` | `Imagem`), `ConteudoUrl` (2000), `ConteudoDescricao?`.
- **Presenca**: `PalestraId`, `PessoaId`, `PresencaRegistradaEm`; única `(PalestraId, PessoaId)` entre ativos.
- **Certificado**: `PalestraId`, `PessoaId`, `CertificadoCodigo` (único, 12 caracteres A-Z0-9 sem ambíguos), `CertificadoEmitidoEm`, `CertificadoCargaHorariaMinutos`.

`PalestraCargaHorariaMinutos = (PalestraFim - PalestraInicio).TotalMinutes`, calculado, não persistido (é copiado para o certificado no momento da emissão).

## Regras

### Palestra

| Código | Regra | Erro |
|---|---|---|
| RN-PAL-001 | O evento deve existir (`IEventosModuleApi.ObterEventoResumoAsync`). | `422 Palestras.EventoNaoEncontrado` |
| RN-PAL-002 | Criar/alterar palestra só se o evento não está `Encerrado` nem `Cancelado`. | `422 Palestras.EventoNaoAceitaPalestras` |
| RN-PAL-003 | `PalestraFim > PalestraInicio`. | `400 Validacao` |
| RN-PAL-004 | O período da palestra está contido no período do evento. | `422 Palestras.PeriodoForaDoEvento` |
| RN-PAL-005 | `salaId` é opcional; se informado, a sala existe, está ativa e pertence ao local do evento (`SalaResumo.LocalId == EventoResumo.LocalId`). | `422 Palestras.SalaNaoPertenceAoLocal` |
| RN-PAL-006 | Não há sobreposição de horário com outra palestra ativa na mesma sala. | `409 Palestras.SalaOcupada` |
| RN-PAL-007 | Criar exige ao menos um palestrante. | `400 Validacao` |
| RN-PAL-008 | Excluir palestra é lógico e cascateia logicamente para palestrantes e conteúdos; presenças e certificados permanecem. | — |
| RN-PAL-009 | Criar emite `PalestraCriada`. | — |
| RN-PAL-010 | Listagem ordenada por `PalestraInicio`, filtros `eventoId` e `busca`. | — |
| RN-PAL-026 | Toda palestra pertence a uma trilha ativa do mesmo evento, tanto na criação quanto na alteração. | `422 Palestras.TrilhaNaoEncontrada` |

### Palestrantes

| Código | Regra | Erro |
|---|---|---|
| RN-PAL-011 | Cada palestrante é uma Pessoa existente e ativa (`IPessoasModuleApi`). | `422 Palestras.PessoaNaoEncontrada` |
| RN-PAL-012 | Uma pessoa aparece no máximo uma vez por palestra. | `409 Palestras.PalestranteJaVinculado` |
| RN-PAL-013 | Não é possível remover o último palestrante. | `422 Palestras.PalestraPrecisaDePalestrante` |

### Conteúdos

| Código | Regra | Erro |
|---|---|---|
| RN-PAL-014 | Conteúdo tem título, tipo válido e URL (até 2000 caracteres); remoção é lógica. | `400 Validacao` |

### Presença

| Código | Regra | Erro |
|---|---|---|
| RN-PAL-015 | Presença só para pessoa com inscrição **confirmada** no evento da palestra (`IEventosModuleApi.InscricaoConfirmadaExisteAsync`). | `422 Palestras.ParticipanteNaoInscrito` |
| RN-PAL-016 | Presença é única por palestra e pessoa. | `409 Palestras.PresencaJaRegistrada` |
| RN-PAL-017 | Registrar presença exige `Gestao` (é o organizador quem registra) e emite `PresencaRegistrada`. | `403` |
| RN-PAL-018 | Um participante pode não ter presença em nenhuma palestra (faltar) ou ter em várias. | — |

### Certificado

| Código | Regra | Erro |
|---|---|---|
| RN-PAL-019 | Certificado exige presença registrada. | `422 Palestras.PresencaNaoRegistrada` |
| RN-PAL-020 | Certificado só após o término da palestra (`PalestraFim <= agora`). | `422 Palestras.PalestraNaoEncerrada` |
| RN-PAL-021 | Emissão é idempotente: se já existe para a pessoa e palestra, devolve o existente com `200` (em vez de `201`). | — |
| RN-PAL-022 | `CertificadoCodigo` tem 12 caracteres de `A-Z0-9` sem caracteres ambíguos (ex.: sem `0/O`, `1/I/L`), único globalmente. | — |
| RN-PAL-023 | `CertificadoCargaHorariaMinutos` é copiado da palestra na emissão (não muda se a palestra for alterada depois). | — |
| RN-PAL-024 | Validação pública por código, sem autenticação, expondo apenas título, evento, nome do participante, datas e carga horária. | `404 Palestras.CertificadoNaoEncontrado` |
| RN-PAL-025 | Emitir exige apenas autenticação (o próprio participante pode emitir) e emite `CertificadoEmitido`. | — |

## Invariantes do agregado

1. Palestrantes ativos ≥ 1 (RN-PAL-007, RN-PAL-013).
2. Período dentro do evento e sem conflito de sala (RN-PAL-004, RN-PAL-006), revalidados em toda alteração de horário ou sala.
3. Presença e certificado únicos por pessoa; certificado implica presença.
4. Presenças e certificados sobrevivem à exclusão lógica da palestra (histórico do participante).

## Contrato oferecido (`IPalestrasModuleApi`)

| Método | Retorno |
|---|---|
| `ContarPalestrasDoEventoAsync(eventoId)` | quantidade de palestras ativas e não excluídas do evento |

Eventos publicados: `PalestraCriada`, `PresencaRegistrada`, `CertificadoEmitido`.

## Endpoints

| Método | Rota | Política |
|---|---|---|
| POST | `/api/v1/palestras` | Gestao |
| PUT | `/api/v1/palestras/{id}` | Gestao |
| DELETE | `/api/v1/palestras/{id}` | Gestao |
| GET | `/api/v1/palestras/{id}` | autenticado |
| GET | `/api/v1/palestras` | autenticado |
| POST | `/api/v1/palestras/{id}/palestrantes` | Gestao |
| DELETE | `/api/v1/palestras/{id}/palestrantes/{pessoaId}` | Gestao |
| POST | `/api/v1/palestras/{id}/conteudos` | Gestao |
| DELETE | `/api/v1/palestras/{id}/conteudos/{conteudoId}` | Gestao |
| POST | `/api/v1/palestras/{id}/presencas` | Gestao |
| GET | `/api/v1/palestras/{id}/presencas` | autenticado |
| POST | `/api/v1/palestras/{id}/certificados` | autenticado |
| GET | `/api/v1/palestras/certificados/{codigo}` | anônimo |
