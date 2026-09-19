using Module.Locais.Domain;

namespace Module.Locais.UseCases.ListarSalas;

public sealed record ListarSalasItemResponse(Guid Id, string SalaNome, int SalaCapacidade, SalaTipo SalaTipo, string? SalaRecursos, bool EstaAtivo);
