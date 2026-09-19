using Shared.Contracts.Palestras;
using Shared.Data.Entidades;
using Shared.Http.Results;

namespace Module.Palestras.Domain;

/// <summary>Palestrante informado na criação da palestra.</summary>
public sealed record NovoPalestrante(Guid PessoaId, PalestrantePapel PalestrantePapel);

/// <summary>
/// Agregado Palestra: sessão de um evento (módulo Eventos), opcionalmente alocada em uma sala (módulo Locais),
/// com palestrantes (Pessoas), conteúdos, presenças e certificados.
/// </summary>
public sealed class Palestra : EntidadeBase
{
    private readonly List<PalestraPalestrante> _palestrantes = [];
    private readonly List<PalestraConteudo> _conteudos = [];
    private readonly List<Presenca> _presencas = [];
    private readonly List<Certificado> _certificados = [];

    private Palestra()
    {
    }

    public Guid EventoId { get; private set; }
    public Guid? SalaId { get; private set; }
    public string PalestraTitulo { get; private set; } = string.Empty;
    public string? PalestraDescricao { get; private set; }
    public DateTimeOffset PalestraInicio { get; private set; }
    public DateTimeOffset PalestraFim { get; private set; }

    /// <summary>Calculado, não persistido.</summary>
    public int PalestraCargaHorariaMinutos => (int)(PalestraFim - PalestraInicio).TotalMinutes;

    public IReadOnlyCollection<PalestraPalestrante> Palestrantes => _palestrantes.AsReadOnly();
    public IReadOnlyCollection<PalestraConteudo> Conteudos => _conteudos.AsReadOnly();
    public IReadOnlyCollection<Presenca> Presencas => _presencas.AsReadOnly();
    public IReadOnlyCollection<Certificado> Certificados => _certificados.AsReadOnly();

    /// <summary>Cria a palestra com ao menos um palestrante; pessoas repetidas resultam em <see cref="PalestrasErros.PalestranteJaVinculado"/>.</summary>
    public static Result<Palestra> Criar(
        Guid eventoId,
        Guid? salaId,
        string palestraTitulo,
        string? palestraDescricao,
        DateTimeOffset palestraInicio,
        DateTimeOffset palestraFim,
        IReadOnlyCollection<NovoPalestrante> palestrantes)
    {
        if (palestrantes.Count == 0)
        {
            return PalestrasErros.PalestraPrecisaDePalestrante;
        }

        var palestra = new Palestra { EventoId = eventoId };
        palestra.Atualizar(salaId, palestraTitulo, palestraDescricao, palestraInicio, palestraFim);

        foreach (var palestrante in palestrantes)
        {
            var resultado = palestra.AdicionarPalestrante(palestrante.PessoaId, palestrante.PalestrantePapel);
            if (resultado.IsFailure)
            {
                return resultado.Error;
            }
        }

        palestra.RegistrarEvento(new PalestraCriada(palestra.Id, palestra.EventoId, palestra.PalestraTitulo));
        return palestra;
    }

    public void Atualizar(Guid? salaId, string palestraTitulo, string? palestraDescricao, DateTimeOffset palestraInicio, DateTimeOffset palestraFim)
    {
        SalaId = salaId;
        PalestraTitulo = palestraTitulo.Trim();
        PalestraDescricao = palestraDescricao?.Trim();
        PalestraInicio = palestraInicio;
        PalestraFim = palestraFim;
    }

    public Result<PalestraPalestrante> AdicionarPalestrante(Guid pessoaId, PalestrantePapel palestrantePapel)
    {
        if (_palestrantes.Any(p => p.ExcluidoEm is null && p.PessoaId == pessoaId))
        {
            return PalestrasErros.PalestranteJaVinculado;
        }

        var palestrante = new PalestraPalestrante(Id, pessoaId, palestrantePapel);
        _palestrantes.Add(palestrante);
        return palestrante;
    }

    /// <summary>Retorna o vínculo a ser removido (soft delete feito pelo contexto). Uma palestra nunca fica sem palestrante.</summary>
    public Result<PalestraPalestrante> RemoverPalestrante(Guid pessoaId)
    {
        var palestrante = _palestrantes.FirstOrDefault(p => p.ExcluidoEm is null && p.PessoaId == pessoaId);
        if (palestrante is null)
        {
            return PalestrasErros.PalestranteNaoEncontrado;
        }

        if (_palestrantes.Count(p => p.ExcluidoEm is null) <= 1)
        {
            return PalestrasErros.PalestraPrecisaDePalestrante;
        }

        return palestrante;
    }

    public PalestraConteudo AdicionarConteudo(string conteudoTitulo, ConteudoTipo conteudoTipo, string conteudoUrl, string? conteudoDescricao)
    {
        var conteudo = new PalestraConteudo(Id, conteudoTitulo, conteudoTipo, conteudoUrl, conteudoDescricao);
        _conteudos.Add(conteudo);
        return conteudo;
    }

    /// <summary>Retorna o conteúdo a ser removido (soft delete feito pelo contexto).</summary>
    public Result<PalestraConteudo> RemoverConteudo(Guid conteudoId)
    {
        var conteudo = _conteudos.FirstOrDefault(c => c.ExcluidoEm is null && c.Id == conteudoId);
        return conteudo is null ? PalestrasErros.ConteudoNaoEncontrado : conteudo;
    }

    /// <summary>Registra a presença (única por pessoa). A inscrição confirmada no evento é verificada pelo caso de uso via contrato.</summary>
    public Result<Presenca> RegistrarPresenca(Guid pessoaId, DateTimeOffset agora)
    {
        if (_presencas.Any(p => p.ExcluidoEm is null && p.PessoaId == pessoaId))
        {
            return PalestrasErros.PresencaJaRegistrada;
        }

        var presenca = new Presenca(Id, pessoaId, agora);
        _presencas.Add(presenca);
        RegistrarEvento(new PresencaRegistrada(Id, EventoId, pessoaId));
        return presenca;
    }

    /// <summary>
    /// Emite o certificado da pessoa: exige presença registrada e palestra encerrada (<c>PalestraFim &lt;= agora</c>).
    /// Idempotente: se já existir, devolve o existente com <c>Criado = false</c>.
    /// </summary>
    public Result<EmissaoCertificado> EmitirCertificado(Guid pessoaId, DateTimeOffset agora)
    {
        var existente = _certificados.FirstOrDefault(c => c.ExcluidoEm is null && c.PessoaId == pessoaId);
        if (existente is not null)
        {
            return new EmissaoCertificado(existente, Criado: false);
        }

        if (!_presencas.Any(p => p.ExcluidoEm is null && p.PessoaId == pessoaId))
        {
            return PalestrasErros.PresencaNaoRegistrada;
        }

        if (PalestraFim > agora)
        {
            return PalestrasErros.PalestraNaoEncerrada;
        }

        var certificado = new Certificado(Id, pessoaId, CertificadoCodigoGerador.Gerar(), agora, PalestraCargaHorariaMinutos);
        _certificados.Add(certificado);
        RegistrarEvento(new CertificadoEmitido(certificado.Id, Id, pessoaId, certificado.CertificadoCodigo));
        return new EmissaoCertificado(certificado, Criado: true);
    }
}

/// <summary>Resultado da emissão: o certificado e se foi criado nesta chamada (201) ou já existia (200).</summary>
public sealed record EmissaoCertificado(Certificado Certificado, bool Criado);
