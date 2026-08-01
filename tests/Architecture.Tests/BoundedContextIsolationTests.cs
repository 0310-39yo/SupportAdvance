using NetArchTest.Rules;
using Xunit;

namespace Architecture.Tests;

/// <summary>
/// Bounded Context 間の依存関係を検証
/// BC は独立し、相互に依存しない
/// </summary>
public class BoundedContextIsolationTests
{
    [Fact]
    public void CarPreferences_ShouldNotDependOnIdentity()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Samples.CarPreferences")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Auth.Identity");

        Assert.True(rule.GetResult().IsSuccessful,
            $"CarPreferences should not depend on Identity: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void CarPreferences_ShouldNotDependOnEmployee()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Samples.CarPreferences")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Master.Employee");

        Assert.True(rule.GetResult().IsSuccessful,
            $"CarPreferences should not depend on Employee: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Identity_ShouldNotDependOnCarPreferences()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Auth.Identity")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Samples.CarPreferences");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Identity should not depend on CarPreferences: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Identity_ShouldNotDependOnEmployee()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Auth.Identity")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Master.Employee");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Identity should not depend on Employee: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Employee_ShouldNotDependOnCarPreferences()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Master.Employee")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Samples.CarPreferences");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Employee should not depend on CarPreferences: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Employee_ShouldNotDependOnIdentity()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts.Master.Employee")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.Auth.Identity");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Employee should not depend on Identity: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }
}
