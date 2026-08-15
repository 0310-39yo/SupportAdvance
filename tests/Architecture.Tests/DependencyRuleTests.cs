using NetArchTest.Rules;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// アーキテクチャ依存関係ルールの検証テスト
///
/// CLEAN_ARCHITECTURE_GUIDELINES.md で定義された依存関係が遵守されていることを確認
/// </summary>
public class DependencyRuleTests
{
    /// <summary>
    /// テスト1: Domain層が Application / Infrastructure / Presentation に依存していないか
    ///
    /// Domain層は最も内側のレイヤーであり、外側の層への依存があってはならない
    /// </summary>
    [Fact]
    public void Domain_Should_Not_DependOn_ApplicationOrInfrastructure()
    {
        var result = Types
            .InAssembly(typeof(SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee.BizDivision).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Application",
                "SupportAdvance.Contexts.Employee.Application",
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Contexts.Employee.Infrastructure",
                "SupportAdvance.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// テスト2: Common層が他のすべての層に依存していないか
    ///
    /// Common層は最も内側のレイヤーで、依存ゼロを維持しなければならない
    /// </summary>
    [Fact]
    public void Common_Should_Have_No_Dependencies()
    {
        var result = Types
            .InAssembly(typeof(SupportAdvance.Common.Clocks.IClock).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.SharedKernel",
                "SupportAdvance.Crosscutting",
                "SupportAdvance.Domain",
                "SupportAdvance.Application",
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Presentation",
                "SupportAdvance.Contexts")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// テスト3: Crosscutting層が Infrastructure に依存していないか
    ///
    /// Crosscutting は NLog などの NuGet パッケージを直接参照し、
    /// Infrastructure（技術詳細）に依存してはならない
    /// </summary>
    [Fact]
    public void Crosscutting_Should_Not_DependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(SupportAdvance.Crosscutting.Logging.IAppLogging<>).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// テスト4: Application層が逆方向依存していないか
    ///
    /// Application層が Infrastructure / Presentation に依存しない（相互参照ルール）
    /// </summary>
    [Fact]
    public void Application_Should_Not_Have_Circular_Dependencies()
    {
        var result = Types
            .InAssembly(typeof(SupportAdvance.Application.UseCases.IUseCase<,>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Presentation",
                "SupportAdvance.Contexts")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// テスト5: Application層が Infrastructure / Presentation に依存していないか
    ///
    /// Application層は呼び出し側（Presentation）に依存してはならず、
    /// Infrastructure への依存は DI により逆転されるべき
    /// </summary>
    [Fact]
    public void Application_Should_Not_Depend_On_InfrastructureOrPresentation()
    {
        // 汎用 Application 層の検証
        var genericAppResult = Types
            .InAssembly(typeof(SupportAdvance.Application.UseCases.IUseCase<,>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Presentation")
            .GetResult();

        Assert.True(genericAppResult.IsSuccessful,
            $"Generic Application: {string.Join(", ", genericAppResult.FailingTypeNames ?? [])}");

        // Context別 Application 層（CarPreferences.Application）の検証
        var contextAppResult = Types
            .InAssembly(typeof(SupportAdvance.Contexts.Employee.Domain.Entities.Employee).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Presentation")
            .GetResult();

        Assert.True(contextAppResult.IsSuccessful,
            $"CarPreferences Application: {string.Join(", ", contextAppResult.FailingTypeNames ?? [])}");
    }

    /// <summary>
    /// テスト6: Infrastructure層が Presentation に依存していないか
    ///
    /// Infrastructure は最も外側の実装レイヤーで、UI層に依存してはならない
    /// </summary>
    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Presentation()
    {
        // 汎用 Infrastructure
        var genericInfraResult = Types
            .InAssembly(typeof(SupportAdvance.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Presentation")
            .GetResult();

        Assert.True(genericInfraResult.IsSuccessful,
            $"Generic Infrastructure: {string.Join(", ", genericInfraResult.FailingTypeNames ?? [])}");

        // Context別 Infrastructure
        var contextInfraResult = Types
            .InAssembly(typeof(SupportAdvance.Contexts.Employee.Infrastructure.Mappers.EmployeeMapper).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Presentation")
            .GetResult();

        Assert.True(contextInfraResult.IsSuccessful,
            $"CarPreferences Infrastructure: {string.Join(", ", contextInfraResult.FailingTypeNames ?? [])}");
    }

    /// <summary>
    /// テスト7: 汎用 Application層が Context別層に依存していないか
    ///
    /// 汎用層はインターフェース定義のみで、Context別層の具体実装を参照してはならない
    /// </summary>
    [Fact]
    public void GenericApplication_Should_Not_Depend_On_ContextSpecificLayers()
    {
        var result = Types
            .InAssembly(typeof(SupportAdvance.Application.UseCases.IUseCase<,>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Contexts",
                "SupportAdvance.Contexts.Employee.Application",
                "SupportAdvance.Contexts.Employee.Domain",
                "SupportAdvance.Contexts.Employee.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// テスト8: SharedKernel層が Domain に依存していないか
    ///
    /// SharedKernel は基盤型のみを定義し、Context固有の Domain に依存してはならない
    /// </summary>
    /// <summary>
    /// テスト8: SharedKernel層が Domain に依存していないか
    ///
    /// SharedKernel は基盤型のみを定義し、Context固有の Domain に依存してはならない
    /// </summary>
    [Fact]
    public void SharedKernel_Should_Not_Depend_On_Domain()
    {
        // SharedKernel アセンブリを直接参照
        var assembly = typeof(SupportAdvance.SharedKernel.Entities.IDomainEvent).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Contexts")
            .GetResult();

        Assert.True(result.IsSuccessful,
            string.Join(", ", result.FailingTypeNames ?? []));
    }
}
