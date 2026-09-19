using Module.Locais.Domain;

namespace Module.Locais.UseCases.AtualizarSala;

public sealed record AtualizarSalaResponse(Guid Id, Guid LocalId, string SalaNome, int SalaCapacidade, SalaTipo SalaTipo, bool EstaAtivo);
