using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Locais.Domain;
using Shared.Data;

namespace Module.Locais.Shared.Configuracoes;

internal sealed class LocalConfiguration : IEntityTypeConfiguration<Local>
{
    public void Configure(EntityTypeBuilder<Local> builder)
    {
        builder.ConfigurarEntidadeBase("Locais");
        builder.Property(l => l.LocalNome).HasMaxLength(150).IsRequired();
        builder.Property(l => l.LocalDescricao).HasMaxLength(1000);
        builder.Property(l => l.EnderecoLogradouro).HasMaxLength(200);
        builder.Property(l => l.EnderecoNumero).HasMaxLength(20);
        builder.Property(l => l.EnderecoBairro).HasMaxLength(100);
        builder.Property(l => l.EnderecoCidade).HasMaxLength(100).IsRequired();
        builder.Property(l => l.EnderecoUf).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(l => l.EnderecoCep).HasMaxLength(10);
        builder.HasIndex(l => l.LocalNome).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
        builder.HasMany(l => l.Salas).WithOne(s => s.Local).HasForeignKey(s => s.LocalId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(l => l.Salas).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
    }
}
