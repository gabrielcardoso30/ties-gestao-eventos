using Shared.Contracts.Pessoas;
using Shared.Data.Entidades;

namespace Module.Pessoas.Domain;

/// <summary>
/// Agregado Pessoa: palestrantes e participantes são Pessoas; o papel nasce do relacionamento com eventos e palestras.
/// E-mail é armazenado em minúsculas e CPF apenas com dígitos; ambos são únicos entre pessoas ativas (não excluídas).
/// </summary>
public sealed class Pessoa : EntidadeBase
{
    private Pessoa()
    {
    }

    public string PessoaNome { get; private set; } = string.Empty;
    public string PessoaEmail { get; private set; } = string.Empty;
    public string? PessoaTelefone { get; private set; }

    /// <summary>CPF somente dígitos (11 caracteres), sem máscara.</summary>
    public string? PessoaDocumento { get; private set; }
    public string? PessoaEmpresa { get; private set; }
    public string? PessoaCargo { get; private set; }
    public string? PessoaMiniBio { get; private set; }
    public string? PessoaFotoUrl { get; private set; }

    public static Pessoa Criar(
        string pessoaNome,
        string pessoaEmail,
        string? pessoaTelefone,
        string? pessoaDocumento,
        string? pessoaEmpresa,
        string? pessoaCargo,
        string? pessoaMiniBio,
        string? pessoaFotoUrl)
    {
        var pessoa = new Pessoa();
        pessoa.Atualizar(pessoaNome, pessoaEmail, pessoaTelefone, pessoaDocumento, pessoaEmpresa, pessoaCargo, pessoaMiniBio, pessoaFotoUrl, estaAtivo: true);
        pessoa.RegistrarEvento(new PessoaCriada(pessoa.Id, pessoa.PessoaNome, pessoa.PessoaEmail));
        return pessoa;
    }

    public void Atualizar(
        string pessoaNome,
        string pessoaEmail,
        string? pessoaTelefone,
        string? pessoaDocumento,
        string? pessoaEmpresa,
        string? pessoaCargo,
        string? pessoaMiniBio,
        string? pessoaFotoUrl,
        bool estaAtivo)
    {
        PessoaNome = pessoaNome.Trim();
        PessoaEmail = NormalizarEmail(pessoaEmail);
        PessoaTelefone = Limpar(pessoaTelefone);
        PessoaDocumento = Cpf.Normalizar(pessoaDocumento);
        PessoaEmpresa = Limpar(pessoaEmpresa);
        PessoaCargo = Limpar(pessoaCargo);
        PessoaMiniBio = Limpar(pessoaMiniBio);
        PessoaFotoUrl = Limpar(pessoaFotoUrl);
        EstaAtivo = estaAtivo;
    }

    public void MarcarExcluida() => RegistrarEvento(new PessoaExcluida(Id));

    /// <summary>E-mail é comparado sem distinção de maiúsculas: sempre armazenado em minúsculas e sem espaços nas pontas.</summary>
    public static string NormalizarEmail(string pessoaEmail) => pessoaEmail.Trim().ToLowerInvariant();

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
