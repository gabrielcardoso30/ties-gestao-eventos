using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Palestras.Domain;
using Shared.Data;

namespace Module.Palestras.Shared.Configuracoes;

internal sealed class PresencaConfiguration : IEntityTypeConfiguration<Presenca>
{
    public void Configure(EntityTypeBuilder<Presenca> builder)
    {
        builder.ConfigurarEntidadeBase("Presencas");
        builder.HasIndex(p => new { p.PalestraId, p.PessoaId }).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
        builder.HasIndex(p => p.PessoaId);
    }
}
