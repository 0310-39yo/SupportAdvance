using Moq;
using SupportAdvance.Application.Queries;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Presentation.Shared.ViewModels.Tabs;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Tests.Presentation.Shared.Tests.ViewModels;

/// <summary>
/// <see cref="MainWindowViewModel"/> のタブ管理の検証
/// </summary>
public class MainWindowViewModelTests
{
    private static Form1ViewModel CreateForm1ViewModel()
    {
        var useCase = new GetEmployeeByBizIdIntegrationUseCase(
            Mock.Of<IEmployeeQueryService>(),
            Mock.Of<IAppLogging<GetEmployeeByBizIdIntegrationUseCase>>());

        return new Form1ViewModel(
            Mock.Of<IAppLogging<Form1ViewModel>>(),
            Mock.Of<IClock>(),
            useCase,
            new BusinessDayClockViewModel());
    }

    private static MainWindowViewModel CreateSut(Form1ViewModel? form1ViewModel = null)
    {
        var settings = new Mock<IAppSettings>();
        settings.SetupGet(s => s.ApplicationBuildType).Returns("Debug");
        return new MainWindowViewModel(
            Mock.Of<IAppLogging<MainWindowViewModel>>(),
            settings.Object,
            form1ViewModel ?? CreateForm1ViewModel());
    }

    [Fact]
    public void OpenTabCommand_CustomerList_ShowsForm1ViewModelAsContent()
    {
        var form1 = CreateForm1ViewModel();
        var sut = CreateSut(form1);

        sut.OpenTabCommand.Execute(MainWindowViewModel.CustomerListMenuName);

        Assert.Same(form1, sut.SelectedTab!.ContentViewModel);
    }

    [Fact]
    public void OpenTabCommand_OtherMenu_ShowsEmptyPlaceholderAsContent()
    {
        var sut = CreateSut();

        sut.OpenTabCommand.Execute("受注一覧");

        Assert.IsType<EmptyTabContentViewModel>(sut.SelectedTab!.ContentViewModel);
    }

    [Fact]
    public void OpenTabCommand_NewMenuName_AddsTabAndSelectsIt()
    {
        var sut = CreateSut();

        sut.OpenTabCommand.Execute("顧客一覧");

        var tab = Assert.Single(sut.OpenTabs);
        Assert.Equal("顧客一覧", tab.Header);
        Assert.Same(tab, sut.SelectedTab);
    }

    [Fact]
    public void OpenTabCommand_ExistingMenuName_SelectsExistingTabWithoutAdding()
    {
        var sut = CreateSut();
        sut.OpenTabCommand.Execute("顧客一覧");
        var first = sut.SelectedTab;
        sut.OpenTabCommand.Execute("受注一覧");

        sut.OpenTabCommand.Execute("顧客一覧");

        Assert.Equal(2, sut.OpenTabs.Count);
        Assert.Same(first, sut.SelectedTab);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OpenTabCommand_NullOrEmptyName_DoesNothing(string? menuName)
    {
        var sut = CreateSut();

        sut.OpenTabCommand.Execute(menuName);

        Assert.Empty(sut.OpenTabs);
        Assert.Null(sut.SelectedTab);
    }

    [Fact]
    public void SelectedTab_WhenChanged_RaisesPropertyChanged()
    {
        var sut = CreateSut();
        var raised = new List<string?>();
        sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        sut.OpenTabCommand.Execute("顧客一覧");

        Assert.Contains(nameof(MainWindowViewModel.SelectedTab), raised);
    }

    [Fact]
    public void CloseTabCommand_SelectedTab_RemovesItAndSelectsLastRemainingTab()
    {
        var sut = CreateSut();
        sut.OpenTabCommand.Execute("顧客一覧");
        var first = sut.SelectedTab!;
        sut.OpenTabCommand.Execute("受注一覧");
        var second = sut.SelectedTab!;

        sut.CloseTabCommand.Execute(second);

        Assert.Single(sut.OpenTabs);
        Assert.Same(first, sut.SelectedTab);
    }

    [Fact]
    public void CloseTabCommand_LastTab_ClearsSelection()
    {
        var sut = CreateSut();
        sut.OpenTabCommand.Execute("顧客一覧");

        sut.CloseTabCommand.Execute(sut.SelectedTab);

        Assert.Empty(sut.OpenTabs);
        Assert.Null(sut.SelectedTab);
    }

    [Fact]
    public void CloseTabCommand_NonSelectedTab_KeepsSelection()
    {
        var sut = CreateSut();
        sut.OpenTabCommand.Execute("顧客一覧");
        var first = sut.SelectedTab!;
        sut.OpenTabCommand.Execute("受注一覧");
        var second = sut.SelectedTab!;

        sut.CloseTabCommand.Execute(first);

        Assert.Same(second, sut.SelectedTab);
        Assert.Single(sut.OpenTabs);
    }

    [Fact]
    public void CloseTabCommand_NullOrUnknownTab_DoesNothing()
    {
        var sut = CreateSut();
        sut.OpenTabCommand.Execute("顧客一覧");
        var selected = sut.SelectedTab;

        sut.CloseTabCommand.Execute(null);
        sut.CloseTabCommand.Execute(new SupportAdvance.Presentation.Shared.ViewModels.Tabs.TabItemViewModel("別", new object()));

        Assert.Single(sut.OpenTabs);
        Assert.Same(selected, sut.SelectedTab);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var form1 = CreateForm1ViewModel();
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null!, Mock.Of<IAppSettings>(), form1));
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(Mock.Of<IAppLogging<MainWindowViewModel>>(), null!, form1));
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(Mock.Of<IAppLogging<MainWindowViewModel>>(), Mock.Of<IAppSettings>(), null!));
    }
}
