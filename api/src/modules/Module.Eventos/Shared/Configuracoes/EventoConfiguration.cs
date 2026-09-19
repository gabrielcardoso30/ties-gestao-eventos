using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Eventos.Domain;
using Shared.Data;

namespace Module.Eventos.Shared.Configuracoes;

internal sealed class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ConfigurarEntidadeBase("Eventos");
        builder.Property(e => e.EventoNome).HasMaxLength(200).IsRequired();
        builder.Property(e => e.EventoDescricao).HasMaxLength(4000);
        builder.Property(e => e.EventoLinkRemoto).HasMaxLength(500);
        builder.Property(e => e.EventoCancelamentoMotivo).HasMaxLength(1000);
        builder.HasIndex(e => e.EventoDataInicio);
        builder.HasIndex(e => e.EventoSituacao);
        builder.HasIndex(e => e.LocalId);
        builder.HasMany(e => e.Inscricoes).WithOne(i => i.Evento).HasForeignKey(i => i.EventoId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.Inscricoes).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
        builder.HasMany(e => e.Trilhas).WithOne(t => t.Evento).HasForeignKey(t => t.EventoId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.Trilhas).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
    }
}
