# ADR 0011 — Front por módulo/caso de uso com componentes genéricos

Status: Aceita · Data: 2026-09

## Contexto

O front (React + Vite + shadcn) precisa espelhar a organização do backend para que a fronteira de módulo seja visível de ponta a ponta, e as telas devem ser montadas apenas com componentes genéricos, para consistência visual e velocidade. Os tipos devem seguir o contrato v1 da API.

## Decisão

- Estrutura `front/src/modules/<modulo>/<use-case>/` (ex.: `modules/locais/criar-local/`), uma página por caso de uso, alinhada às pastas `UseCases/` do backend.
- Componentes shadcn em `front/src/shared/components/ui`; **componentes genéricos** do projeto em `front/src/shared/components/generic` (listagem paginada, formulário, campos, diálogo de confirmação, badges de situação, exibição de ProblemDetails etc.). Páginas não usam elementos brutos nem shadcn diretamente; compõem os genéricos.
- Tipos TypeScript derivados de `docs/contracts/v1/openapi.yaml` (equivalente ao `/openapi/v1.yaml` gerado pela API), em camelCase, seguindo a regra `entidadeAtributo`.
- Comunicação com a API via `/api` (proxy do Vite em dev para `http://localhost:5761`; nginx no container para o serviço `api`), token JWT no header, `X-Correlation-Id` gerado por ação do usuário, e tratamento centralizado de ProblemDetails por `codigo`.

## Consequências

Positivas:

- quem lê `api/src/modules/Module.Eventos/UseCases/PublicarEvento` acha `front/src/modules/eventos/publicar-evento` sem procurar;
- consistência visual e acessibilidade centralizadas nos genéricos;
- extrair um módulo do backend não muda a organização do front (só a URL base, se houver gateway).

Negativas:

- componentes genéricos precisam de manutenção e podem virar gargalo se forem rígidos demais; permitir composição (slots/children) em vez de configurações infinitas;
- páginas simples podem parecer "over-engineered" no início.

## Alternativas consideradas

- **Organização por tipo (pages/, components/, hooks/)**: perde a rastreabilidade por módulo; descartado.
- **Uso livre de shadcn nas páginas**: rápido no início, inconsistente no médio prazo; descartado pelo requisito.
- **Geração de cliente a partir do OpenAPI (orval/openapi-typescript)**: recomendado como evolução; o ADR fixa a fonte (o contrato v1), não a ferramenta.
