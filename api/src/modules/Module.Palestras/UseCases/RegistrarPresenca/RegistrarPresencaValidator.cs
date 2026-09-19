using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.RegistrarPresenca;

internal sealed class RegistrarPresencaValidator : AbstractValidator<RegistrarPresencaRequest>
{
    public RegistrarPresencaValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
