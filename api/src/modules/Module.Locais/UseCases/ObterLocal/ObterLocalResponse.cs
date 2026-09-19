using Module.Locais.Domain;

namespace Module.Locais.UseCases.ObterLocal;

public sealed record ObterLocalResponse(
    Guid Id,
    string LocalNome,
    string? LocalDescricao,
    string? EnderecoLogradouro,
    string? EnderecoNumero,
    string? EnderecoBairro,
    string EnderecoCidade,
    string EnderecoUf,
    string? EnderecoCep,
    int LocalCapacidadeTotal,
    bool EstaAtivo,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AlteradoEm,
    IReadOnlyList<ObterLocalSalaResponse> Salas);

public sealed record ObterLocalSalaResponse(Guid Id, string SalaNome, int SalaCapacidade, SalaTipo SalaTipo, string? SalaRecursos, bool EstaAtivo);
