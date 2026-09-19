using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Eventos.Domain;
using Shared.Data;

namespace Module.Eventos.Shared.Configuracoes;

internal sealed class InscricaoConfiguration : IEntityTypeConfiguration<Inscricao>
{
    public void Configure(EntityTypeBuilder<Inscricao> builder)
    {
        builder.ConfigurarEntidadeBase("Inscricoes");
        builder.Ignore(i => i.EstaConfirmada);
        builder.HasIndex(i => new { i.EventoId, i.InscricaoSituacao });

        // Uma única inscrição confirmada por pessoa em cada evento; protege contra corrida entre requisições concorrentes.
        builder.HasIndex(i => new { i.EventoId, i.PessoaId })
            .IsUnique()
            .HasDatabaseName("IX_Inscricoes_EventoId_PessoaId_Confirmada")
            .HasFilter("\"InscricaoSituacao\" = 'Confirmada'");
    }
}
