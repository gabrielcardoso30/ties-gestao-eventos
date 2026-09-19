using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.CriarPalestra;

internal sealed class CriarPalestraValidator : AbstractValidator<CriarPalestraRequest>
{
    public CriarPalestraValidator()
    {
        RuleFor(r => r.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.TrilhaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.SalaId).NotEqual(Guid.Empty).When(r => r.SalaId.HasValue).WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PalestraTitulo).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(200).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PalestraDescricao).MaximumLength(4000).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.PalestraInicio).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio);
        RuleFor(r => r.PalestraFim).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio)
            .GreaterThan(r => r.PalestraInicio).WithMessage("O campo {PropertyName} deve ser posterior ao início da palestra.");
        RuleFor(r => r.Palestrantes).NotEmpty().WithMessage("A palestra precisa de ao menos um palestrante.");
        RuleForEach(r => r.Palestrantes).ChildRules(p =>
        {
            p.RuleFor(x => x.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
            p.RuleFor(x => x.PalestrantePapel).IsInEnum();
        });
    }
}
