using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Palestras.Domain;
using Shared.Data;

namespace Module.Palestras.Shared.Configuracoes;

internal sealed class PalestraConfiguration : IEntityTypeConfiguration<Palestra>
{
    public void Configure(EntityTypeBuilder<Palestra> builder)
    {
        builder.ConfigurarEntidadeBase("Palestras");
        builder.Property(p => p.PalestraTitulo).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PalestraDescricao).HasMaxLength(4000);
        builder.Ignore(p => p.PalestraCargaHorariaMinutos);

        builder.HasIndex(p => p.EventoId);
        builder.HasIndex(p => p.TrilhaId);
        builder.HasIndex(p => new { p.SalaId, p.PalestraInicio });

        builder.HasMany(p => p.Palestrantes).WithOne().HasForeignKey(x => x.PalestraId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Conteudos).WithOne().HasForeignKey(x => x.PalestraId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Presencas).WithOne().HasForeignKey(x => x.PalestraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Certificados).WithOne().HasForeignKey(x => x.PalestraId).OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.Palestrantes).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
        builder.Navigation(p => p.Conteudos).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
        builder.Navigation(p => p.Presencas).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
        builder.Navigation(p => p.Certificados).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
    }
}
