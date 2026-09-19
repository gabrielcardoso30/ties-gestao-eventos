using Shared.Contracts.Integracao;

namespace Shared.Contracts.Identidade;

public sealed record UsuarioRegistrado(Guid UsuarioId, string UsuarioEmail, string UsuarioNome) : IntegrationEvent;
public sealed record UsuarioAutenticado(Guid UsuarioId, string UsuarioEmail) : IntegrationEvent;
