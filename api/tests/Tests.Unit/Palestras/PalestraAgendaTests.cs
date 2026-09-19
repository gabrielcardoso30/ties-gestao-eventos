using Module.Palestras.Domain;
using Shouldly;

namespace Tests.Unit.Palestras;

public class PalestraAgendaTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    public static TheoryData<int, int, int, int, bool> Casos => new()
    {
        // (inicio, fim, outroInicio, outroFim) em minutos a partir de T0 → sobrepõe?
        { 0, 60, 30, 90, true },      // parcial no fim
        { 30, 90, 0, 60, true },      // parcial no início
        { 0, 120, 30, 60, true },     // contém
        { 30, 60, 0, 120, true },     // contido
        { 0, 60, 0, 60, true },       // idêntico
        { 0, 60, 60, 120, false },    // encosta no fim (limite não conta)
        { 60, 120, 0, 60, false },    // encosta no início
        { 0, 60, 90, 120, false },    // disjunto depois
        { 90, 120, 0, 60, false },    // disjunto antes
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void Sobrepoe_deve_detectar_intersecao_de_periodos(int inicio, int fim, int outroInicio, int outroFim, bool esperado)
    {
        PalestraAgenda.Sobrepoe(T0.AddMinutes(inicio), T0.AddMinutes(fim), T0.AddMinutes(outroInicio), T0.AddMinutes(outroFim)).ShouldBe(esperado);
    }

    [Fact]
    public void OcupaSala_deve_considerar_sala_periodo_e_ignorar_a_propria_palestra()
    {
        var sala = Guid.NewGuid();
        var existente = Palestra.Criar(Guid.NewGuid(), Guid.NewGuid(), sala, "Existente", null, T0, T0.AddMinutes(60), [new NovoPalestrante(Guid.NewGuid(), PalestrantePapel.Principal)]).Value;

        PalestraAgenda.OcupaSala(sala, T0.AddMinutes(30), T0.AddMinutes(90)).Compile()(existente).ShouldBeTrue();
        PalestraAgenda.OcupaSala(sala, T0.AddMinutes(60), T0.AddMinutes(90)).Compile()(existente).ShouldBeFalse();
        PalestraAgenda.OcupaSala(Guid.NewGuid(), T0.AddMinutes(30), T0.AddMinutes(90)).Compile()(existente).ShouldBeFalse();
        PalestraAgenda.OcupaSala(sala, T0.AddMinutes(30), T0.AddMinutes(90), palestraIdIgnorada: existente.Id).Compile()(existente).ShouldBeFalse();
    }
}
