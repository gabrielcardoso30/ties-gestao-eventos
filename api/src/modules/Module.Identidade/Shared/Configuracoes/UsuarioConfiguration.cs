using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Identidade.Domain;
using Shared.Data;

namespace Module.Identidade.Shared.Configuracoes;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ConfigurarEntidadeBase("Usuarios");
        builder.Property(u => u.UsuarioNome).HasMaxLength(150).IsRequired();
        builder.Property(u => u.UserName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedUserName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.ConcurrencyStamp).HasMaxLength(100);

        // Segredos nunca vão para a trilha de auditoria (EntidadeAlterada).
        builder.Property(u => u.PasswordHash).HasMaxLength(500).Sensivel();
        builder.Property(u => u.SecurityStamp).HasMaxLength(100).Sensivel();

        builder.HasIndex(u => u.NormalizedUserName).HasDatabaseName("IX_Usuarios_NormalizedUserName").IsUnique();
        builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("IX_Usuarios_NormalizedEmail");
    }
}
