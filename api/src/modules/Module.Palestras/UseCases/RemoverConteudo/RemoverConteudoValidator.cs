using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.RemoverConteudo;

internal sealed class RemoverConteudoValidator : AbstractValidator<RemoverConteudoRequest>
{
    public RemoverConteudoValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.ConteudoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
