using Shared.Contracts.Auditoria;

namespace Module.Auditoria.Domain;

/// <summary>
/// Trilha de auditoria: um registro imutável por alteração de entidade em qualquer módulo.
/// NÃO herda <c>EntidadeBase</c> (não sofre update, soft delete nem auto-auditoria). O <see cref="Id"/> é o Id do evento
/// <see cref="EntidadeAlterada"/> que o originou, o que torna a persistência idempotente diante de reentregas do Outbox.
/// </summary>
public sealed class RegistroAuditoria
{
    private RegistroAuditoria()
    {
    }

    public Guid Id { get; private set; }
    public string Modulo { get; private set; } = string.Empty;
    public string EntidadeNome { get; private set; } = string.Empty;
    public string EntidadeId { get; private set; } = string.Empty;

    /// <summary>Inclusao, Alteracao ou Exclusao (ver <see cref="OperacoesAuditoria"/>).</summary>
    public string Operacao { get; private set; } = string.Empty;

    /// <summary>JSON (jsonb) com os valores anteriores das propriedades alteradas; nulo em inclusões.</summary>
    public string? DadosAnteriores { get; private set; }

    /// <summary>JSON (jsonb) com os valores novos (todas as propriedades em inclusões; só as alteradas nos demais casos).</summary>
    public string? DadosNovos { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public string? UsuarioNome { get; private set; }
    public string? TraceId { get; private set; }

    /// <summary>Momento em que a alteração aconteceu no módulo de origem.</summary>
    public DateTimeOffset OcorridoEm { get; private set; }

    /// <summary>Momento em que este módulo persistiu o registro (latência do Outbox = RegistradoEm - OcorridoEm).</summary>
    public DateTimeOffset RegistradoEm { get; private set; }

    public static RegistroAuditoria Criar(EntidadeAlterada evento, DateTimeOffset registradoEm) => new()
    {
        Id = evento.Id,
        Modulo = evento.Modulo,
        EntidadeNome = evento.EntidadeNome,
        EntidadeId = evento.EntidadeId,
        Operacao = evento.Operacao,
        DadosAnteriores = evento.DadosAnteriores,
        DadosNovos = evento.DadosNovos,
        UsuarioId = evento.UsuarioId,
        UsuarioNome = evento.UsuarioNome,
        TraceId = evento.TraceId,
        OcorridoEm = evento.OcorridoEm,
        RegistradoEm = registradoEm,
    };
}
