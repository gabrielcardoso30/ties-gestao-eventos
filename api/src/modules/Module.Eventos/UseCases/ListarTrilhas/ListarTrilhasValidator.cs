using FluentValidation;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.ListarTrilhas;
internal sealed class ListarTrilhasValidator : AbstractValidator<ListarTrilhasRequest> { public ListarTrilhasValidator() => RuleFor(x => x.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio); }
