using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Observability.Telemetria;

namespace Shared.Http.Endpoints;

public static class EndpointGroupExtensions
{
    /// <summary>Cria o grupo padrão do módulo: prefixo <c>api/v1/{rota}</c>, uma única tag Swagger, autenticação exigida por padrão e respostas comuns documentadas.</summary>
    public static RouteGroupBuilder MapModuleGroup(this IEndpointRouteBuilder endpoints, string rota, string tag)
    {
        return endpoints.MapGroup($"api/v1/{rota}")
            .WithTags(tag)
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    /// <summary>Mapeia todos os <see cref="IEndpoint"/> do assembly no grupo informado.</summary>
    public static RouteGroupBuilder MapEndpointsFromAssembly(this RouteGroupBuilder group, Assembly assembly)
    {
        var tipos = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t));
        foreach (var tipo in tipos)
        {
            var map = tipo.GetMethod(nameof(IEndpoint.Map), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"{tipo.Name} não implementa Map estático.");
            map.Invoke(null, [group]);
        }

        return group;
    }

    /// <summary>
    /// Registra todos os casos de uso (<see cref="IUseCase{TRequest,TResponse}"/>) do assembly como scoped,
    /// envolvidos pelo decorator de telemetria do módulo.
    /// </summary>
    public static IServiceCollection AddUseCasesFromAssembly(this IServiceCollection services, Assembly assembly, ModuleTelemetry telemetry)
    {
        foreach (var tipo in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            foreach (var iface in tipo.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUseCase<,>)))
            {
                var decorator = typeof(TelemetryUseCaseDecorator<,>).MakeGenericType(iface.GenericTypeArguments);
                services.AddScoped(tipo);
                services.AddScoped(iface, sp => ActivatorUtilities.CreateInstance(sp, decorator, sp.GetRequiredService(tipo), telemetry));
            }
        }

        return services;
    }
}
