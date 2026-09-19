namespace Module.Identidade.UseCases.ObterUsuarioAtual;

public sealed record ObterUsuarioAtualResponse(Guid Id, string UsuarioNome, string UsuarioEmail, IReadOnlyList<string> Perfis, DateTimeOffset? UltimoAcessoEm);
