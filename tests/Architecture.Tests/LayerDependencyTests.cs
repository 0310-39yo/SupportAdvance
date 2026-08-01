using NetArchTest.Rules;
using Xunit;

namespace Architecture.Tests;

/// <summary>
/// Clean Architecture の層間依存ルールを検証
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var types = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Domain")
            .GetTypes();

        var rule = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Domain")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.*.Application");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Domain should not depend on Application: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Domain")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.*.Infrastructure");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Domain should not depend on Infrastructure: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_ShouldNotDependOnPresentation()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Domain")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Presentation");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Domain should not depend on Presentation: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureImplementations()
    {
        // Application can depend on Infrastructure interfaces, but not implementations
        var rule = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Application")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure.Repositories")
            .And()
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure.Mappers");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Application should not depend on Infrastructure implementations: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotDependOnPresentation()
    {
        var rule = Types.InNamespace("SupportAdvance.Contexts")
            .That()
            .ResideInNamespace("*.Application")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Presentation");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Application should not depend on Presentation: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Crosscutting_ShouldNotDependOnInfrastructure()
    {
        var rule = Types.InNamespace("SupportAdvance.Crosscutting")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Crosscutting should not depend on Infrastructure: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Presentation_ShouldNotDependOnDomain()
    {
        var rule = Types.InNamespace("SupportAdvance.Presentation")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts.*.Domain");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Presentation should not depend on Domain: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Presentation_ShouldNotDependOnInfrastructureImplementations()
    {
        // Presentation can depend on Infrastructure in Program.cs, but not elsewhere
        var rule = Types.InNamespace("SupportAdvance.Presentation")
            .That()
            .DoNotHaveName("Program")
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure.Repositories")
            .And()
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure.Mappers");

        Assert.True(rule.GetResult().IsSuccessful,
            $"Presentation (except Program) should not depend on Infrastructure implementations: {string.Join(", ", rule.GetResult().FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }
}
