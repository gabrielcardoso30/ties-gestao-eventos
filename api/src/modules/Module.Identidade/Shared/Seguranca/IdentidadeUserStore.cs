using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Module.Identidade.Domain;

namespace Module.Identidade.Shared.Seguranca;

/// <summary>
/// Store do ASP.NET Identity com as mesmas operações do store EF padrão, mas identificando as consultas internas.
/// Essas consultas são disparadas por <see cref="UserManager{TUser}"/> durante validação de unicidade e vínculo de perfis,
/// fora das expressões LINQ controladas diretamente pelos casos de uso.
/// </summary>
internal sealed class IdentidadeUserStore
    : UserStore<Usuario, Perfil, IdentidadeDbContext, Guid, UsuarioClaim, UsuarioPerfil, UsuarioLogin, UsuarioToken, PerfilClaim>
{
    private readonly IdentidadeDbContext _db;

    public IdentidadeUserStore(IdentidadeDbContext db, IdentityErrorDescriber describer) : base(db, describer) => _db = db;

    public override Task<Usuario?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        Guid.TryParse(userId, out var id)
            ? _db.Usuarios
                .TagWith("Identidade.UserStore.FindById")
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            : Task.FromResult<Usuario?>(null);

    public override Task<Usuario?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default) =>
        _db.Usuarios
            .TagWith("Identidade.UserStore.FindByName")
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

    public override Task<Usuario?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        _db.Usuarios
            .TagWith("Identidade.UserStore.FindByEmail")
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

    protected override Task<Usuario?> FindUserAsync(Guid userId, CancellationToken cancellationToken) =>
        _db.Usuarios
            .TagWith("Identidade.UserStore.FindUser")
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    protected override Task<Perfil?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
        _db.Perfis
            .TagWith("Identidade.UserStore.FindRole")
            .SingleOrDefaultAsync(p => p.NormalizedName == normalizedRoleName, cancellationToken);

    protected override Task<UsuarioPerfil?> FindUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken) =>
        _db.UsuarioPerfis
            .TagWith("Identidade.UserStore.FindUserRole")
            .FirstOrDefaultAsync(up => up.UserId == userId && up.RoleId == roleId, cancellationToken);
}
