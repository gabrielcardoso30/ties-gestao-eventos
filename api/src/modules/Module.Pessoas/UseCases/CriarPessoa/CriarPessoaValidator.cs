using FluentValidation;
using Module.Pessoas.Domain;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.CriarPessoa;

internal sealed class CriarPessoaValidator : AbstractValidator<CriarPessoaRequest>
{
    public CriarPessoaValidator()
    {
        RuleFor(r => r.PessoaNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(150).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PessoaEmail).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(200).WithMessage(MensagensValidacao.TamanhoMaximo)
            .EmailAddress().WithMessage(MensagensValidacao.EmailInvalido);
        RuleFor(r => r.PessoaTelefone).MaximumLength(20).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PessoaDocumento).Must(Cpf.EhValido).When(r => !string.IsNullOrWhiteSpace(r.PessoaDocumento)).WithMessage(PessoasMensagens.CpfInvalido);
        RuleFor(r => r.PessoaEmpresa).MaximumLength(150).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PessoaCargo).MaximumLength(100).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PessoaMiniBio).MaximumLength(2000).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PessoaFotoUrl).MaximumLength(500).WithMessage(MensagensValidacao.TamanhoMaximo)
            .Must(PessoasMensagens.EhUrlHttp).When(r => !string.IsNullOrWhiteSpace(r.PessoaFotoUrl)).WithMessage(PessoasMensagens.UrlInvalida);
    }
}

/// <summary>Mensagens e regras de validação específicas do módulo Pessoas, compartilhadas entre os validators.</summary>
internal static class PessoasMensagens
{
    public const string CpfInvalido = "O campo {PropertyName} deve ser um CPF válido (11 dígitos, com ou sem máscara).";
    public const string UrlInvalida = "O campo {PropertyName} deve ser uma URL absoluta http(s).";

    public static bool EhUrlHttp(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
