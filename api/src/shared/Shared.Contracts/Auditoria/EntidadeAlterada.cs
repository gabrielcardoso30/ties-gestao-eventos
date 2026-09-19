using Shared.Contracts.Integracao;

namespace Shared.Contracts.Auditoria;

/// <summary>
/// Emitido automaticamente pela infraestrutura de dados a cada Insert/Update/Delete(soft) de qualquer entidade auditável.
/// Consumido pelo módulo Auditoria, que persiste o registro no seu próprio schema.
/// </summary>
public sealed record EntidadeAlterada(
    string Modulo,
    string EntidadeNome,
    string EntidadeId,
    string Operacao,
    string? DadosAnteriores,
    string? DadosNovos,
    Guid? UsuarioId,
    string? UsuarioNome,
    string? TraceId) : IntegrationEvent;

public static class OperacoesAuditoria
{
    public const string Inclusao = "Inclusao";
    public const string Alteracao = "Alteracao";
    public const string Exclusao = "Exclusao";
}
