# Especificação — Trilhas de eventos

## Objetivo

Permitir que a programação de um evento seja organizada em uma ou mais linhas temáticas e garantir que toda palestra seja associada a uma trilha do próprio evento.

## Fluxos

1. Ao criar um evento, o organizador informa a primeira trilha; o contrato aceita várias trilhas na mesma transação. Se um cliente legado omitir a coleção, a API cria `Trilha única`.
2. No detalhe ou edição do evento, o organizador cria, edita, ativa, inativa e exclui trilhas. A última trilha não pode ser excluída.
3. Ao criar ou editar uma palestra, o usuário seleciona primeiro o evento e depois uma trilha ativa daquele evento.
4. Eventos existentes recebem `Trilha única` durante a migração e palestras existentes são vinculadas a ela.

## Critérios de aceite

- Um evento sempre possui uma ou mais trilhas não excluídas.
- Nomes de trilha são únicos dentro do evento, ignorando maiúsculas e minúsculas.
- A cor opcional segue `#RRGGBB`; nome e descrição usam os limites documentados no OpenAPI.
- Uma trilha excluída usa soft delete e mantém os campos completos de auditoria.
- Uma trilha inativa permanece visível para histórico, mas não aparece como opção para uma nova palestra.
- Toda palestra nova ou alterada exige `trilhaId` ativo e pertencente ao seu `eventoId`.
- O módulo Palestras valida a trilha somente por `IEventosModuleApi`, sem referência a `Module.Eventos`.
- Todas as consultas usam `TagWith`; todas as escritas são transacionais e geram auditoria/outbox pelos interceptors.
- Os quatro endpoints de trilhas pertencem à única tag Swagger `Eventos` e retornam Problem Details nos erros.

## Cobertura de regressão

- Unidade: invariantes do agregado, normalização, duplicidade e proteção da última trilha.
- Integração/contrato: criação atômica, CRUD, validação, soft delete, OpenAPI versionado e migração.
- Reqnroll: evento com múltiplas trilhas, inclusão posterior e tentativa de excluir a única trilha.
- Playwright: gerenciamento visual de trilhas e carregamento do dropdown dependente no formulário de palestra.
