namespace Module.Identidade.UseCases.ListarUsuarios;

public sealed record ListarUsuariosItemResponse(Guid Id, string UsuarioNome, string UsuarioEmail, IReadOnlyList<string> Perfis, bool EstaAtivo, DateTimeOffset? UltimoAcessoEm);
