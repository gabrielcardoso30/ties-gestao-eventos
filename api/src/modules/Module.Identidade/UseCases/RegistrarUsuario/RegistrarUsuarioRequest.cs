namespace Module.Identidade.UseCases.RegistrarUsuario;

/// <summary>Dados para registro de um usuário. <c>Perfis</c> aceita apenas os perfis conhecidos (Administrador, Organizador, Participante).</summary>
public sealed record RegistrarUsuarioRequest(string UsuarioNome, string UsuarioEmail, string Senha, IReadOnlyList<string> Perfis);
