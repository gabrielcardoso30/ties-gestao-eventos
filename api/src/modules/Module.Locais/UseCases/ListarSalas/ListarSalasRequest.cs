using Microsoft.AspNetCore.Mvc;

namespace Module.Locais.UseCases.ListarSalas;

public sealed record ListarSalasRequest([FromRoute(Name = "id")] Guid LocalId, [FromQuery] bool? EstaAtivo);
