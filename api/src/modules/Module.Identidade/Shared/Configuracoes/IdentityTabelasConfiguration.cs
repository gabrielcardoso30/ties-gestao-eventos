using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Identidade.Domain;

namespace Module.Identidade.Shared.Configuracoes;

/// <summary>Tabelas auxiliares do Identity renomeadas para PascalCase em pt-BR (sem o prefixo AspNet*).</summary>
internal sealed class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder) => builder.ToTable("UsuarioPerfis");
}

internal sealed class UsuarioClaimConfiguration : IEntityTypeConfiguration<UsuarioClaim>
{
    public void Configure(EntityTypeBuilder<UsuarioClaim> builder)
    {
        builder.ToTable("UsuarioClaims");
        builder.Property(c => c.ClaimType).HasMaxLength(200);
        builder.Property(c => c.ClaimValue).HasMaxLength(1000);
    }
}

internal sealed class UsuarioLoginConfiguration : IEntityTypeConfiguration<UsuarioLogin>
{
    public void Configure(EntityTypeBuilder<UsuarioLogin> builder)
    {
        builder.ToTable("UsuarioLogins");
        builder.Property(l => l.LoginProvider).HasMaxLength(128);
        builder.Property(l => l.ProviderKey).HasMaxLength(128);
        builder.Property(l => l.ProviderDisplayName).HasMaxLength(200);
    }
}

internal sealed class UsuarioTokenConfiguration : IEntityTypeConfiguration<UsuarioToken>
{
    public void Configure(EntityTypeBuilder<UsuarioToken> builder)
    {
        builder.ToTable("UsuarioTokens");
        builder.Property(t => t.LoginProvider).HasMaxLength(128);
        builder.Property(t => t.Name).HasMaxLength(128);
        builder.Property(t => t.Value).HasMaxLength(2000);
    }
}

internal sealed class PerfilClaimConfiguration : IEntityTypeConfiguration<PerfilClaim>
{
    public void Configure(EntityTypeBuilder<PerfilClaim> builder)
    {
        builder.ToTable("PerfilClaims");
        builder.Property(c => c.ClaimType).HasMaxLength(200);
        builder.Property(c => c.ClaimValue).HasMaxLength(1000);
    }
}
