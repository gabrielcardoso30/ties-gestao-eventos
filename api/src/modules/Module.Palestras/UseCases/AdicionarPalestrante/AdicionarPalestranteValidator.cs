using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

internal sealed class AdicionarPalestranteValidator : AbstractValidator<AdicionarPalestranteRequest>
{
    public AdicionarPalestranteValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PalestrantePapel).IsInEnum();
    }
}
