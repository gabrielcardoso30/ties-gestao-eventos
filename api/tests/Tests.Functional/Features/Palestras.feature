#language: pt-BR
Funcionalidade: Gestão de palestras
  Como organizador
  Quero montar a programação e seus materiais
  Para entregar conteúdo aos participantes

  Contexto:
    Dado que estou autenticado como "Organizador"
    E que existem evento, sala e palestrante para a palestra

  Cenário: Criar palestra vinculada ao evento e palestrante
    Quando crio uma palestra válida
    Então a resposta deve ter status 201
    E a palestra deve possuir 1 palestrante(s)

  Cenário: Palestra exige ao menos um palestrante
    Quando tento criar uma palestra sem palestrantes
    Então a resposta deve ter status 400
    E a resposta deve ser um problema com código "Validacao"

  Cenário: Adicionar material complementar à palestra
    Dado que existe uma palestra cadastrada
    Quando adiciono o conteúdo "Slides da palestra" do tipo "Slides"
    Então a resposta deve ter status 201
    E a palestra deve possuir o conteúdo "Slides da palestra"

  Cenário: Uma pessoa não pode ser vinculada duas vezes à mesma palestra
    Dado que existe uma palestra cadastrada
    Quando tento adicionar novamente o palestrante
    Então a resposta deve ter status 409
    E a resposta deve ser um problema com código "Palestras.PalestranteJaVinculado"
