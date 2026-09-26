using SupportAdvance.Presentation.WpfTrial.ViewModels;
using Syncfusion.UI.Xaml.NavigationDrawer;
using Syncfusion.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// メインウィンドウ（WPF + MVVM Toolkit）
///
/// 【責務】
/// - ViewModel（MainWindowViewModel）を DataContext に設定する
/// - ナビゲーション項目選択時に ViewModel メソッドを呼び出す
///
/// 【設計方針】
/// - MVVM Toolkit の ObservableProperty / RelayCommand を活用し、コードビハインドを最小化
/// - タブ管理（OpenTabs、SelectedTab）は ViewModel で一元管理
/// - UI ロジックはバインディングで実装し、イベントハンドラは最小限に
///
/// 【UI コンポーネント】
/// - Ribbon：シンプルリボンモード（EnableSimplifiedLayoutMode="True"）
/// - SfNavigationDrawer（ナビゲーション）：Syncfusion モダン系コントロール
///   ナビゲーション項目（NavigationItem）は XAML で SfNavigationDrawer 直下に宣言し、
///   ContentView にはメインコンテンツ（TabControlExt）のみを配置する。
///   NavigationItem.HasItems は XAML 宣言時点では常に false を返すため、
///   子項目の有無判定には Items.Count を使用する（実機検証で確認済み）
///   （詳細は docs/Assistance/Plans/20260926_SfNavigationDrawer再検証計画.md）
/// - TabControlExt（タブペイン）：ItemsSource/SelectedItem をバインディング
/// - RibbonStatusBar：ステータス表示
///
/// 【イベントハンドリング】
/// - NavigationPane.ItemClicked：ナビゲーション項目クリック時
///   → 子項目を持たない葉ノードのみ ViewModel.NavigationItemSelected() を呼び出す
///   → 既存タブを選択 or 新規タブを作成
///
/// 【バインディング】
/// - TabControlExt.ItemsSource ← ViewModel.OpenTabs
/// - TabControlExt.SelectedItem ← ViewModel.SelectedTab
/// - ItemTemplate / ContentTemplate で自動 UI 生成
/// </summary>
public partial class MainWindow : SfChromelessWindow
{
    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">DataContext に設定する ViewModel</param>
    /// <remarks>
    /// DI コンテナから ViewModel が注入され、コンストラクタで DataContext に設定される。
    /// ここでは最小限の初期化のみ実施：
    /// 1. InitializeComponent() で XAML レイアウトを構築
    /// 2. DataContext を ViewModel に設定
    /// 3. NavigationPane.ItemClicked イベントハンドラを登録
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;

        // ナビゲーション項目選択時のイベントハンドラを登録
        // このハンドラのみコードビハインドに残す（UI イベント対応は必須）
        NavigationPane.ItemClicked += NavigationPane_ItemClicked;
    }

    /// <summary>
    /// NavigationPane（SfNavigationDrawer）の項目がクリックされた時の処理
    /// </summary>
    /// <remarks>
    /// 【処理フロー】
    /// 1. DataContext が MainWindowViewModel であることを確認
    /// 2. クリックされた NavigationItem を取得
    /// 3. 子項目を持つ親ノード（"顧客管理" 等の見出し）はタブを開かず展開/折りたたみのみとする
    /// 4. ViewModel.NavigationItemSelected() メソッドを呼び出す
    ///
    /// 【ViewModel 内での処理】
    /// - 既に同じ Header のタブが開いていれば、そのタブを SelectedTab に設定
    /// - 新規の場合は TabItemData を作成、OpenTabs に追加、SelectedTab に設定
    ///
    /// 【なぜ ViewModel メソッドの直接呼び出しか】
    /// - MVVM Toolkit の RelayCommand はコンパイル時生成のため、
    ///   コードビハインドから直接プロパティ参照がコンパイル時に見えない
    /// - イベントハンドラからは ViewModel メソッドの直接呼び出しが
    ///   最もシンプルで確実
    /// - ViewModel メソッドが public であり、ロジックは ViewModel に集約
    /// </remarks>
    private void NavigationPane_ItemClicked(object? sender, NavigationItemClickedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var item = e.Item;
        // NavigationItem.HasItems は XAML 宣言時点では更新されず常に false を返すため、
        // Items.Count で子項目の有無を判定する（実機検証で確認済み）
        if (item == null || item.Items.Count > 0)
        {
            return;
        }

        // ViewModel の NavigationItemSelected メソッドを呼び出す
        viewModel.NavigationItemSelected(item);
    }
}


