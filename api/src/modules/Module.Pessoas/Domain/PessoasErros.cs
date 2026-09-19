using Shared.Http.Results;

namespace Module.Pessoas.Domain;

public static class PessoasErros
{
    public static readonly Error PessoaNaoEncontrada = Error.NotFound("Pessoas.PessoaNaoEncontrada", "Pessoa não encontrada.");
    public static readonly Error EmailJaCadastrado = Error.Conflict("Pessoas.EmailJaCadastrado", "Já existe uma pessoa ativa com este e-mail.");
    public static readonly Error DocumentoJaCadastrado = Error.Conflict("Pessoas.DocumentoJaCadastrado", "Já existe uma pessoa ativa com este CPF.");
}
