#language: pt-BR
Funcionalidade: Autenticação e autorização
  Como responsável pela plataforma
  Quero controlar usuários e perfis
  Para proteger as operações administrativas

  Cenário: Administrador autentica com credenciais válidas
    Quando autentico com o administrador inicial
    Então a resposta deve ter status 200
    E a sessão deve conter um token e o perfil "Administrador"

  Cenário: Credenciais inválidas não revelam detalhes
    Quando tento autenticar com senha inválida
    Então a resposta deve ter status 401
    E a resposta deve ser um problema com código "Identidade.CredenciaisInvalidas"

  Cenário: Administrador cadastra um organizador
    Dado que estou autenticado como "Administrador"
    Quando cadastro um usuário com perfil "Organizador"
    Então a resposta deve ter status 201
    E o usuário deve possuir o perfil "Organizador"

  Cenário: Participante não pode cadastrar usuários
    Dado que estou autenticado como "Participante"
    Quando cadastro um usuário com perfil "Participante"
    Então a resposta deve ter status 403
