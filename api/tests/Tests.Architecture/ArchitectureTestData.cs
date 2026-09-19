using System.Reflection;
using Module.Auditoria.Shared;
using Module.Eventos.Shared;
using Module.Identidade.Shared;
using Module.Locais.Shared;
using Module.Palestras.Shared;
using Module.Pessoas.Shared;

namespace Tests.Architecture;

public static class ArchitectureTestData
{
    public static readonly Assembly[] ModuleAssemblies =
    [
        typeof(AuditoriaModule).Assembly,
        typeof(EventosModule).Assembly,
        typeof(IdentidadeModule).Assembly,
        typeof(LocaisModule).Assembly,
        typeof(PalestrasModule).Assembly,
        typeof(PessoasModule).Assembly
    ];

    public static IEnumerable<object[]> Modules =>
        ModuleAssemblies.Select(assembly => new object[] { assembly });
}
