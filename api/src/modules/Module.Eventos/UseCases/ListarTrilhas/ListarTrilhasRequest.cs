using Microsoft.AspNetCore.Mvc;
namespace Module.Eventos.UseCases.ListarTrilhas;
public sealed record ListarTrilhasRequest([FromRoute(Name="id")] Guid EventoId, [FromQuery] bool? EstaAtivo);
