namespace Module.Locais.UseCases.CriarLocal;

/// <summary>Dados para criação de um local. Informe <c>CapacidadeAmbienteUnico</c> quando o local tiver um único ambiente.</summary>
public sealed record CriarLocalRequest(
    string LocalNome,
    string? LocalDescricao,
    string? EnderecoLogradouro,
    string? EnderecoNumero,
    string? EnderecoBairro,
    string EnderecoCidade,
    string EnderecoUf,
    string? EnderecoCep,
    int? CapacidadeAmbienteUnico);
