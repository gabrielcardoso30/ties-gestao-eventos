using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.CriarLocal;

internal sealed class CriarLocalValidator : AbstractValidator<CriarLocalRequest>
{
    public CriarLocalValidator()
    {
        RuleFor(r => r.LocalNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(150).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.LocalDescricao).MaximumLength(1000).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EnderecoCidade).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(100).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EnderecoUf).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).Length(2).WithMessage("O campo {PropertyName} deve ter 2 letras (UF).");
        RuleFor(r => r.EnderecoCep).MaximumLength(10).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.CapacidadeAmbienteUnico).GreaterThan(0).When(r => r.CapacidadeAmbienteUnico.HasValue).WithMessage(MensagensValidacao.MaiorQueZero);
    }
}
