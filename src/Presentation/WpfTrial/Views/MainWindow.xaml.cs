using SupportAdvance.Presentation.Shared.ViewModels;
using Syncfusion.Windows.Tools.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// メインウィンドウ（WPF + MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ViewModel（MainWindowViewModel）の DataContext への設定</description></item>
/// </list>
/// <para>【設計方針】</para>
/// <list type="bullet">
/// <item><description>コードビハインドは DataContext の設定のみ。イベントハンドラなし</description></item>
/// <item><description>タブ管理（OpenTabs、SelectedTab）は ViewModel で一元管理</description></item>
/// <item><description>ナビゲーション項目のクリックは Behaviors/NavigationDrawerBehavior が ViewModel の OpenTabCommand に橋渡しする（子項目を持たない葉ノードのみ。子項目の有無は NavigationItem.Items.Count で判定。詳細は docs/Assistance/Plans/20260926_SfNavigationDrawer再検証計画.md）</description></item>
/// <item><description>タブの内容は、各タブの ContentViewModel の型に対応する DataTemplate（MainWindow.xaml）で View が決まる</description></item>
/// </list>
/// <para>【UI コンポーネント】</para>
/// <list type="bullet">
/// <item><description>Ribbon：シンプルリボンモード（EnableSimplifiedLayoutMode="True"）</description></item>
/// <item><description>SfNavigationDrawer（ナビゲーション）：NavigationItem は XAML で直下に宣言し、ContentView にはメインコンテンツのみを配置</description></item>
/// <item><description>TabControlExt（タブペイン）：ItemsSource／SelectedItem を ViewModel にバインド</description></item>
/// <item><description>RibbonStatusBar：ステータス表示</description></item>
/// </list>
/// </remarks>
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
