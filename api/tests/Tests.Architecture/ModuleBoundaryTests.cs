using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace Tests.Architecture;

public sealed class ModuleBoundaryTests
{
    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void Modulo_NaoDeveDependerDeOutroModulo(Assembly moduleAssembly)
    {
        var ownNamespace = moduleAssembly.GetName().Name!;
        var forbiddenNamespaces = ArchitectureTestData.ModuleAssemblies
            .Select(assembly => assembly.GetName().Name!)
            .Where(moduleNamespace => moduleNamespace != ownNamespace)
            .ToArray();

        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        AssertSuccess(result, $"{ownNamespace} não pode referenciar diretamente outro Module.*; use Shared.Contracts");
    }

    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void Domain_NaoDeveDependerDeUseCasesOuSharedDoModulo(Assembly moduleAssembly)
    {
        var moduleNamespace = moduleAssembly.GetName().Name!;
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceStartingWith($"{moduleNamespace}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny($"{moduleNamespace}.UseCases", $"{moduleNamespace}.Shared")
            .GetResult();

        AssertSuccess(result, $"O Domain de {moduleNamespace} deve permanecer independente das camadas externas");
    }

    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void Modulo_NaoDeveDeclararRepositorio(Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .AreClasses()
            .Should()
            .NotHaveNameMatching(".*Repository.*|.*Repositorio.*")
            .GetResult();

        AssertSuccess(result, "O acesso a dados deve usar o DbContext diretamente, sem repositórios");
    }

    private static void AssertSuccess(TestResult result, string rule) =>
        result.IsSuccessful.ShouldBeTrue(
            $"{rule}. Tipos em violação: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
