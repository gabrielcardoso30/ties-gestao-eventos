#language: pt-BR
Funcionalidade: Gestão de locais e salas
  Como organizador de eventos
  Quero cadastrar locais e seus ambientes (salas)
  Para alocar palestras em espaços físicos

  Contexto:
    Dado que estou autenticado como "Organizador"

  Cenário: Local com um único ambiente nasce com uma sala
    Quando crio um local chamado "Auditório Globalsys" com ambiente único para 200 pessoas
    Então a resposta deve ter status 201
    E o local deve possuir 1 sala(s)
    E a primeira sala deve se chamar "Ambiente único" e ser do tipo "AmbienteUnico"
    E a capacidade total do local deve ser 200

  Cenário: Capacidade total é a soma das salas
    Dado que existe um local chamado "Centro de Convenções" com ambiente único para 300 pessoas
    Quando adiciono a sala "Laboratório 1" com capacidade 40 do tipo "Laboratorio"
    E adiciono a sala "Área de descompressão" com capacidade 60 do tipo "AreaRecreacao"
    Então o local deve possuir 3 sala(s)
    E a capacidade total do local deve ser 400

  Cenário: Um local nunca fica sem salas
    Dado que existe um local chamado "Espaço Único" com ambiente único para 50 pessoas
    Quando tento excluir a sala "Ambiente único"
    Então a resposta deve ter status 422
    E a resposta deve ser um problema com código "Locais.LocalPrecisaDeUmaSala"

  Cenário: Nome de sala é único dentro do local
    Dado que existe um local chamado "Hub de Inovação" com ambiente único para 80 pessoas
    Quando adiciono a sala "Sala Ciano" com capacidade 20 do tipo "SalaAula"
    E adiciono a sala "Sala Ciano" com capacidade 25 do tipo "SalaAula"
    Então a resposta deve ter status 409
    E a resposta deve ser um problema com código "Locais.SalaNomeDuplicado"

  Cenário: Participante não pode cadastrar locais
    Dado que estou autenticado como "Participante"
    Quando crio um local chamado "Local Proibido" com ambiente único para 10 pessoas
    Então a resposta deve ter status 403

  Cenário: Nome de local deve ser único
    Dado que existe um local chamado "Local Repetido" com ambiente único para 40 pessoas
    Quando tento criar outro local chamado "Local Repetido" com ambiente único para 60 pessoas
    Então a resposta deve ter status 409
    E a resposta deve ser um problema com código "Locais.LocalNomeDuplicado"

  Cenário: UF deve possuir exatamente duas letras
    Quando tento criar um local com UF "ESP"
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Exclusão de local é lógica
    Dado que existe um local chamado "Local Temporário" com ambiente único para 20 pessoas
    Quando excluo o local
    Então a resposta deve ter status 204
    E o local não deve mais ser encontrado
