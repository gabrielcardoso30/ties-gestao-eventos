namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

public sealed record AtualizarPerfisUsuarioResponse(Guid Id, IReadOnlyList<string> Perfis);
