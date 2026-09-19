using System.Text.Json.Serialization;

namespace Module.Palestras.UseCases.EmitirCertificado;

public sealed record EmitirCertificadoResponse(
    Guid Id,
    string CertificadoCodigo,
    Guid PalestraId,
    string PalestraTitulo,
    Guid PessoaId,
    string PessoaNome,
    DateTimeOffset CertificadoEmitidoEm,
    int CertificadoCargaHorariaMinutos)
{
    /// <summary>Indica se o certificado foi criado nesta chamada (201) ou já existia (200). Não é serializado.</summary>
    [JsonIgnore]
    public bool Criado { get; init; }
}
