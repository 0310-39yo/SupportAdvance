using SupportAdvance.Presentation.WinTrial.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// ログインダイアログ（WinForms + MVVM Toolkit）
///
/// 【責務】
/// - ログインID・パスワード入力フォーム
/// - ViewModel（LoginDialogViewModel）とデータバインディング
/// - UI イベントハンドリング
///
/// 【UI パターン】
/// - テキストボックス: LoginId（従業員番号） ← ViewModel にバインド
/// - パスワードボックス: Password ← ViewModel にバインド
/// - ボタン: ログイン（LoginCommand）、キャンセル
/// - ステータスラベル: エラーメッセージ（ErrorMessage）← ViewModel にバインド
///
/// 【ライフサイクル】
/// - アプリケーション起動時に表示（モーダル）
/// - ログイン成功時（LoginSucceeded イベント）に DialogResult.OK で閉じる
/// - キャンセル時に DialogResult.Cancel で閉じる
/// </summary>
public partial class LoginDialog : Form
{
    private readonly LoginDialogViewModel _viewModel;

    /// <summary>
    /// <see cref="LoginDialog"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">バインドする ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public LoginDialog(LoginDialogViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;

        InitializeComponent();

        // DataContext を ViewModel に設定
        DataContext = _viewModel;

        // ViewModel のログイン成功イベントをハンドル
        _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;

        // ViewModel のキャンセルイベントをハンドル
        _viewModel.CancelRequested += ViewModel_CancelRequested;

        // ログインID テキストボックスにバインド
        txtLoginId.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.LoginId), true,
            DataSourceUpdateMode.OnPropertyChanged);

        // パスワード テキストボックスにバインド
        txtPassword.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.Password), true,
            DataSourceUpdateMode.OnPropertyChanged);

        // エラーメッセージ ラベルにバインド
        lblError.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.ErrorMessage));
        lblError.DataBindings.Add("Visible", _viewModel, nameof(LoginDialogViewModel.ErrorMessage),
            true, DataSourceUpdateMode.Never, "");

        // ログイン ボタンにバインド
        btnLogin.Command = _viewModel.LoginCommand;
        btnLogin.DataBindings.Add("Enabled", _viewModel, nameof(LoginDialogViewModel.IsNotLoading),
            true, DataSourceUpdateMode.OnPropertyChanged);

        // キャンセル ボタンにバインド
        btnCancel.Command = _viewModel.CancelCommand;
    }

    /// <summary>
    /// ログイン成功時のイベントハンドラ
    /// </summary>
    private void ViewModel_LoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>
    /// キャンセル時のイベントハンドラ
    /// </summary>
    private void ViewModel_CancelRequested(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
