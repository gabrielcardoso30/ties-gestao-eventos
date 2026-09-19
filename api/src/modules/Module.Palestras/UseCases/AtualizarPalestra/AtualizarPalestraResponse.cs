namespace Module.Palestras.UseCases.AtualizarPalestra;

public sealed record AtualizarPalestraResponse(Guid Id, string PalestraTitulo, DateTimeOffset? AlteradoEm);
