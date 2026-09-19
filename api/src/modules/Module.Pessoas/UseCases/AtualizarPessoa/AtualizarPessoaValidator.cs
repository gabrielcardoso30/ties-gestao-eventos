using FluentValidation;
using Module.Pessoas.Domain;
using Module.Pessoas.UseCases.CriarPessoa;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.AtualizarPessoa;

internal sealed class AtualizarPessoaValidator : AbstractValidator<AtualizarPessoaRequest>
{
    public AtualizarPessoaValidator()
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
