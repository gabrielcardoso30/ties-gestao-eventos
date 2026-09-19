# Dentro dessa pasta, quero iniciar um projeto modelo para gestão de eventos, afim de apresentar a palestra amanhã em um evento
## A palestra é sobre monolito modular: construindo o futuro de forma simples
## O projeto deve ser de simples entendimento com objetivo de gerir eventos, aí teria módulos de eventos, palestras, palestrantes, locais (local do evento, salas disponiveis, ambientes para recreaçao, etc)
## O projeto deve ser em asp net core 10 com banco em postgres
## O front em react com vite

### Regras de arquitetura, design de código, engenharia de software:

- Dentro do diretório atual (workspace) deve ter dois subdiretórios de projeto: api, front
- O projeto api deve ter o diretório src dentro dele e dentro do diretório src, três subdiretórios: hosts (onde ficará Host.Api e futuros Host.Worker, agora só terá o Host.Api), modules (onde ficarão cada módulo da aplicacao no padrao de nomenclatura Module.NomeModulo) e o diretório Shared (onde ficarão os projetos class library compartilhados, exemplo: Shared.Data que terá entidade base, por exemplo. Shared.Observability, Shared.Contracts, Shared.Http, Shared.WebHost)
- O projeto front também deve ter a estrutura das páginas separadas por módulo e usecase. Além de que todas as telas do front devem ser construidas a partir de componentes genéricos
- Front deve usar template shadcn (https://github.com/shadcn-ui/ui)
- Os endpoints da api devem seguir o prefixo padrão `api/v1/nome-modulo/`
- Os endpoints devem seguir o padrão http rest, não ter nome verbo infinitivo
- Padrão result com problem details
- Os endpoints de um módulo devem ficar agrupados naquele módulo, ou seja, cada módulo deve ter um conjunto apenas de endpoints no swagger, não ficar criando vários conjuntos para um mesmo módulo
-⁠ ⁠Projeto em português brasileiro (tudo o que for domínio, negócios, ubíquo), o que for técnico permanece em inglês 
-⁠ ⁠O nome de propriedades, métodos, etc devem seguir a lógica de ser assim: PessoaNome ao invés de NomePessoa. Outro exemplo: ClienteSituacao ao invés de SituacaoCliente 
-⁠ ⁠Todo acesso feito ao banco é sem repositório, já que o ef core já atua bem. Além disso, toda requisição precisa ter TagWith para auditoria do DBA
-⁠ ⁠Requisição do banco precisa ter controle de transação, se for consulta, precisa trazer somente os campos que irá utilizar
-⁠ ⁠Pense nessa API para ser mega escalável 
-⁠ ⁠Os contratos entre os módulos ficarão em Shared.Contracts (tanto os síncronos como os assíncronos)
-⁠ ⁠Dentro de cada módulo, haverá 3 pastas (use cases, domain e shared) 
-⁠ ⁠Cada use case será uma pasta com os seguintes arquivos dentro: validator, usecase, endpoint, dto (request e response)
-⁠ ⁠Observabilidade nessa aplicação é prioridade máxima 
-⁠ ⁠Toda ação feita nessa aplicação precisa gerar registros de auditoria
-⁠ ⁠Segurança, performance e observabilidade são os 3 pilares dessa solução 
-⁠ ⁠Swagger super bem documentado com markdown 
- Precisa ter o arquivo de contratos v1 yaml para o front se orientar
-⁠ ⁠A documentação precisa ter spec, ADR, business-rules, runbooks e glossary 
-⁠ ⁠A aplicação precisa poder ser rodada tanto no azure como em VPs comum 
-⁠ ⁠Utilizar padrão outbox 
-⁠ ⁠Testes funcionais, integração, unitários
-⁠ ⁠Utilizar Reqnroll 
-⁠ ⁠A autenticação e autorização pode ser pelo identity 
-⁠ ⁠Cada módulo da aplicação possui seu schema no banco
-⁠ ⁠Objetos do banco seguindo o padrão .net que é PascalCase
-⁠ ⁠Soft delete 
-⁠ As entidades principais precisam ter os campos CriadoEm, CriadoPor, AlteradoEm, AlteradoPor, ExcluidoEm, ExcluidoPor, EstaAtivo
-⁠ ⁠A chave primária das tabelas utilizará GuidV7
- A autenticacao e autorização é diretamente no identity do asp net core, lembrando que as classes padrão do identity precisam ser sobrescritas de modo a se adequarem às regras de arquitetura, design de código e engenharia do projeto
- Observabilidade será com serilog e open telemetry (deixar habilitado para application insights)
- Padrão docker compose no projeto, para conseguir executar todo o projeto com apenas um comando

### Regras de negócio:

- Evento precisa de um Local para acontecer ou pode ser remoto
- Local possui salas, se for um local que possua apenas um ambiente, esse ambiente é traduzido como uma sala
- Evento possui uma ou mais palestras
- Cada palestra possui uma ou mais pessoas (palestrantes)
- Cada palestra possui seu conteúdo próprio: slides, arquivos em geral, pdf, links complementares, vídeos, imagens, etc
- Evento possui participantes (pessoas que se inscreveram)
- Um participante pode participar de nenhuma palestra (faltar), bem como pode participar de uma ou mais
- Ao final de cada palestra, o participante pode obter o certificado de participação