# Regras de negócio — Locais

Módulo implementado (`api/src/modules/Module.Locais`); este documento reflete o código. Rota `api/v1/locais`, schema `Locais`, tabelas `Locais` e `Salas`.

## Agregado

**Local** (raiz) com coleção de **Salas**. A sala só existe dentro de um local; toda operação sobre salas passa pelo agregado (`Local.AdicionarSala`, `AtualizarSala`, `RemoverSala`). Campos do Local: `LocalNome`, `LocalDescricao?`, `EnderecoLogradouro?`, `EnderecoNumero?`, `EnderecoBairro?`, `EnderecoCidade`, `EnderecoUf`, `EnderecoCep?`. Campos da Sala: `LocalId`, `SalaNome`, `SalaCapacidade`, `SalaTipo`, `SalaRecursos?`.

`SalaTipo`: `AmbienteUnico`, `Auditorio`, `SalaAula`, `Laboratorio`, `AreaRecreacao`, `Coworking`, `Outro` (persistido como texto).

## Regras

| Código | Regra | Implementação | Erro |
|---|---|---|---|
| RN-LOC-001 | O nome do local é único entre locais não excluídos. | `CriarLocalUseCase`/`AtualizarLocalUseCase` verificam com `AnyAsync`; índice único filtrado `IX_Locais_LocalNome` | `409 Locais.LocalNomeDuplicado` |
| RN-LOC-002 | Um local com um único ambiente é representado por **uma sala** do tipo `AmbienteUnico`, criada junto com o local quando `capacidadeAmbienteUnico` é informado (nome fixo "Ambiente único"). | `Local.Criar` | — |
| RN-LOC-003 | O nome da sala é único dentro do local (comparação sem distinção de maiúsculas), entre salas não excluídas. | `Local.AdicionarSala`/`AtualizarSala`; índice `IX_Salas_LocalId_SalaNome` | `409 Locais.SalaNomeDuplicado` |
| RN-LOC-004 | Um local nunca fica sem salas: não é possível remover a última sala não excluída. | `Local.RemoverSala` | `422 Locais.LocalPrecisaDeUmaSala` |
| RN-LOC-005 | A capacidade da sala é maior que zero. | validators (`SalaCapacidade > 0`, `capacidadeAmbienteUnico > 0` quando informado) | `400 Validacao` |
| RN-LOC-006 | `EnderecoUf` tem exatamente 2 letras e é armazenada em maiúsculas; `EnderecoCidade` é obrigatória. | validator + `Local.Atualizar` (`ToUpperInvariant`) | `400 Validacao` |
| RN-LOC-007 | Excluir um local exclui logicamente todas as suas salas, na mesma transação, e emite `LocalExcluido`. | `ExcluirLocalUseCase` (`RemoveRange(local.Salas)` + `Remove(local)`), `Local.MarcarExcluido` | `404 Locais.LocalNaoEncontrado` |
| RN-LOC-008 | Excluir uma sala é lógico e não altera as demais. | `ExcluirSalaUseCase` | `404 Locais.SalaNaoEncontrada` |
| RN-LOC-009 | A capacidade total do local é a soma das capacidades das salas não excluídas (calculada em leitura, não persistida). | projeções `l.Salas.Sum(s => s.SalaCapacidade)` (o filtro `SoftDelete` exclui salas removidas) | — |
| RN-LOC-010 | Uma sala pode ser inativada (`EstaAtivo=false`) sem ser excluída; salas inativas não são oferecidas a outros módulos. | `AtualizarSalaRequest.EstaAtivo`; `LocaisModuleApi.ObterSalaResumoAsync` filtra `EstaAtivo` | — |
| RN-LOC-011 | Criar um local emite `LocalCriado`. | `Local.Criar` → `RegistrarEvento` | — |
| RN-LOC-012 | Tamanhos: `LocalNome` 150, `LocalDescricao` 1000, `SalaNome` 100, `SalaRecursos` 500, `EnderecoCep` 10. | validators + configurações EF | `400 Validacao` |

## Invariantes do agregado

1. `Salas` não excluídas ≥ 1 após qualquer remoção (RN-LOC-004). Observação: um local criado sem `capacidadeAmbienteUnico` nasce com zero salas; a invariante protege remoções, não a criação. Cabe ao organizador adicionar salas antes de alocar palestras.
2. Nomes de sala únicos por local (RN-LOC-003).
3. `EnderecoUf` sempre em maiúsculas com 2 caracteres.
4. Mutação de salas somente via métodos do `Local` (coleção exposta como `IReadOnlyCollection`, acesso por campo no EF).

## Contrato oferecido aos outros módulos (`ILocaisModuleApi`)

| Método | Retorno | Regras aplicadas |
|---|---|---|
| `ObterLocalResumoAsync(localId)` | `LocalResumo(Id, LocalNome, LocalCapacidadeTotal, SalasQuantidade)` ou `null` | só locais não excluídos; capacidade = soma das salas não excluídas |
| `ObterSalaResumoAsync(salaId)` | `SalaResumo(Id, LocalId, SalaNome, SalaCapacidade, SalaTipo)` ou `null` | só salas não excluídas **e ativas** |

Eventos publicados: `LocalCriado(LocalId, LocalNome)`, `LocalExcluido(LocalId)`.

## Endpoints (resumo)

| Método | Rota | Política | Caso de uso |
|---|---|---|---|
| POST | `/api/v1/locais` | Gestao | CriarLocal |
| PUT | `/api/v1/locais/{id}` | Gestao | AtualizarLocal |
| DELETE | `/api/v1/locais/{id}` | Gestao | ExcluirLocal |
| GET | `/api/v1/locais/{id}` | autenticado | ObterLocal |
| GET | `/api/v1/locais` | autenticado | ListarLocais (`busca`, `enderecoUf`, `estaAtivo`, paginação) |
| POST | `/api/v1/locais/{id}/salas` | Gestao | AdicionarSala |
| PUT | `/api/v1/locais/{id}/salas/{salaId}` | Gestao | AtualizarSala |
| DELETE | `/api/v1/locais/{id}/salas/{salaId}` | Gestao | ExcluirSala |
| GET | `/api/v1/locais/{id}/salas` | autenticado | ListarSalas (`estaAtivo`) |
