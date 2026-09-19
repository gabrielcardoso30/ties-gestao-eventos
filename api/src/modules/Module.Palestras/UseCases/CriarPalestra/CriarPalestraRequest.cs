using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.CriarPalestra;

/// <summary>Dados para criação de uma palestra. Exige ao menos um palestrante.</summary>
public sealed record CriarPalestraRequest(
    Guid EventoId,
    Guid TrilhaId,
    Guid? SalaId,
    string PalestraTitulo,
    string? PalestraDescricao,
    DateTimeOffset PalestraInicio,
    DateTimeOffset PalestraFim,
    IReadOnlyList<CriarPalestraPalestranteRequest> Palestrantes);

public sealed record CriarPalestraPalestranteRequest(Guid PessoaId, PalestrantePapel PalestrantePapel);
