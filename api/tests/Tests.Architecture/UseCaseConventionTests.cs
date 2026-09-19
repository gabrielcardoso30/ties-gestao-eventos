using FluentValidation;
using NetArchTest.Rules;
using Shared.Http.Endpoints;
using Shouldly;
using Xunit;

namespace Tests.Architecture;

public sealed class UseCaseConventionTests
{
    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void UseCases_DevemImplementarContratoDeCasoDeUso(System.Reflection.Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("UseCase")
            .Should()
            .ImplementInterface(typeof(IUseCase<,>))
            .GetResult();

        AssertSuccess(result, "Classes *UseCase devem implementar IUseCase<TRequest, TResponse>");
    }

    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void Endpoints_DevemImplementarContratoDeEndpoint(System.Reflection.Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .ImplementInterface(typeof(IEndpoint))
            .GetResult();

        AssertSuccess(result, "Classes *Endpoint devem implementar IEndpoint");
    }

    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void Validators_DevemHerdarDoFluentValidation(System.Reflection.Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .Should()
            .Inherit(typeof(AbstractValidator<>))
            .GetResult();

        AssertSuccess(result, "Classes *Validator devem herdar de AbstractValidator<T>");
    }

    [Theory]
    [MemberData(nameof(ArchitectureTestData.Modules), MemberType = typeof(ArchitectureTestData))]
    public void UseCases_DevemEstarNoNamespaceUseCases(System.Reflection.Assembly moduleAssembly)
    {
        var moduleNamespace = moduleAssembly.GetName().Name!;
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("UseCase")
            .Or()
            .HaveNameEndingWith("Endpoint")
            .Or()
            .HaveNameEndingWith("Validator")
            .Should()
            .ResideInNamespaceStartingWith($"{moduleNamespace}.UseCases")
            .GetResult();

        AssertSuccess(result, "UseCase, Endpoint e Validator devem permanecer em UseCases/<CasoDeUso>");
    }

    private static void AssertSuccess(TestResult result, string rule) =>
        result.IsSuccessful.ShouldBeTrue(
            $"{rule}. Tipos em violação: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
