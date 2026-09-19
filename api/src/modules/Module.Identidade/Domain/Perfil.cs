using Microsoft.AspNetCore.Identity;

namespace Module.Identidade.Domain;

/// <summary>Perfil (role) de acesso. Os perfis conhecidos estão em <c>Shared.Contracts.Identidade.PerfisPadrao</c>. Tabela <c>Identidade.Perfis</c>.</summary>
public sealed class Perfil : IdentityRole<Guid>
{
    private Perfil()
    {
        Id = Guid.CreateVersion7();
    }

    public string? PerfilDescricao { get; private set; }

    public static Perfil Criar(string perfilNome, string? perfilDescricao) =>
        new() { Name = perfilNome, PerfilDescricao = perfilDescricao?.Trim() };
}
