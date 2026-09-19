using Shared.Contracts.Identidade;
using Shared.Http.Results;

namespace Module.Identidade.Domain;

public static class IdentidadeErros
{
    public static readonly Error CredenciaisInvalidas = Error.Unauthorized("Identidade.CredenciaisInvalidas", "E-mail ou senha inválidos.");
    public static readonly Error UsuarioBloqueado = Error.Unauthorized("Identidade.UsuarioBloqueado", "Usuário temporariamente bloqueado por excesso de tentativas. Tente novamente em alguns minutos.");
    public static readonly Error UsuarioInativo = Error.Unauthorized("Identidade.UsuarioInativo", "Usuário inativo. Procure um administrador.");
    public static readonly Error UsuarioNaoEncontrado = Error.NotFound("Identidade.UsuarioNaoEncontrado", "Usuário não encontrado.");
    public static readonly Error EmailJaCadastrado = Error.Conflict("Identidade.EmailJaCadastrado", "Já existe um usuário com este e-mail.");

    public static Error PerfilInvalido(string perfil) =>
        Error.BusinessRule("Identidade.PerfilInvalido", $"Perfil '{perfil}' inválido. Perfis aceitos: {string.Join(", ", PerfisPadrao.Todos)}.");

    public static Error SenhaFraca(IEnumerable<string> motivos) =>
        Error.BusinessRule("Identidade.SenhaFraca", $"A senha não atende à política de segurança: {string.Join(" ", motivos)}");

    public static Error FalhaAoRegistrar(IEnumerable<string> motivos) =>
        Error.BusinessRule("Identidade.FalhaAoRegistrar", $"Não foi possível registrar o usuário: {string.Join(" ", motivos)}");
}
