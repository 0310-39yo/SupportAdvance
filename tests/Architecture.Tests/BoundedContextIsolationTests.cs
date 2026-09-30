using NetArchTest.Rules;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// Bounded Context 間の依存関係を検証
/// BC は独立し、相互に依存しない
/// </summary>
public class BoundedContextIsolationTests
{
    [Fact]
    public void CarPreferences_ShouldNotDependOnAuthentication()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Samples.CarPreferences")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Authentication");

        Assert.True(rule.GetResult().IsSuccessful,
            $"CarPreferences should not depend on Authentication: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void CarPreferences_ShouldNotDependOnEmployee()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Samples.CarPreferences")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Employee");

        Assert.True(rule.GetResult().IsSuccessful,
            $"CarPreferences should not depend on Employee: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Authentication_ShouldNotDependOnCarPreferences()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Authentication")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Samples.CarPreferences");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Authentication should not depend on CarPreferences: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Authentication_ShouldNotDependOnEmployee()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Authentication")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Employee");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Authentication should not depend on Employee: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Employee_ShouldNotDependOnCarPreferences()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Employee")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Samples.CarPreferences");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Employee should not depend on CarPreferences: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Employee_ShouldNotDependOnAuthentication()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Employee")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Authentication");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Employee should not depend on Authentication: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Theory]
    [InlineData("SupportAdvance.Contexts.Employee")]
    [InlineData("SupportAdvance.Contexts.Department")]
    [InlineData("SupportAdvance.Contexts.Authentication")]
    public void IntegrationPrototype_ShouldNotDependOnOtherContexts(string otherContext)
    {
        // Context 間の連携は SharedKernel の型（IEmployee など）と汎用 Application 層の Query Service 経由のみ
        var assembly = typeof(GetEmployeeByBizIdIntegrationUseCase).Assembly;
        Assert.NotEmpty(Types.InAssembly(assembly).GetTypes());

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(otherContext)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"IntegrationPrototype should not depend on {otherContext}: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }
}
