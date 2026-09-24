using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WpfTrial.ViewModels;
using Syncfusion.Windows.Controls;                 // SfChromelessWindow / SfTabControl / SfTabItem
using Syncfusion.UI.Xaml.NavigationDrawer;         // SfNavigationDrawer / NavigationItemClickedEventArgs
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

        // NavigationItem の階層はコードビハインドで構築する（XAML でネストした Items を
        // 直接パースすると、テンプレート未適用の状態で CollectionChanged が発火し
        // NullReferenceException になるため）。
        // さらに、NavigationItem 内部の親 SfNavigationDrawer への逆参照はテンプレート適用後
        // （ビジュアルツリー構築後）に設定されるため、コンストラクタ内ではまだ null であり、
        // ここで Items.Add() すると同様に NullReferenceException になる。
        // そのため Loaded イベント（テンプレート適用後）まで構築を遅延する。
        Loaded += MainWindow_Loaded;

        // NavigationPane → タブ追加
        NavigationPane.ItemClicked += NavigationPane_ItemClicked;

        // タブ切替 → Ribbon の ViewModel 切替
        MainTabs.SelectionChanged += MainTabs_SelectionChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        BuildNavigationItems();
    }

    private void BuildNavigationItems()
    {
        // NavigationItem.Items（ObservableCollection）へ Add() すると、内部の
        // CollectionChanged ハンドラが navigationDrawer フィールド（親 SfNavigationDrawer
        // への逆参照）を参照するが、このフィールドはコンテナ生成のタイミング次第で
        // 未設定のままになることがあり、逐次 Add すると NullReferenceException になる。
        // そのため、ツリー全体を Items.Add() を使わずにオフラインで構築し、
        // 最後に ItemsSource として一括で割り当てることで、CollectionChanged の
        // 逐次発火を回避する。
        var customerItem = new NavigationItem
        {
            Header = "顧客管理",
            ItemsSource = new[]
            {
                new NavigationItem { Header = "顧客一覧" },
                new NavigationItem { Header = "顧客登録" },
            }
        };

        var orderItem = new NavigationItem
        {
            Header = "受注管理",
            ItemsSource = new[]
            {
                new NavigationItem { Header = "受注一覧" },
                new NavigationItem { Header = "受注登録" },
            }
        };

        NavigationItemsHost.ItemsSource = new[] { customerItem, orderItem };
    }

    private void NavigationPane_ItemClicked(object sender, NavigationItemClickedEventArgs e)
    {
        var item = e.Item;
        if (item == null)
        {
            return;
        }

        // 今は空の UserControl を載せるだけ
        switch (item.Header?.ToString())
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
        var tab = new SfTabItem
        {
            Header = header,
            Content = content
        };

        MainTabs.Items.Add(tab);
        MainTabs.SelectedItem = tab;
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs.SelectedItem is SfTabItem tab &&
            tab.Content is FrameworkElement fe)
        {
            DataContext = fe.DataContext;
        }
    }
}
