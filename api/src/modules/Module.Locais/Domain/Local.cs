using Shared.Contracts.Locais;
using Shared.Data.Entidades;
using Shared.Http.Results;

namespace Module.Locais.Domain;

/// <summary>
/// Agregado Local: onde eventos presenciais acontecem. Possui uma ou mais salas; um local de ambiente único
/// nasce com uma sala do tipo <see cref="SalaTipo.AmbienteUnico"/>.
/// </summary>
public sealed class Local : EntidadeBase
{
    private readonly List<Sala> _salas = [];

    private Local()
    {
    }

    public string LocalNome { get; private set; } = string.Empty;
    public string? LocalDescricao { get; private set; }
    public string? EnderecoLogradouro { get; private set; }
    public string? EnderecoNumero { get; private set; }
    public string? EnderecoBairro { get; private set; }
    public string EnderecoCidade { get; private set; } = string.Empty;
    public string EnderecoUf { get; private set; } = string.Empty;
    public string? EnderecoCep { get; private set; }

    public IReadOnlyCollection<Sala> Salas => _salas.AsReadOnly();

    public static Local Criar(
        string localNome,
        string? localDescricao,
        string? enderecoLogradouro,
        string? enderecoNumero,
        string? enderecoBairro,
        string enderecoCidade,
        string enderecoUf,
        string? enderecoCep,
        int? capacidadeAmbienteUnico)
    {
        var local = new Local();
        local.Atualizar(localNome, localDescricao, enderecoLogradouro, enderecoNumero, enderecoBairro, enderecoCidade, enderecoUf, enderecoCep);

        // Regra: local com apenas um ambiente é traduzido como uma sala.
        if (capacidadeAmbienteUnico is > 0)
        {
            local._salas.Add(new Sala(local.Id, "Ambiente único", capacidadeAmbienteUnico.Value, SalaTipo.AmbienteUnico, null));
        }

        local.RegistrarEvento(new LocalCriado(local.Id, local.LocalNome));
        return local;
    }

    public void Atualizar(
        string localNome,
        string? localDescricao,
        string? enderecoLogradouro,
        string? enderecoNumero,
        string? enderecoBairro,
        string enderecoCidade,
        string enderecoUf,
        string? enderecoCep)
    {
        LocalNome = localNome.Trim();
        LocalDescricao = localDescricao?.Trim();
        EnderecoLogradouro = enderecoLogradouro?.Trim();
        EnderecoNumero = enderecoNumero?.Trim();
        EnderecoBairro = enderecoBairro?.Trim();
        EnderecoCidade = enderecoCidade.Trim();
        EnderecoUf = enderecoUf.Trim().ToUpperInvariant();
        EnderecoCep = enderecoCep?.Trim();
    }

    public Result<Sala> AdicionarSala(string salaNome, int salaCapacidade, SalaTipo salaTipo, string? salaRecursos)
    {
        if (_salas.Any(s => s.ExcluidoEm is null && string.Equals(s.SalaNome, salaNome.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return LocaisErros.SalaNomeDuplicado;
        }

        var sala = new Sala(Id, salaNome, salaCapacidade, salaTipo, salaRecursos);
        _salas.Add(sala);
        return sala;
    }

    public Result AtualizarSala(Guid salaId, string salaNome, int salaCapacidade, SalaTipo salaTipo, string? salaRecursos)
    {
        var sala = _salas.FirstOrDefault(s => s.Id == salaId && s.ExcluidoEm is null);
        if (sala is null)
        {
            return LocaisErros.SalaNaoEncontrada;
        }

        if (_salas.Any(s => s.Id != salaId && s.ExcluidoEm is null && string.Equals(s.SalaNome, salaNome.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return LocaisErros.SalaNomeDuplicado;
        }

        sala.Atualizar(salaNome, salaCapacidade, salaTipo, salaRecursos);
        return Result.Success();
    }

    /// <summary>Retorna a sala a ser removida (soft delete feito pelo contexto). Um local nunca fica sem salas.</summary>
    public Result<Sala> RemoverSala(Guid salaId)
    {
        var sala = _salas.FirstOrDefault(s => s.Id == salaId && s.ExcluidoEm is null);
        if (sala is null)
        {
            return LocaisErros.SalaNaoEncontrada;
        }

        if (_salas.Count(s => s.ExcluidoEm is null) <= 1)
        {
            return LocaisErros.LocalPrecisaDeUmaSala;
        }

        return sala;
    }

    public void MarcarExcluido() => RegistrarEvento(new LocalExcluido(Id));
}
