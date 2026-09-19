using Shared.Data.Entidades;

namespace Module.Palestras.Domain;

/// <summary>Certificado de participação em uma palestra, validável publicamente pelo <see cref="CertificadoCodigo"/>.</summary>
public sealed class Certificado : EntidadeBase
{
    private Certificado()
    {
    }

    internal Certificado(Guid palestraId, Guid pessoaId, string certificadoCodigo, DateTimeOffset certificadoEmitidoEm, int certificadoCargaHorariaMinutos)
    {
        PalestraId = palestraId;
        PessoaId = pessoaId;
        CertificadoCodigo = certificadoCodigo;
        CertificadoEmitidoEm = certificadoEmitidoEm;
        CertificadoCargaHorariaMinutos = certificadoCargaHorariaMinutos;
    }

    public Guid PalestraId { get; private set; }
    public Guid PessoaId { get; private set; }
    public string CertificadoCodigo { get; private set; } = string.Empty;
    public DateTimeOffset CertificadoEmitidoEm { get; private set; }
    public int CertificadoCargaHorariaMinutos { get; private set; }
}
