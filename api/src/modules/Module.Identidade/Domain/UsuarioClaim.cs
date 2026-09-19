using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Claim adicional de um usuário. Tabela <c>Identidade.UsuarioClaims</c>.</summary>
public sealed class UsuarioClaim : IdentityUserClaim<Guid>;
