using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Module.Identidade.Domain;

namespace Module.Identidade.Shared.Seguranca;

/// <summary>Store de perfis que identifica as consultas internas executadas pelo <see cref="RoleManager{TRole}"/>.</summary>
internal sealed class IdentidadeRoleStore
    : RoleStore<Perfil, IdentidadeDbContext, Guid, UsuarioPerfil, PerfilClaim>
{
    private readonly IdentidadeDbContext _db;

    public IdentidadeRoleStore(IdentidadeDbContext db, IdentityErrorDescriber describer) : base(db, describer) => _db = db;

    public override Task<Perfil?> FindByIdAsync(string roleId, CancellationToken cancellationToken = default) =>
        Guid.TryParse(roleId, out var id)
            ? _db.Perfis
                .TagWith("Identidade.RoleStore.FindById")
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            : Task.FromResult<Perfil?>(null);

    public override Task<Perfil?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default) =>
        _db.Perfis
            .TagWith("Identidade.RoleStore.FindByName")
            .FirstOrDefaultAsync(p => p.NormalizedName == normalizedName, cancellationToken);
}
