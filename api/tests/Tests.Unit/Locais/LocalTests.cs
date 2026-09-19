using Module.Locais.Domain;
using Shared.Contracts.Locais;
using Shouldly;
using Xunit;

namespace Tests.Unit.Locais;

public sealed class LocalTests
{
    private static Local NovoLocal(int? capacidade = 100) =>
        Local.Criar("Auditório", null, null, null, null, "Vila Velha", "es", null, capacidade);

    [Fact]
    public void Local_de_ambiente_unico_nasce_com_uma_sala_e_registra_evento()
    {
        var local = NovoLocal(150);
        local.Salas.Count.ShouldBe(1);
        local.Salas.Single().SalaTipo.ShouldBe(SalaTipo.AmbienteUnico);
        local.Salas.Single().SalaCapacidade.ShouldBe(150);
        local.EnderecoUf.ShouldBe("ES");
        local.Eventos.ShouldHaveSingleItem().ShouldBeOfType<LocalCriado>();
        local.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Nao_deve_permitir_sala_com_nome_duplicado()
    {
        var local = NovoLocal();
        var resultado = local.AdicionarSala("ambiente único", 10, SalaTipo.SalaAula, null);
        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(LocaisErros.SalaNomeDuplicado);
    }

    [Fact]
    public void Nao_deve_remover_a_ultima_sala()
    {
        var local = NovoLocal();
        var resultado = local.RemoverSala(local.Salas.Single().Id);
        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(LocaisErros.LocalPrecisaDeUmaSala);
    }

    [Fact]
    public void Deve_remover_sala_quando_houver_mais_de_uma()
    {
        var local = NovoLocal();
        var sala = local.AdicionarSala("Lab", 20, SalaTipo.Laboratorio, "projetor").Value;
        local.RemoverSala(sala.Id).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Sala_inexistente_deve_retornar_nao_encontrada()
    {
        var local = NovoLocal();
        local.AtualizarSala(Guid.NewGuid(), "X", 1, SalaTipo.Outro, null).Error.ShouldBe(LocaisErros.SalaNaoEncontrada);
    }
}
