using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Palestras.Domain;
using Shared.Data;

namespace Module.Palestras.Shared.Configuracoes;

internal sealed class PalestraPalestranteConfiguration : IEntityTypeConfiguration<PalestraPalestrante>
{
    public void Configure(EntityTypeBuilder<PalestraPalestrante> builder)
    {
        builder.ConfigurarEntidadeBase("PalestraPalestrantes");
        builder.HasIndex(p => new { p.PalestraId, p.PessoaId }).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
        builder.HasIndex(p => p.PessoaId);
    }
}
