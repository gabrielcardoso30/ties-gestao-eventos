using Microsoft.AspNetCore.Http;

namespace Shared.WebHost.Security;

/// <summary>Cabeçalhos de segurança padrão para uma API JSON (não interferem no Swagger).</summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers.Remove("Server");
        return next(context);
    }
}
