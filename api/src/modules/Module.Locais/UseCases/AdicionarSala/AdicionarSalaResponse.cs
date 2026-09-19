using Module.Locais.Domain;

namespace Module.Locais.UseCases.AdicionarSala;

public sealed record AdicionarSalaResponse(Guid Id, Guid LocalId, string SalaNome, int SalaCapacidade, SalaTipo SalaTipo);
