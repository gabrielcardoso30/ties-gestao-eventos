namespace Module.Identidade.UseCases.CriarSessao;

/// <summary>Credenciais de login. O e-mail é o identificador do usuário.</summary>
public sealed record CriarSessaoRequest(string UsuarioEmail, string Senha);
