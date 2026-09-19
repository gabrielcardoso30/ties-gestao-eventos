using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Claim associada a um perfil. Tabela <c>Identidade.PerfilClaims</c>.</summary>
public sealed class PerfilClaim : IdentityRoleClaim<Guid>;
