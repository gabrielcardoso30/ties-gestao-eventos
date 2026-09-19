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

  Cenário: E-mail de usuário deve ser único
    Dado que estou autenticado como "Administrador"
    E que cadastrei um usuário com perfil "Participante"
    Quando tento cadastrar outro usuário com o mesmo e-mail
    Então a resposta deve ter status 409
    E a resposta deve ser um problema com código "Identidade.EmailJaCadastrado"

  Cenário: Senha fraca é rejeitada
    Dado que estou autenticado como "Administrador"
    Quando tento cadastrar um usuário com senha fraca
    Então a resposta deve ter status 422
    E a resposta deve ser um problema com código "Identidade.SenhaFraca"

  Cenário: Perfil desconhecido é rejeitado
    Dado que estou autenticado como "Administrador"
    Quando cadastro um usuário com perfil "SuperUsuario"
    Então a resposta deve ter status 422
    E a resposta deve ser um problema com código "Identidade.PerfilInvalido"
