namespace Module.Palestras.UseCases.ValidarCertificado;

public sealed record ValidarCertificadoResponse(
    string CertificadoCodigo,
    string PalestraTitulo,
    string EventoNome,
    string PessoaNome,
    DateTimeOffset PalestraInicio,
    DateTimeOffset CertificadoEmitidoEm,
    int CertificadoCargaHorariaMinutos);
