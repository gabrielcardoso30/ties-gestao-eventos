using Shared.Contracts.Common;
using Shouldly;
using Xunit;

namespace Tests.Unit.Shared;

public sealed class PagedRequestTests
{
    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-5, 500, 1, 100)]
    [InlineData(3, 50, 3, 50)]
    public void Deve_normalizar_pagina_e_tamanho(int pagina, int tamanho, int paginaEsperada, int tamanhoEsperado)
    {
        var request = new PagedRequest(pagina, tamanho);
        request.PaginaNormalizada.ShouldBe(paginaEsperada);
        request.TamanhoNormalizado.ShouldBe(tamanhoEsperado);
    }

    [Fact]
    public void Total_de_paginas_deve_arredondar_para_cima()
    {
        new PagedResult<int>([], 1, 20, 41).TotalPaginas.ShouldBe(3);
        new PagedResult<int>([], 1, 20, 0).TotalPaginas.ShouldBe(0);
    }
}
