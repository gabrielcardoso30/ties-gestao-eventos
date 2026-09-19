namespace Module.Identidade.UseCases.CriarSessao;

public sealed record CriarSessaoResponse(string AccessToken, DateTimeOffset ExpiraEm, CriarSessaoUsuarioResponse Usuario);

public sealed record CriarSessaoUsuarioResponse(Guid Id, string UsuarioNome, string UsuarioEmail, IReadOnlyList<string> Perfis);
