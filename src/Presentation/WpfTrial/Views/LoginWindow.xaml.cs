using System.Windows;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// ログインウィンドウ（WPF + MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ViewModel（LoginViewModel）を DataContext に設定</description></item>
/// <item><description>ログインID・パスワード入力欄、エラーメッセージは XAML の Binding のみで表現</description></item>
/// <item><description>PasswordBox は仕様上バインド不可のため Behaviors/PasswordBoxAssistant で仲介</description></item>
/// <item><description>ViewModel の LoginSucceeded / CancelRequested イベントを受けて DialogResult を確定しウィンドウを閉じる（ウィンドウのライフサイクル制御は View の責務であり ViewModel はダイアログの存在を知らない）</description></item>
/// </list>
/// </remarks>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    /// <summary>
    /// <see cref="LoginWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">DataContext に設定する ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public LoginWindow(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;
        _viewModel.CancelRequested += ViewModel_CancelRequested;
    }

    private void ViewModel_LoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ViewModel_CancelRequested(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
