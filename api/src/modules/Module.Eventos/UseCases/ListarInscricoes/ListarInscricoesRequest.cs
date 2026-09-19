using System.Text.Json.Serialization;
using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.ListarInscricoes;

/// <summary>Filtros de listagem de inscrições de um evento (query string).</summary>
public sealed record ListarInscricoesRequest(InscricaoSituacao? InscricaoSituacao, int Pagina = 1, int TamanhoPagina = 20)
{
    /// <summary>Preenchido pela rota; não faz parte da query string.</summary>
    [JsonIgnore]
    public Guid EventoId { get; init; }
}
