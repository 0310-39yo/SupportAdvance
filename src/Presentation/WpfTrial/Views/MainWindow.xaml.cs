using SupportAdvance.Presentation.WpfTrial.ViewModels;
using Syncfusion.Windows.Controls;
using System.Windows;

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
/// - GroupBar（ナビゲーション）：Syncfusion Tools 系コントロール
///   ※ SfNavigationDrawer（Modern）は動的 NavigationItem 追加時に
///      NullReferenceException が発生する既知の問題があるため不採用
/// - TabControlExt（タブペイン）：ItemsSource/SelectedItem をバインディング
/// - RibbonStatusBar：ステータス表示
/// 
/// 【イベントハンドリング】
/// - NavigationPane.SelectedItemChanged：ナビゲーション項目選択時
///   → ViewModel.NavigationItemSelected() を呼び出す
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
    /// 3. NavigationPane.SelectedItemChanged イベントハンドラを登録
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;

        // ナビゲーション項目選択時のイベントハンドラを登録
        // このハンドラのみコードビハインドに残す（UI イベント対応は必須）
        NavigationPane.SelectedItemChanged += NavigationPane_SelectedItemChanged;
    }

    /// <summary>
    /// NavigationPane（GroupBar）の SelectedItem が変更された時の処理
    /// </summary>
    /// <remarks>
    /// 【処理フロー】
    /// 1. DataContext が MainWindowViewModel であることを確認
    /// 2. NavigationPane.SelectedItem（GroupViewItem）を取得
    /// 3. ViewModel.NavigationItemSelected() メソッドを呼び出す
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
    private void NavigationPane_SelectedItemChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var selectedItem = NavigationPane.SelectedItem;
        if (selectedItem == null)
        {
            return;
        }

        // ViewModel の NavigationItemSelected メソッドを呼び出す
        viewModel.NavigationItemSelected(selectedItem);
    }
}


