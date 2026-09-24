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
/// </summary>
public partial class MainWindow : SfChromelessWindow
{
    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">DataContext に設定する ViewModel</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;

        // ナビゲーション項目選択時のイベントハンドラを登録
        NavigationPane.SelectedItemChanged += NavigationPane_SelectedItemChanged;
    }

    /// <summary>
    /// NavigationPane（GroupBar）の SelectedItem が変更された時の処理
    /// </summary>
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

