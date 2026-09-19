using Module.Auditoria.Domain;
using Shared.Contracts.Auditoria;
using Shouldly;
using Xunit;

namespace Tests.Unit.Auditoria;

public sealed class RegistroAuditoriaTests
{
    [Fact]
    public void Criar_deve_copiar_todos_os_campos_do_evento_e_usar_o_id_do_evento_como_chave()
    {
        var usuarioId = Guid.NewGuid();
        var evento = new EntidadeAlterada("Pessoas", "Pessoa", "abc", OperacoesAuditoria.Alteracao, "{\"PessoaNome\":\"A\"}", "{\"PessoaNome\":\"B\"}", usuarioId, "Agente", "trace-1");
        var registradoEm = new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

        var registro = RegistroAuditoria.Criar(evento, registradoEm);

        registro.Id.ShouldBe(evento.Id);
        registro.Modulo.ShouldBe("Pessoas");
        registro.EntidadeNome.ShouldBe("Pessoa");
        registro.EntidadeId.ShouldBe("abc");
        registro.Operacao.ShouldBe(OperacoesAuditoria.Alteracao);
        registro.DadosAnteriores.ShouldBe("{\"PessoaNome\":\"A\"}");
        registro.DadosNovos.ShouldBe("{\"PessoaNome\":\"B\"}");
        registro.UsuarioId.ShouldBe(usuarioId);
        registro.UsuarioNome.ShouldBe("Agente");
        registro.TraceId.ShouldBe("trace-1");
        registro.OcorridoEm.ShouldBe(evento.OcorridoEm);
        registro.RegistradoEm.ShouldBe(registradoEm);
    }

    [Fact]
    public void Criar_para_inclusao_deve_manter_dados_anteriores_nulos()
    {
        var evento = new EntidadeAlterada("Locais", "Local", "1", OperacoesAuditoria.Inclusao, null, "{}", null, "sistema", null);

        var registro = RegistroAuditoria.Criar(evento, DateTimeOffset.UtcNow);

        registro.DadosAnteriores.ShouldBeNull();
        registro.UsuarioId.ShouldBeNull();
        registro.TraceId.ShouldBeNull();
    }
}
