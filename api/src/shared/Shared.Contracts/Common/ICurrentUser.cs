namespace Shared.Contracts.Common;

/// <summary>Usuário autenticado na requisição corrente. Fonte para auditoria (CriadoPor/AlteradoPor/ExcluidoPor).</summary>
public interface ICurrentUser
{
    Guid? Id { get; }
    string? Nome { get; }
    string? Email { get; }
    bool EstaAutenticado { get; }
    IReadOnlyCollection<string> Perfis { get; }
    bool PossuiPerfil(string perfil);
}
