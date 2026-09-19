using Shared.Http.Results;
using Shouldly;
using Xunit;

namespace Tests.Unit.Shared;

public sealed class ResultTests
{
    [Fact]
    public void Sucesso_deve_expor_valor_e_nao_ter_erro()
    {
        Result<int> resultado = 42;
        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.ShouldBe(42);
        resultado.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Falha_deve_expor_erro_e_lancar_ao_acessar_valor()
    {
        Result<int> resultado = Error.NotFound("Teste.NaoEncontrado", "não achei");
        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
        Should.Throw<InvalidOperationException>(() => resultado.Value);
    }

    [Fact]
    public void Match_deve_escolher_o_ramo_correto()
    {
        Result<string> ok = "x";
        Result<string> erro = Error.Conflict("Teste.Conflito", "conflito");
        ok.Match(v => v, e => e.Code).ShouldBe("x");
        erro.Match(v => v, e => e.Code).ShouldBe("Teste.Conflito");
    }
}
