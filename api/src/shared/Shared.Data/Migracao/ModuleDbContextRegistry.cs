namespace Shared.Data.Migracao;

/// <summary>Registro dos DbContexts de módulo, usado pelo migrador na inicialização e por diagnósticos.</summary>
public sealed class ModuleDbContextRegistry
{
    private readonly List<(Type ContextType, string Schema)> _contexts = [];
    public IReadOnlyList<(Type ContextType, string Schema)> Contexts => _contexts;
    public void Add(Type contextType, string schema) => _contexts.Add((contextType, schema));
}
