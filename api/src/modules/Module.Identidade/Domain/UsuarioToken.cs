using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Token de usuário (recuperação de senha, autenticadores etc.). Tabela <c>Identidade.UsuarioTokens</c>.</summary>
public sealed class UsuarioToken : IdentityUserToken<Guid>;
