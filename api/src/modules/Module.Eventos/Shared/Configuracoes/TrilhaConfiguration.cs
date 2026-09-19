using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Eventos.Domain;
using Shared.Data;

namespace Module.Eventos.Shared.Configuracoes;

internal sealed class TrilhaConfiguration : IEntityTypeConfiguration<Trilha>
{
    public void Configure(EntityTypeBuilder<Trilha> builder)
    {
        builder.ConfigurarEntidadeBase("Trilhas");
        builder.Property(t => t.TrilhaNome).HasMaxLength(120).IsRequired();
        builder.Property(t => t.TrilhaDescricao).HasMaxLength(1000);
        builder.Property(t => t.TrilhaCor).HasMaxLength(7);
        builder.HasIndex(t => new { t.EventoId, t.TrilhaNome }).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
    }
}
