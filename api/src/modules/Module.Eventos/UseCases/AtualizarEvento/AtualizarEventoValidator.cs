using FluentValidation;
using Module.Eventos.Domain;
using Module.Eventos.UseCases.CriarEvento;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.AtualizarEvento;

internal sealed class AtualizarEventoValidator : AbstractValidator<AtualizarEventoRequest>
{
    public AtualizarEventoValidator()
    {
        RuleFor(r => r.EventoNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(200).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EventoDescricao).MaximumLength(4000).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EventoDataInicio).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio);
        RuleFor(r => r.EventoDataFim).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio)
            .GreaterThan(r => r.EventoDataInicio).WithMessage(MensagensValidacaoEventos.DataFimPosterior);
        RuleFor(r => r.EventoFormato).IsInEnum().WithMessage(MensagensValidacaoEventos.FormatoInvalido);
        RuleFor(r => r.EventoLinkRemoto).MaximumLength(500).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EventoCapacidadeMaxima).GreaterThan(0).When(r => r.EventoCapacidadeMaxima.HasValue).WithMessage(MensagensValidacao.MaiorQueZero);
        RuleFor(r => r.LocalId).NotEqual(Guid.Empty).When(r => r.LocalId.HasValue).WithMessage(MensagensValidacao.GuidObrigatorio);

        RuleFor(r => r.LocalId).NotNull().WithMessage(MensagensValidacaoEventos.LocalObrigatorio)
            .When(r => r.EventoFormato is EventoFormato.Presencial or EventoFormato.Hibrido);
        RuleFor(r => r.LocalId).Null().WithMessage(MensagensValidacaoEventos.LocalNaoPermitido)
            .When(r => r.EventoFormato == EventoFormato.Remoto);
        RuleFor(r => r.EventoLinkRemoto).NotEmpty().WithMessage(MensagensValidacaoEventos.LinkObrigatorio)
            .When(r => r.EventoFormato is EventoFormato.Remoto or EventoFormato.Hibrido);
    }
}
