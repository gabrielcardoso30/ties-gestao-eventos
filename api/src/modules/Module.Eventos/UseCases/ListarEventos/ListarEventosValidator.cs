using FluentValidation;
using Module.Eventos.UseCases.CriarEvento;
using Shared.Contracts.Common;

namespace Module.Eventos.UseCases.ListarEventos;

internal sealed class ListarEventosValidator : AbstractValidator<ListarEventosRequest>
{
    public ListarEventosValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.Busca).MaximumLength(100);
        RuleFor(r => r.OrdenarPor).Must(c => new[] { "eventoNome", "eventoDataInicio", "eventoFormato", "eventoSituacao", "inscricoesConfirmadas" }.Contains(c!, StringComparer.OrdinalIgnoreCase)).When(r => !string.IsNullOrWhiteSpace(r.OrdenarPor));
        RuleFor(r => r.EventoSituacao).IsInEnum().When(r => r.EventoSituacao.HasValue);
        RuleFor(r => r.EventoFormato).IsInEnum().When(r => r.EventoFormato.HasValue).WithMessage(MensagensValidacaoEventos.FormatoInvalido);
        RuleFor(r => r.DataInicioAte).GreaterThanOrEqualTo(r => r.DataInicioDe!.Value)
            .When(r => r.DataInicioDe.HasValue && r.DataInicioAte.HasValue)
            .WithMessage("O campo {PropertyName} deve ser igual ou posterior a DataInicioDe.");
    }
}
