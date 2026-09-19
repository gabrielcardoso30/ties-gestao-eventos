using Shared.Contracts.Identidade;
using Shared.Http.Results;

namespace Module.Identidade.Domain;

/// <summary>Normaliza nomes de perfis informados pela API para os nomes canônicos de <see cref="PerfisPadrao"/>.</summary>
public static class PerfisValidos
{
    /// <summary>Aceita variações de caixa e espaços; devolve a lista canônica, sem duplicatas e ordenada. Perfil desconhecido → <c>Identidade.PerfilInvalido</c>.</summary>
    public static Result<IReadOnlyList<string>> Normalizar(IEnumerable<string> perfis)
    {
        var canonicos = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var informado in perfis)
        {
            var canonico = PerfisPadrao.Todos.FirstOrDefault(p => string.Equals(p, informado?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (canonico is null)
            {
                return IdentidadeErros.PerfilInvalido(informado ?? string.Empty);
            }

            canonicos.Add(canonico);
        }

        return canonicos.ToList();
    }
}
