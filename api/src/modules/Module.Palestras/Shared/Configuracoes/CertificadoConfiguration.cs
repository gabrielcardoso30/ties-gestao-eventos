using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Palestras.Domain;
using Shared.Data;

namespace Module.Palestras.Shared.Configuracoes;

internal sealed class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.ConfigurarEntidadeBase("Certificados");
        builder.Property(c => c.CertificadoCodigo).HasMaxLength(CertificadoCodigoGerador.Tamanho).IsFixedLength().IsRequired();
        builder.HasIndex(c => c.CertificadoCodigo).IsUnique();
        builder.HasIndex(c => new { c.PalestraId, c.PessoaId });
    }
}
