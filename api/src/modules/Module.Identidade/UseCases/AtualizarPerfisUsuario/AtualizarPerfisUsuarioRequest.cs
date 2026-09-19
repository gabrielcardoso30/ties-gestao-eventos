using System.Text.Json.Serialization;

namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

/// <summary>Conjunto completo de perfis que o usuário deve possuir após a operação (substitui os atuais).</summary>
public sealed record AtualizarPerfisUsuarioRequest(IReadOnlyList<string> Perfis)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid UsuarioId { get; init; }
}
