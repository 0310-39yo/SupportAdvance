using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WpfTrial.ViewModels;
using Syncfusion.Windows.Controls;       // SfChromelessWindow
using Syncfusion.Windows.Tools.Controls; // GroupBar / GroupViewItem / TabControlExt / TabItemExt
using System.Windows;
using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// メインウィンドウ（WPF + MVVM Toolkit）
///
/// 【責務】
/// - ViewModel（MainWindowViewModel）を DataContext に設定する
/// - NavigationPane → タブ追加のイベント配線のみ（UI ロジックは最小限）
/// </summary>
public partial class MainWindow : SfChromelessWindow
{
    private readonly IAppLogging<MainWindow> _logger;

    public MainWindow(MainWindowViewModel viewModel, IAppLogging<MainWindow> logger, IAppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);

        InitializeComponent();

        _logger = logger;
        DataContext = viewModel;

        _logger.LogInformation("MainWindow initialized.");

        // NavigationPane → タブ追加
        NavigationPane.SelectedItemChanged += NavigationPane_SelectedItemChanged;

        // タブ切替 → Ribbon の ViewModel 切替
        MainTabs.SelectionChanged += MainTabs_SelectionChanged;
    }

    private void NavigationPane_SelectedItemChanged(object sender, RoutedEventArgs e)
    {
        var item = NavigationPane.SelectedItem;
        if (item == null)
        {
            return;
        }

        // 今は空の UserControl を載せるだけ
        switch (item.Text)
        {
            case "顧客一覧":
                AddTab("顧客一覧", new EmptyView());
                break;

            case "顧客登録":
                AddTab("顧客登録", new EmptyView());
                break;

            case "受注一覧":
                AddTab("受注一覧", new EmptyView());
                break;

            case "受注登録":
                AddTab("受注登録", new EmptyView());
                break;
        }
    }

    private void AddTab(string header, UserControl content)
    {
        var tab = new TabItemExt
        {
            Header = header,
            Content = content
        };

        MainTabs.Items.Add(tab);
        MainTabs.SelectedItem = tab;
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs.SelectedItem is TabItemExt tab &&
            tab.Content is FrameworkElement fe)
        {
            DataContext = fe.DataContext;
        }
    }
}
