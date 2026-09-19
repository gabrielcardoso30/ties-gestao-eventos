using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.AdicionarConteudo;

internal sealed class AdicionarConteudoValidator : AbstractValidator<AdicionarConteudoRequest>
{
    public AdicionarConteudoValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.ConteudoTitulo).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(200).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.ConteudoTipo).IsInEnum();
        RuleFor(r => r.ConteudoUrl).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(2000).WithMessage(MensagensValidacao.TamanhoMaximo)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).When(r => !string.IsNullOrWhiteSpace(r.ConteudoUrl)).WithMessage("O campo {PropertyName} deve ser uma URL absoluta válida.");
        RuleFor(r => r.ConteudoDescricao).MaximumLength(1000).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
