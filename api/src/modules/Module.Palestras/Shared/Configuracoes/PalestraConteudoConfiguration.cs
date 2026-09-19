using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Palestras.Domain;
using Shared.Data;

namespace Module.Palestras.Shared.Configuracoes;

internal sealed class PalestraConteudoConfiguration : IEntityTypeConfiguration<PalestraConteudo>
{
    public void Configure(EntityTypeBuilder<PalestraConteudo> builder)
    {
        builder.ConfigurarEntidadeBase("PalestraConteudos");
        builder.Property(c => c.ConteudoTitulo).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ConteudoUrl).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.ConteudoDescricao).HasMaxLength(1000);
        builder.HasIndex(c => c.PalestraId);
    }
}
