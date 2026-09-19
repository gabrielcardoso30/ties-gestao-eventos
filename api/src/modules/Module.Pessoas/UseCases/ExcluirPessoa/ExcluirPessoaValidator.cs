using FluentValidation;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.ExcluirPessoa;

internal sealed class ExcluirPessoaValidator : AbstractValidator<ExcluirPessoaRequest>
{
    public ExcluirPessoaValidator() => RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
