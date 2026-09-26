using Moq;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WpfTrial.ViewModels;

namespace SupportAdvance.Tests.Presentation.WpfTrial.Tests.ViewModels;

/// <summary>
/// <see cref="MainWindowViewModel"/> のタブ管理の検証
/// </summary>
public class MainWindowViewModelTests
{
    private static MainWindowViewModel CreateSut()
    {
        var settings = new Mock<IAppSettings>();
        settings.SetupGet(s => s.ApplicationBuildType).Returns("Debug");
        return new MainWindowViewModel(Mock.Of<IAppLogging<MainWindowViewModel>>(), settings.Object);
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
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null!, Mock.Of<IAppSettings>()));
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(Mock.Of<IAppLogging<MainWindowViewModel>>(), null!));
    }
}
