namespace Module.Eventos.UseCases.ListarTrilhas;
public sealed record ListarTrilhasItemResponse(Guid Id, Guid EventoId, string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor, bool EstaAtivo);
