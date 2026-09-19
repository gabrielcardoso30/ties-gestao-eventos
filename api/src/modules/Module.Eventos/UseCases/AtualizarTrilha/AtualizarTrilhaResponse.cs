namespace Module.Eventos.UseCases.AtualizarTrilha;
public sealed record AtualizarTrilhaResponse(Guid Id, Guid EventoId, string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor, bool EstaAtivo);
