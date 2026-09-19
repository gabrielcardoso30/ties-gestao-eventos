using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Identidade.Domain;
using Shared.Contracts.Identidade;

namespace Module.Identidade.Shared.Seed;

/// <summary>
/// Seed idempotente na subida: perfis de <see cref="PerfisPadrao"/> e o administrador inicial.
/// Roda depois do migrador porque <c>AddSharedData</c> (que o registra) é chamado antes dos módulos.
/// Nunca loga a senha.
/// </summary>
internal sealed class IdentidadeSeedHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<AdministradorInicialOptions> adminOptions,
    ILogger<IdentidadeSeedHostedService> logger) : IHostedService
{
    private static readonly IReadOnlyDictionary<string, string> Descricoes = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [PerfisPadrao.Administrador] = "Acesso total: administra usuários, perfis e todos os módulos.",
        [PerfisPadrao.Organizador] = "Gestão de eventos, palestras, locais e pessoas.",
        [PerfisPadrao.Participante] = "Inscrição em eventos e acesso às próprias presenças e certificados.",
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Perfil>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        await GarantirPerfisAsync(roleManager);
        await GarantirAdministradorAsync(userManager);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task GarantirPerfisAsync(RoleManager<Perfil> roleManager)
    {
        foreach (var nome in PerfisPadrao.Todos)
        {
            if (await roleManager.RoleExistsAsync(nome))
            {
                continue;
            }

            var resultado = await roleManager.CreateAsync(Perfil.Criar(nome, Descricoes.GetValueOrDefault(nome)));
            if (resultado.Succeeded)
            {
                logger.LogInformation("Perfil {Perfil} criado pelo seed", nome);
            }
            else
            {
                LogFalha("Falha ao criar perfil {Alvo}", nome, resultado);
            }
        }
    }

    private async Task GarantirAdministradorAsync(UserManager<Usuario> userManager)
    {
        var admin = adminOptions.Value;
        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Senha))
        {
            logger.LogInformation("Administrador inicial não configurado (Identidade:AdministradorInicial:Senha vazia); seed ignorado");
            return;
        }

        var usuario = await userManager.FindByEmailAsync(admin.Email);
        if (usuario is null)
        {
            usuario = Usuario.Criar(string.IsNullOrWhiteSpace(admin.Nome) ? "Administrador" : admin.Nome, admin.Email);
            var criacao = await userManager.CreateAsync(usuario, admin.Senha);
            if (!criacao.Succeeded)
            {
                LogFalha("Falha ao criar administrador inicial {Alvo}", admin.Email, criacao);
                return;
            }

            logger.LogInformation("Administrador inicial {Email} criado pelo seed", admin.Email);
        }

        if (!await userManager.IsInRoleAsync(usuario, PerfisPadrao.Administrador))
        {
            var vinculo = await userManager.AddToRoleAsync(usuario, PerfisPadrao.Administrador);
            if (vinculo.Succeeded)
            {
                logger.LogInformation("Administrador inicial {Email} vinculado ao perfil {Perfil}", admin.Email, PerfisPadrao.Administrador);
            }
            else
            {
                LogFalha("Falha ao vincular administrador inicial {Alvo} ao perfil Administrador", admin.Email, vinculo);
            }
        }
    }

    private void LogFalha(string mensagem, string alvo, IdentityResult resultado)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            var erros = string.Join("; ", resultado.Errors.Select(e => $"{e.Code}: {e.Description}"));
#pragma warning disable CA2254 // template dinâmico intencional (mensagens fixas deste serviço)
            logger.LogError(mensagem + ": {Erros}", alvo, erros);
#pragma warning restore CA2254
        }
    }
}
