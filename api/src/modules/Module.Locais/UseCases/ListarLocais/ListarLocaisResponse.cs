namespace Module.Locais.UseCases.ListarLocais;

public sealed record ListarLocaisItemResponse(Guid Id, string LocalNome, string EnderecoCidade, string EnderecoUf, int SalasQuantidade, int LocalCapacidadeTotal, bool EstaAtivo);
