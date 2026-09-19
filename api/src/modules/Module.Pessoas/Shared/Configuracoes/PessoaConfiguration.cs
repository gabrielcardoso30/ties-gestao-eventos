using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Pessoas.Domain;
using Shared.Data;

namespace Module.Pessoas.Shared.Configuracoes;

internal sealed class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        builder.ConfigurarEntidadeBase("Pessoas");
        builder.Property(p => p.PessoaNome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.PessoaEmail).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PessoaTelefone).HasMaxLength(20);
        builder.Property(p => p.PessoaDocumento).HasMaxLength(Cpf.Tamanho).IsFixedLength();
        builder.Property(p => p.PessoaEmpresa).HasMaxLength(150);
        builder.Property(p => p.PessoaCargo).HasMaxLength(100);
        builder.Property(p => p.PessoaMiniBio).HasMaxLength(2000);
        builder.Property(p => p.PessoaFotoUrl).HasMaxLength(500);
        builder.HasIndex(p => p.PessoaEmail).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL");
        builder.HasIndex(p => p.PessoaDocumento).IsUnique().HasFilter("\"ExcluidoEm\" IS NULL AND \"PessoaDocumento\" IS NOT NULL");
        builder.HasIndex(p => p.PessoaNome);
    }
}
