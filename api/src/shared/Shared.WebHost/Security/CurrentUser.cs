using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Common;

namespace Shared.WebHost.Security;

/// <summary>Usuário corrente a partir das claims do JWT. Singleton (via IHttpContextAccessor) para funcionar com DbContext pooling.</summary>
internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? Id => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub"), out var id) ? id : null;
    public string? Nome => Principal?.FindFirstValue(ClaimTypes.Name) ?? Principal?.FindFirstValue("name");
    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");
    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated ?? false;
    public IReadOnlyCollection<string> Perfis => Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
    public bool PossuiPerfil(string perfil) => Principal?.IsInRole(perfil) ?? false;
}
