using FluentValidation;
using Shared.Contracts.Common;

namespace Module.Eventos.UseCases.ListarInscricoes;

internal sealed class ListarInscricoesValidator : AbstractValidator<ListarInscricoesRequest>
{
    public ListarInscricoesValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.InscricaoSituacao).IsInEnum().When(r => r.InscricaoSituacao.HasValue);
    }
}
