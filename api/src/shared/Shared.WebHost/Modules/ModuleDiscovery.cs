using System.Reflection;

namespace Shared.WebHost.Modules;

/// <summary>Descobre módulos pelos assemblies <c>Module.*.dll</c> presentes ao lado do host.</summary>
public static class ModuleDiscovery
{
    public static IReadOnlyList<IModule> Discover()
    {
        var assemblies = Directory.EnumerateFiles(AppContext.BaseDirectory, "Module.*.dll")
            .Select(caminho => Assembly.Load(AssemblyName.GetAssemblyName(caminho)))
            .Distinct()
            .ToList();

        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IModule).IsAssignableFrom(t))
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();
    }
}
