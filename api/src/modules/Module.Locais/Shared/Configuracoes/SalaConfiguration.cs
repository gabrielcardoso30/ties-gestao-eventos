using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Locais.Domain;
using Shared.Data;

namespace Module.Locais.Shared.Configuracoes;

internal sealed class SalaConfiguration : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> builder)
    {
        builder.ConfigurarEntidadeBase("Salas");
        builder.Property(s => s.SalaNome).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SalaRecursos).HasMaxLength(500);
        builder.HasIndex(s => new { s.LocalId, s.SalaNome }).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
    }
}
