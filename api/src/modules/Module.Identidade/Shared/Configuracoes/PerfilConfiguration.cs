using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Identidade.Domain;

namespace Module.Identidade.Shared.Configuracoes;

internal sealed class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfis");
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PerfilDescricao).HasMaxLength(300);
        builder.Property(p => p.ConcurrencyStamp).HasMaxLength(100);
        builder.HasIndex(p => p.NormalizedName).HasDatabaseName("IX_Perfis_NormalizedName").IsUnique();
    }
}
