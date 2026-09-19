using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.RemoverPalestrante;

internal sealed class RemoverPalestranteValidator : AbstractValidator<RemoverPalestranteRequest>
{
    public RemoverPalestranteValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
