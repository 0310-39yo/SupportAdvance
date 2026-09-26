using System.Reflection;
using Mono.Cecil;
using NetArchTest.Rules;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// Presentation 層（WpfTrial / WinTrial）の依存ルールを検証
/// </summary>
/// <remarks>
/// <para>【検証内容】Composition Root（WpfTrial の App、WinTrial の Program）以外は Infrastructure に依存しない</para>
/// <para>【検証内容】WpfTrial の ViewModel は View 型・UI コントロール型に依存しない</para>
/// <para>【設計】Types.InNamespace は読み込み済みアセンブリのみが対象のため、アセンブリを明示して検証。対象が空で常に成功する状態を防ぐため、検証対象の存在も確認する</para>
/// </remarks>
public class PresentationArchitectureTests
{
    private static readonly string[] InfrastructureNamespaces =
    [
        "SupportAdvance.Infrastructure",
        "SupportAdvance.Contexts.Employee.Infrastructure",
        "SupportAdvance.Contexts.Department.Infrastructure",
        "SupportAdvance.Contexts.Authentication.Infrastructure"
    ];

    private static readonly string[] UiTypeNamespaces =
    [
        "System.Windows.Controls",
        "System.Windows.Media",
        "System.Windows.Data",
        "System.Windows.Documents",
        "System.Windows.Markup",
        "System.Windows.Threading",
        "System.Windows.Forms",
        "System.Windows.Window",
        "System.Windows.Application",
        "System.Windows.DependencyObject",
        "System.Windows.Visibility",
        "Syncfusion",
        "SupportAdvance.Presentation.WpfTrial.Views"
    ];

    private static Assembly WpfTrialAssembly => typeof(SupportAdvance.Presentation.WpfTrial.DependencyInjection).Assembly;

    private static Assembly WinTrialAssembly => typeof(SupportAdvance.Presentation.WinTrial.DependencyInjection).Assembly;

    /// <summary>
    /// Composition Root（<c>App</c> / <c>Program</c> とそのコンパイラ生成の入れ子型）を検証対象から除くルール
    /// </summary>
    private sealed class NotCompositionRoot : ICustomRule
    {
        public bool MeetsRule(TypeDefinition type)
        {
            var topLevel = type;
            while (topLevel.DeclaringType is not null)
            {
                topLevel = topLevel.DeclaringType;
            }

            return topLevel.Name is not ("App" or "Program");
        }
    }

    private static string Describe(TestResult result)
        => string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>());

    [Fact]
    public void WpfTrial_ExceptCompositionRoot_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(WpfTrialAssembly)
            .That().MeetCustomRule(new NotCompositionRoot())
            .ShouldNot().HaveDependencyOnAny(InfrastructureNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, $"WpfTrial (except App) should not depend on Infrastructure: {Describe(result)}");
    }

    [Fact]
    public void WinTrial_ExceptCompositionRoot_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(WinTrialAssembly)
            .That().MeetCustomRule(new NotCompositionRoot())
            .ShouldNot().HaveDependencyOnAny(InfrastructureNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, $"WinTrial (except Program) should not depend on Infrastructure: {Describe(result)}");
    }

    [Fact]
    public void CompositionRoots_DoDependOnInfrastructure_SoTheRulesAreNotVacuous()
    {
        // 検証ルールが Infrastructure への依存を実際に検出できることの確認
        var wpf = Types.InAssembly(WpfTrialAssembly)
            .That().HaveName("App")
            .Should().HaveDependencyOnAny(InfrastructureNamespaces)
            .GetResult();
        var win = Types.InAssembly(WinTrialAssembly)
            .That().HaveName("Program")
            .Should().HaveDependencyOnAny(InfrastructureNamespaces)
            .GetResult();

        Assert.True(wpf.IsSuccessful, "WpfTrial.App should depend on Infrastructure (Composition Root)");
        Assert.True(win.IsSuccessful, "WinTrial.Program should depend on Infrastructure (Composition Root)");
    }

    [Fact]
    public void WpfTrialViews_DoDependOnUiTypes_SoTheViewModelRuleIsNotVacuous()
    {
        // View（MainWindow）が UI 型に依存していることを検出できるかの確認
        var result = Types.InAssembly(WpfTrialAssembly)
            .That().HaveName("MainWindow")
            .Should().HaveDependencyOnAny(UiTypeNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, "WpfTrial.MainWindow should depend on UI types");
    }

    [Fact]
    public void WpfTrialViewModels_ShouldNotDependOnViewsOrUiControls()
    {
        var viewModels = Types.InAssembly(WpfTrialAssembly)
            .That().ResideInNamespace("SupportAdvance.Presentation.WpfTrial.ViewModels");

        Assert.NotEmpty(viewModels.GetTypes());

        var result = viewModels
            .ShouldNot().HaveDependencyOnAny(UiTypeNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, $"WpfTrial ViewModels should not depend on Views / UI types: {Describe(result)}");
    }
}
