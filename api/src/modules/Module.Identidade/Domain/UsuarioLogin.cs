using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Login externo (provedor) de um usuário. Tabela <c>Identidade.UsuarioLogins</c>.</summary>
public sealed class UsuarioLogin : IdentityUserLogin<Guid>;
