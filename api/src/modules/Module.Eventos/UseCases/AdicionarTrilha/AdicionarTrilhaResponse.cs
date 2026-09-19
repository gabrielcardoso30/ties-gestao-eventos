namespace Module.Eventos.UseCases.AdicionarTrilha;
public sealed record AdicionarTrilhaResponse(Guid Id, Guid EventoId, string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor);
