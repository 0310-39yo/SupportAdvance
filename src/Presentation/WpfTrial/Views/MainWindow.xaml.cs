using SupportAdvance.Presentation.Shared.ViewModels;
using Syncfusion.Windows.Tools.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// メインウィンドウ（WPF + MVVM Toolkit）
///
/// 【責務】
/// - ViewModel（MainWindowViewModel）を DataContext に設定する
///
/// 【設計方針】
/// - コードビハインドは DataContext の設定のみ。イベントハンドラは持たない
/// - タブ管理（OpenTabs、SelectedTab）は ViewModel で一元管理
/// - ナビゲーション項目のクリックは Behaviors/NavigationDrawerBehavior が ViewModel の OpenTabCommand に橋渡しする
///   （子項目を持たない葉ノードのみ。子項目の有無は NavigationItem.Items.Count で判定。
///   詳細は docs/Assistance/Plans/20260926_SfNavigationDrawer再検証計画.md）
/// - タブの内容は、各タブの ContentViewModel の型に対応する DataTemplate（MainWindow.xaml）で View が決まる
///
/// 【UI コンポーネント】
/// - Ribbon：シンプルリボンモード（EnableSimplifiedLayoutMode="True"）
/// - SfNavigationDrawer（ナビゲーション）：NavigationItem は XAML で直下に宣言し、ContentView にはメインコンテンツのみを配置
/// - TabControlExt（タブペイン）：ItemsSource／SelectedItem を ViewModel にバインド
/// - RibbonStatusBar：ステータス表示
/// </summary>
public partial class MainWindow : RibbonWindow
{
    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">DataContext に設定する ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}
