using FluentValidation;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.ObterPessoa;

internal sealed class ObterPessoaValidator : AbstractValidator<ObterPessoaRequest>
{
    public ObterPessoaValidator() => RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
