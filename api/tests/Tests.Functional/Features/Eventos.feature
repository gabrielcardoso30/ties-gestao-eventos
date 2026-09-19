#language: pt-BR
Funcionalidade: Gestão do ciclo de vida de eventos
  Como organizador
  Quero criar e administrar eventos
  Para disponibilizar uma programação consistente

  Contexto:
    Dado que estou autenticado como "Organizador"

  Cenário: Evento remoto nasce em rascunho e não exige local
    Quando crio um evento remoto válido
    Então a resposta deve ter status 201
    E o evento deve estar na situação "Rascunho"

  Cenário: Evento presencial exige local
    Quando tento criar um evento presencial sem local
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Data final precisa ser posterior ao início
    Quando tento criar um evento com período invertido
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Evento em rascunho pode ser excluído
    Dado que existe um evento remoto em rascunho
    Quando excluo o evento
    Então a resposta deve ter status 204
    E o evento não deve mais ser encontrado

  Cenário: Evento remoto exige link de acesso
    Quando tento criar um evento remoto sem link
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Capacidade deve ser maior que zero
    Quando tento criar um evento remoto com capacidade zero
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Evento sem palestras não pode ser publicado
    Dado que existe um evento remoto em rascunho
    Quando tento publicar o evento
    Então a resposta deve ter status 422
    E a resposta deve ser um problema com código "Eventos.EventoSemPalestras"

  Cenário: Participante não pode criar eventos
    Dado que estou autenticado como "Participante"
    Quando crio um evento remoto válido
    Então a resposta deve ter status 403
