using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.ObterPalestra;

public sealed record ObterPalestraResponse(
    Guid Id,
    Guid EventoId,
    string EventoNome,
    Guid? SalaId,
    string? SalaNome,
    string PalestraTitulo,
    string? PalestraDescricao,
    DateTimeOffset PalestraInicio,
    DateTimeOffset PalestraFim,
    int PalestraCargaHorariaMinutos,
    IReadOnlyList<ObterPalestraPalestranteResponse> Palestrantes,
    IReadOnlyList<ObterPalestraConteudoResponse> Conteudos,
    int PresencasQuantidade,
    int CertificadosQuantidade,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AlteradoEm);

public sealed record ObterPalestraPalestranteResponse(Guid PessoaId, string PessoaNome, PalestrantePapel PalestrantePapel);

public sealed record ObterPalestraConteudoResponse(Guid Id, string ConteudoTitulo, ConteudoTipo ConteudoTipo, string ConteudoUrl, string? ConteudoDescricao);
