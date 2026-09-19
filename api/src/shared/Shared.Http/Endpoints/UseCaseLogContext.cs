using System.Collections;
using System.Reflection;

namespace Shared.Http.Endpoints;

/// <summary>
/// Extrai somente metadados seguros e úteis para diagnóstico do request. Textos livres, credenciais e demais dados
/// pessoais não entram no log. Coleções são representadas apenas pela quantidade de itens.
/// </summary>
internal static class UseCaseLogContext
{
    private static readonly string[] SafeSuffixes =
    [
        "Id", "Situacao", "Formato", "EstaAtivo", "Pagina", "Tamanho", "Capacidade", "Quantidade"
    ];

    public static IReadOnlyDictionary<string, object?> From<TRequest>(TRequest request)
    {
        var contexto = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (request is null)
        {
            return contexto;
        }

        foreach (var propriedade in typeof(TRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!propriedade.CanRead || propriedade.GetIndexParameters().Length != 0)
            {
                continue;
            }

            var valor = propriedade.GetValue(request);
            if (valor is IEnumerable colecao and not string)
            {
                var quantidade = ContarSemEnumerar(colecao);
                if (quantidade.HasValue)
                {
                    contexto[$"{propriedade.Name}Quantidade"] = quantidade.Value;
                }
                continue;
            }

            var tipo = Nullable.GetUnderlyingType(propriedade.PropertyType) ?? propriedade.PropertyType;
            if (tipo.IsEnum || tipo == typeof(bool) || SafeSuffixes.Any(s => propriedade.Name.EndsWith(s, StringComparison.Ordinal)))
            {
                contexto[propriedade.Name] = valor;
            }
        }

        return contexto;
    }

    private static int? ContarSemEnumerar(IEnumerable colecao)
    {
        if (colecao is ICollection collection)
        {
            return collection.Count;
        }

        var interfaceContagem = colecao.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
        return interfaceContagem?.GetProperty(nameof(IReadOnlyCollection<object>.Count))?.GetValue(colecao) as int?;
    }
}
