namespace Module.Identidade.UseCases.ObterUsuarioAtual;

/// <summary>Identificador vindo da claim <c>sub</c> do token (via <c>ICurrentUser</c>); não faz parte da rota nem do corpo.</summary>
public sealed record ObterUsuarioAtualRequest(Guid UsuarioId);
