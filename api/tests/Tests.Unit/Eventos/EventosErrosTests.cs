using Module.Eventos.Domain;
using Shared.Http.Results;
using Shouldly;
using Xunit;

namespace Tests.Unit.Eventos;

public sealed class EventosErrosTests
{
    [Theory]
    [InlineData("EventoNaoEncontrado", ErrorType.NotFound)]
    [InlineData("LocalNaoEncontrado", ErrorType.BusinessRule)]
    [InlineData("FormatoInconsistente", ErrorType.BusinessRule)]
    [InlineData("EventoSemPalestras", ErrorType.BusinessRule)]
    [InlineData("MotivoCancelamentoObrigatorio", ErrorType.BusinessRule)]
    [InlineData("TransicaoSituacaoInvalida", ErrorType.BusinessRule)]
    [InlineData("EventoNaoPodeSerAlterado", ErrorType.BusinessRule)]
    [InlineData("EventoNaoPodeSerExcluido", ErrorType.BusinessRule)]
    [InlineData("EventoNaoAceitaInscricoes", ErrorType.BusinessRule)]
    [InlineData("PessoaNaoEncontrada", ErrorType.BusinessRule)]
    [InlineData("PessoaJaInscrita", ErrorType.Conflict)]
    [InlineData("CapacidadeEsgotada", ErrorType.BusinessRule)]
    [InlineData("InscricaoNaoEncontrada", ErrorType.NotFound)]
    [InlineData("InscricaoJaCancelada", ErrorType.BusinessRule)]
    public void Codigos_e_tipos_devem_seguir_a_especificacao(string motivo, ErrorType tipo)
    {
        var campo = typeof(EventosErros).GetField(motivo);
        campo.ShouldNotBeNull();
        var erro = (Error)campo.GetValue(null)!;

        erro.Code.ShouldBe($"Eventos.{motivo}");
        erro.Type.ShouldBe(tipo);
    }
}
