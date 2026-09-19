namespace Module.Locais.UseCases.AtualizarLocal;

public sealed record AtualizarLocalResponse(Guid Id, string LocalNome, DateTimeOffset? AlteradoEm);
