# Regras de negócio — Pessoas

Módulo em implementação; fonte: [`../spec/api-endpoints.md`](../spec/api-endpoints.md). Rota `api/v1/pessoas`, schema `Pessoas`, tabela `Pessoas`.

## Agregado

**Pessoa**: `PessoaNome`, `PessoaEmail` (armazenado em minúsculas), `PessoaTelefone?`, `PessoaDocumento?` (CPF, somente dígitos), `PessoaEmpresa?`, `PessoaCargo?`, `PessoaMiniBio?` (2000), `PessoaFotoUrl?`. Uma Pessoa não sabe se é palestrante ou participante: o papel nasce dos vínculos nos módulos Palestras e Eventos.

## Regras

| Código | Regra | Erro |
|---|---|---|
| RN-PES-001 | `PessoaEmail` é obrigatório, válido, normalizado para minúsculas e único entre pessoas não excluídas. | `409 Pessoas.EmailJaCadastrado` |
| RN-PES-002 | `PessoaDocumento`, quando informado, é armazenado só com dígitos e é único entre pessoas não excluídas. | `409 Pessoas.DocumentoJaCadastrado` |
| RN-PES-003 | Palestrantes e participantes são Pessoas; não existem cadastros separados. | — |
| RN-PES-004 | Atualizar permite alterar todos os campos e `EstaAtivo`. | `404 Pessoas.PessoaNaoEncontrada` |
| RN-PES-005 | Excluir é lógico e emite `PessoaExcluida`; vínculos existentes em Eventos/Palestras permanecem (histórico), mas a pessoa deixa de ser encontrada por `IPessoasModuleApi`. | — |
| RN-PES-006 | Criar emite `PessoaCriada(PessoaId, PessoaNome, PessoaEmail)`. | — |
| RN-PES-007 | `IPessoasModuleApi` devolve apenas pessoas **ativas e não excluídas**, e apenas `Id`, `PessoaNome`, `PessoaEmail` (minimização de dados entre módulos). | — |
| RN-PES-008 | Busca na listagem cobre nome, e-mail e empresa; filtro por `estaAtivo`. | — |
| RN-PES-009 | Dados pessoais só são coletados quando necessários; campos além de nome e e-mail são opcionais (LGPD). | — |

## Invariantes

1. E-mail único e em minúsculas.
2. Documento único quando presente; só dígitos.
3. Nenhum outro módulo copia dados pessoais além de `PessoaId`; nomes e e-mails são resolvidos em leitura.

## Contrato oferecido (`IPessoasModuleApi`)

| Método | Retorno |
|---|---|
| `ObterPessoaResumoAsync(pessoaId)` | `PessoaResumo(Id, PessoaNome, PessoaEmail)` ou `null` |
| `ObterPessoasResumoAsync(pessoaIds)` | lista de `PessoaResumo` (lote, para listagens) |

## Endpoints

| Método | Rota | Política |
|---|---|---|
| POST | `/api/v1/pessoas` | Gestao |
| PUT | `/api/v1/pessoas/{id}` | Gestao |
| DELETE | `/api/v1/pessoas/{id}` | Gestao |
| GET | `/api/v1/pessoas/{id}` | autenticado |
| GET | `/api/v1/pessoas` | autenticado |
