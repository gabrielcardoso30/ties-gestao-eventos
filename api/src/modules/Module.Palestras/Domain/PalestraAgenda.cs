using System.Linq.Expressions;

namespace Module.Palestras.Domain;

/// <summary>Regras de agenda: sobreposição de horários entre palestras na mesma sala.</summary>
public static class PalestraAgenda
{
    /// <summary>Dois períodos se sobrepõem quando um começa antes de o outro terminar e termina depois de o outro começar (limites tocantes não contam).</summary>
    public static bool Sobrepoe(DateTimeOffset inicio, DateTimeOffset fim, DateTimeOffset outroInicio, DateTimeOffset outroFim) =>
        inicio < outroFim && fim > outroInicio;

    /// <summary>Predicado traduzível pelo EF Core: palestras ativas que ocupam a sala no período, ignorando opcionalmente a própria palestra (atualização).</summary>
    public static Expression<Func<Palestra, bool>> OcupaSala(Guid salaId, DateTimeOffset inicio, DateTimeOffset fim, Guid? palestraIdIgnorada = null) =>
        p => p.SalaId == salaId
             && p.EstaAtivo
             && p.PalestraInicio < fim
             && p.PalestraFim > inicio
             && (palestraIdIgnorada == null || p.Id != palestraIdIgnorada);
}
