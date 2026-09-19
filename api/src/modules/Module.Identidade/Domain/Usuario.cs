using Microsoft.AspNetCore.Identity;
using Shared.Contracts.Identidade;
using Shared.Contracts.Integracao;
using Shared.Data.Entidades;

namespace Module.Identidade.Domain;

/// <summary>
/// Usuário da aplicação (ASP.NET Core Identity sobrescrito). Além dos campos do Identity, carrega auditoria,
/// soft delete e eventos de integração, como qualquer entidade principal do monolito. Tabela <c>Identidade.Usuarios</c>.
/// </summary>
public sealed class Usuario : IdentityUser<Guid>, IEntidadeAuditavel, IEmissorDeEventos
{
    private readonly List<IIntegrationEvent> _eventos = [];

    private Usuario()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>Nome de exibição do usuário (vira a claim <c>name</c> do token).</summary>
    public string UsuarioNome { get; private set; } = string.Empty;

    /// <summary>Último login bem-sucedido.</summary>
    public DateTimeOffset? UltimoAcessoEm { get; private set; }

    public DateTimeOffset CriadoEm { get; set; }
    public string CriadoPor { get; set; } = string.Empty;
    public DateTimeOffset? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
    public DateTimeOffset? ExcluidoEm { get; set; }
    public string? ExcluidoPor { get; set; }
    public bool EstaAtivo { get; set; } = true;

    public IReadOnlyCollection<IIntegrationEvent> Eventos => _eventos.AsReadOnly();

    /// <summary>Cria o usuário (e-mail é também o nome de login) e registra <see cref="UsuarioRegistrado"/> para o Outbox.</summary>
    public static Usuario Criar(string usuarioNome, string usuarioEmail)
    {
        var email = usuarioEmail.Trim();
        var usuario = new Usuario
        {
            UsuarioNome = usuarioNome.Trim(),
            UserName = email,
            Email = email,
        };
        usuario.RegistrarEvento(new UsuarioRegistrado(usuario.Id, email, usuario.UsuarioNome));
        return usuario;
    }

    /// <summary>Login bem-sucedido: atualiza <see cref="UltimoAcessoEm"/> e registra <see cref="UsuarioAutenticado"/>.</summary>
    public void RegistrarAcesso(DateTimeOffset agora)
    {
        UltimoAcessoEm = agora;
        RegistrarEvento(new UsuarioAutenticado(Id, Email ?? string.Empty));
    }

    public void RegistrarEvento(IIntegrationEvent integrationEvent) => _eventos.Add(integrationEvent);

    public void LimparEventos() => _eventos.Clear();
}
