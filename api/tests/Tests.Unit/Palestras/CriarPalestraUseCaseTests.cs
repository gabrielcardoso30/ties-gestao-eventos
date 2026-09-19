using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Module.Palestras.UseCases.CriarPalestra;
using NSubstitute;
using Shared.Contracts.Eventos;
using Shared.Contracts.Locais;
using Shared.Contracts.Pessoas;
using Shared.Http.Results;
using Shouldly;

namespace Tests.Unit.Palestras;

/// <summary>
/// Regras cruzadas do caso de uso com os contratos dos outros módulos substituídos por NSubstitute.
/// O contexto é instanciado sem conexão: todos os cenários falham antes de qualquer acesso ao banco.
/// </summary>
public sealed class CriarPalestraUseCaseTests : IDisposable
{
    private static readonly DateTimeOffset EventoInicio = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EventoFim = EventoInicio.AddDays(2);
    private static readonly Guid LocalId = Guid.NewGuid();

    private readonly IEventosModuleApi _eventos = Substitute.For<IEventosModuleApi>();
    private readonly ILocaisModuleApi _locais = Substitute.For<ILocaisModuleApi>();
    private readonly IPessoasModuleApi _pessoas = Substitute.For<IPessoasModuleApi>();
    private readonly PalestrasDbContext _db = new(new DbContextOptionsBuilder<PalestrasDbContext>()
        .UseNpgsql("Host=localhost;Database=nao_conecta;Username=x;Password=x")
        .Options);

    public CriarPalestraUseCaseTests()
    {
        _eventos.ObterTrilhaResumoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => new TrilhaResumo(call.ArgAt<Guid>(1), call.ArgAt<Guid>(0), "Principal", true));
    }

    private CriarPalestraUseCase CriarUseCase() => new(
        _db,
        new AgendaPalestraVerificador(_eventos, _locais, NullLogger<AgendaPalestraVerificador>.Instance),
        _pessoas,
        NullLogger<CriarPalestraUseCase>.Instance);

    private static CriarPalestraRequest Request(Guid? salaId = null) => new(
        Guid.NewGuid(), Guid.NewGuid(), salaId, "Monolito modular", null, EventoInicio.AddHours(1), EventoInicio.AddHours(2),
        [new CriarPalestraPalestranteRequest(Guid.NewGuid(), PalestrantePapel.Principal)]);

    private static EventoResumo Evento(Guid id, string situacao = "Publicado", Guid? localId = null) =>
        new(id, "Evento Teste", EventoInicio, EventoFim, "Presencial", situacao, localId ?? LocalId);


    [Fact]
    public async Task Evento_inexistente_deve_retornar_422_EventoNaoEncontrado()
    {
        _eventos.ObterEventoResumoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EventoResumo?)null);

        var resultado = await CriarUseCase().HandleAsync(Request(), CancellationToken.None);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(PalestrasErros.EventoNaoEncontrado);
        resultado.Error.Type.ShouldBe(ErrorType.BusinessRule);
    }

    [Theory]
    [InlineData("Encerrado")]
    [InlineData("Cancelado")]
    public async Task Evento_encerrado_ou_cancelado_deve_retornar_422_EventoNaoAceitaPalestras(string situacao)
    {
        var request = Request();
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>()).Returns(Evento(request.EventoId, situacao));

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(PalestrasErros.EventoNaoAceitaPalestras);
        resultado.Error.Code.ShouldBe("Palestras.EventoNaoAceitaPalestras");
        resultado.Error.Type.ShouldBe(ErrorType.BusinessRule);
        await _pessoas.DidNotReceiveWithAnyArgs().ObterPessoasResumoAsync(default!, default);
    }

    [Fact]
    public async Task Periodo_fora_do_evento_deve_retornar_422_PeriodoForaDoEvento()
    {
        var request = Request() with { PalestraInicio = EventoFim.AddMinutes(-30), PalestraFim = EventoFim.AddMinutes(30) };
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>()).Returns(Evento(request.EventoId));

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.Error.ShouldBe(PalestrasErros.PeriodoForaDoEvento);
    }

    [Fact]
    public async Task Sala_inexistente_deve_retornar_422_SalaNaoEncontrada()
    {
        var request = Request(salaId: Guid.NewGuid());
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>()).Returns(Evento(request.EventoId));
        _locais.ObterSalaResumoAsync(request.SalaId!.Value, Arg.Any<CancellationToken>()).Returns((SalaResumo?)null);

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.Error.ShouldBe(PalestrasErros.SalaNaoEncontrada);
    }

    [Fact]
    public async Task Sala_de_outro_local_deve_retornar_422_SalaNaoPertenceAoLocal()
    {
        var request = Request(salaId: Guid.NewGuid());
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>()).Returns(Evento(request.EventoId));
        _locais.ObterSalaResumoAsync(request.SalaId!.Value, Arg.Any<CancellationToken>())
            .Returns(new SalaResumo(request.SalaId.Value, Guid.NewGuid(), "Auditório", 100, "Auditorio"));

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.Error.ShouldBe(PalestrasErros.SalaNaoPertenceAoLocal);
    }

    [Fact]
    public async Task Evento_online_sem_local_com_sala_informada_deve_retornar_SalaNaoPertenceAoLocal()
    {
        var request = Request(salaId: Guid.NewGuid());
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>())
            .Returns(new EventoResumo(request.EventoId, "Online", EventoInicio, EventoFim, "Online", "Publicado", null));
        _locais.ObterSalaResumoAsync(request.SalaId!.Value, Arg.Any<CancellationToken>())
            .Returns(new SalaResumo(request.SalaId.Value, LocalId, "Auditório", 100, "Auditorio"));

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.Error.ShouldBe(PalestrasErros.SalaNaoPertenceAoLocal);
    }

    [Fact]
    public async Task Pessoa_inexistente_deve_retornar_422_PessoaNaoEncontrada()
    {
        var request = Request();
        _eventos.ObterEventoResumoAsync(request.EventoId, Arg.Any<CancellationToken>()).Returns(Evento(request.EventoId));
        _pessoas.ObterPessoasResumoAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        var resultado = await CriarUseCase().HandleAsync(request, CancellationToken.None);

        resultado.Error.ShouldBe(PalestrasErros.PessoaNaoEncontrada);
        await _pessoas.Received(1).ObterPessoasResumoAsync(Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1), Arg.Any<CancellationToken>());
    }

    public void Dispose() => _db.Dispose();
}
