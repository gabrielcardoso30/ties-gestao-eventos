#language: pt-BR
Funcionalidade: Gestão de pessoas
  Como organizador de eventos
  Quero cadastrar palestrantes e participantes
  Para vinculá-los aos eventos e palestras

  Contexto:
    Dado que estou autenticado como "Organizador"

  Cenário: Cadastrar e consultar uma pessoa
    Quando cadastro uma pessoa chamada "Ada Lovelace"
    Então a resposta deve ter status 201
    E consigo consultar a pessoa cadastrada

  Cenário: E-mail de pessoa deve ser único
    Dado que cadastrei uma pessoa chamada "Grace Hopper"
    Quando tento cadastrar outra pessoa com o mesmo e-mail
    Então a resposta deve ter status 409
    E a resposta deve ser um problema com código "Pessoas.EmailJaCadastrado"

  Cenário: Exclusão de pessoa é lógica e remove o registro das consultas
    Dado que cadastrei uma pessoa chamada "Margaret Hamilton"
    Quando excluo a pessoa cadastrada
    Então a resposta deve ter status 204
    E a pessoa cadastrada não deve mais ser encontrada

  Cenário: E-mail inválido é rejeitado
    Quando tento cadastrar uma pessoa com e-mail inválido
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Organizador pode atualizar os dados da pessoa
    Dado que cadastrei uma pessoa chamada "Katherine Johnson"
    Quando altero o nome da pessoa para "Katherine Coleman Johnson"
    Então a resposta deve ter status 200
    E consigo consultar a pessoa com o nome "Katherine Coleman Johnson"

  Cenário: Participante não pode cadastrar pessoas
    Dado que estou autenticado como "Participante"
    Quando cadastro uma pessoa chamada "Pessoa Proibida"
    Então a resposta deve ter status 403
