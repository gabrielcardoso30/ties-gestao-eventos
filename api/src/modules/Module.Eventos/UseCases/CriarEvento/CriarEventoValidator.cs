using FluentValidation;
using Module.Eventos.Domain;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.CriarEvento;

internal sealed class CriarEventoValidator : AbstractValidator<CriarEventoRequest>
{
    public CriarEventoValidator()
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
        RuleFor(r => r.Trilhas).Must(x => x is null || x.Count > 0).WithMessage("Informe ao menos uma trilha.");
        RuleForEach(r => r.Trilhas).ChildRules(t =>
        {
            t.RuleFor(x => x.TrilhaNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(120).WithMessage(MensagensValidacao.TamanhoMaximo);
            t.RuleFor(x => x.TrilhaDescricao).MaximumLength(1000).WithMessage(MensagensValidacao.TamanhoMaximo);
            t.RuleFor(x => x.TrilhaCor).Matches("^#[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrWhiteSpace(x.TrilhaCor)).WithMessage("Informe uma cor hexadecimal no formato #RRGGBB.");
        });
        RuleFor(r => r.Trilhas).Must(x => x is null || x.Select(t => t.TrilhaNome.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == x.Count).WithMessage("Os nomes das trilhas não podem se repetir.");
    }
}

/// <summary>Mensagens específicas do módulo Eventos (as genéricas ficam em <see cref="MensagensValidacao"/>).</summary>
internal static class MensagensValidacaoEventos
{
    public const string DataFimPosterior = "O campo {PropertyName} deve ser posterior à data de início.";
    public const string FormatoInvalido = "O campo {PropertyName} deve ser Presencial, Remoto ou Hibrido.";
    public const string LocalObrigatorio = "O campo {PropertyName} é obrigatório para eventos presenciais ou híbridos.";
    public const string LocalNaoPermitido = "O campo {PropertyName} não deve ser informado para eventos remotos.";
    public const string LinkObrigatorio = "O campo {PropertyName} é obrigatório para eventos remotos ou híbridos.";
    public const string SituacaoInvalida = "O campo {PropertyName} deve ser Publicado, EmAndamento, Encerrado ou Cancelado.";
}
