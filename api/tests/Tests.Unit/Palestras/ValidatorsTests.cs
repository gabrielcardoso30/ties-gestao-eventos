using FluentValidation.TestHelper;
using Module.Palestras.Domain;
using Module.Palestras.UseCases.AdicionarConteudo;
using Module.Palestras.UseCases.AtualizarPalestra;
using Module.Palestras.UseCases.CriarPalestra;
using Module.Palestras.UseCases.ListarPalestras;
using Shouldly;

namespace Tests.Unit.Palestras;

public class ValidatorsTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    private static CriarPalestraRequest CriarValido() => new(
        Guid.NewGuid(), Guid.NewGuid(), null, "Título", null, Inicio, Inicio.AddHours(1),
        [new CriarPalestraPalestranteRequest(Guid.NewGuid(), PalestrantePapel.Principal)]);

    [Fact]
    public void CriarPalestra_valido_deve_passar()
    {
        new CriarPalestraValidator().TestValidate(CriarValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CriarPalestra_fim_antes_do_inicio_deve_falhar()
    {
        var request = CriarValido() with { PalestraFim = Inicio.AddMinutes(-1) };

        new CriarPalestraValidator().TestValidate(request).ShouldHaveValidationErrorFor(r => r.PalestraFim);
    }

    [Fact]
    public void CriarPalestra_sem_palestrantes_deve_falhar()
    {
        var request = CriarValido() with { Palestrantes = [] };

        new CriarPalestraValidator().TestValidate(request).ShouldHaveValidationErrorFor(r => r.Palestrantes);
    }

    [Fact]
    public void CriarPalestra_com_pessoa_vazia_ou_papel_invalido_deve_falhar()
    {
        var request = CriarValido() with { Palestrantes = [new CriarPalestraPalestranteRequest(Guid.Empty, (PalestrantePapel)99)] };

        var resultado = new CriarPalestraValidator().TestValidate(request);

        resultado.ShouldHaveValidationErrorFor("Palestrantes[0].PessoaId");
        resultado.ShouldHaveValidationErrorFor("Palestrantes[0].PalestrantePapel");
    }

    [Fact]
    public void CriarPalestra_titulo_vazio_evento_vazio_e_sala_vazia_devem_falhar()
    {
        var request = CriarValido() with { EventoId = Guid.Empty, SalaId = Guid.Empty, PalestraTitulo = " ", PalestraDescricao = new string('x', 4001) };

        var resultado = new CriarPalestraValidator().TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.EventoId);
        resultado.ShouldHaveValidationErrorFor(r => r.SalaId);
        resultado.ShouldHaveValidationErrorFor(r => r.PalestraTitulo);
        resultado.ShouldHaveValidationErrorFor(r => r.PalestraDescricao);
    }

    [Fact]
    public void AtualizarPalestra_exige_id_da_rota_e_periodo_valido()
    {
        var request = new AtualizarPalestraRequest(Guid.NewGuid(), null, "Título", null, Inicio, Inicio);

        var resultado = new AtualizarPalestraValidator().TestValidate(request);

        resultado.ShouldHaveValidationErrorFor(r => r.PalestraId);
        resultado.ShouldHaveValidationErrorFor(r => r.PalestraFim);
    }

    [Fact]
    public void AdicionarConteudo_exige_url_absoluta_e_tipo_valido()
    {
        var invalido = new AdicionarConteudoRequest("Slides", (ConteudoTipo)42, "nao-e-url", null) { PalestraId = Guid.NewGuid() };
        var valido = new AdicionarConteudoRequest("Slides", ConteudoTipo.Slides, "https://exemplo.com/a.pdf", null) { PalestraId = Guid.NewGuid() };

        var resultado = new AdicionarConteudoValidator().TestValidate(invalido);
        resultado.ShouldHaveValidationErrorFor(r => r.ConteudoUrl);
        resultado.ShouldHaveValidationErrorFor(r => r.ConteudoTipo);
        new AdicionarConteudoValidator().TestValidate(valido).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ListarPalestras_limita_paginacao()
    {
        var resultado = new ListarPalestrasValidator().TestValidate(new ListarPalestrasRequest(null, null, 0, 101));

        resultado.ShouldHaveValidationErrorFor(r => r.Pagina);
        resultado.ShouldHaveValidationErrorFor(r => r.TamanhoPagina);
        resultado.Errors.Count.ShouldBe(2);
    }
}
