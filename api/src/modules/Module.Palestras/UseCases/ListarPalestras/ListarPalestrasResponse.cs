namespace Module.Palestras.UseCases.ListarPalestras;

public sealed record ListarPalestrasItemResponse(
    Guid Id,
    Guid EventoId,
    Guid? SalaId,
    string PalestraTitulo,
    DateTimeOffset PalestraInicio,
    DateTimeOffset PalestraFim,
    int PalestrantesQuantidade,
    int PresencasQuantidade);
