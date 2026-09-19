using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Vínculo usuário × perfil. Tabela <c>Identidade.UsuarioPerfis</c>.</summary>
public sealed class UsuarioPerfil : IdentityUserRole<Guid>;
