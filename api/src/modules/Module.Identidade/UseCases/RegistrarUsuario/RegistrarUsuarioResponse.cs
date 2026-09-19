namespace Module.Identidade.UseCases.RegistrarUsuario;

public sealed record RegistrarUsuarioResponse(Guid Id, string UsuarioNome, string UsuarioEmail, IReadOnlyList<string> Perfis);
