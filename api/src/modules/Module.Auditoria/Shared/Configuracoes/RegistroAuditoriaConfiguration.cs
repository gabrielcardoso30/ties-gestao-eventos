using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Auditoria.Domain;

namespace Module.Auditoria.Shared.Configuracoes;

internal sealed class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Modulo).HasMaxLength(50).IsRequired();
        builder.Property(r => r.EntidadeNome).HasMaxLength(100).IsRequired();
        builder.Property(r => r.EntidadeId).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Operacao).HasMaxLength(20).IsRequired();
        builder.Property(r => r.DadosAnteriores).HasColumnType("jsonb");
        builder.Property(r => r.DadosNovos).HasColumnType("jsonb");
        builder.Property(r => r.UsuarioNome).HasMaxLength(150);
        builder.Property(r => r.TraceId).HasMaxLength(64);
        builder.HasIndex(r => new { r.Modulo, r.EntidadeNome, r.EntidadeId });
        builder.HasIndex(r => r.OcorridoEm);
        builder.HasIndex(r => r.UsuarioId);
    }
}
