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

        // フォーム設定
        Text = "ログイン";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(400, 200);

        // ViewModel のログイン成功イベントをハンドル
        _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;

        // ViewModel のキャンセルイベントをハンドル
        _viewModel.CancelRequested += ViewModel_CancelRequested;
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

    private TextBox? _txtLoginId;
    private TextBox? _txtPassword;
    private Button? _btnLogin;
    private Button? _btnCancel;
    private Label? _lblError;

    private void InitializeComponent()
    {
        // ログインID ラベル
        var lblLoginId = new Label
        {
            Text = "ログインID:",
            Location = new Point(20, 20),
            Size = new Size(100, 20),
            AutoSize = true
        };

        // ログインID テキストボックス
        _txtLoginId = new TextBox
        {
            Location = new Point(120, 20),
            Size = new Size(250, 25),
            TabIndex = 0
        };
        // ViewModel の LoginId にバインド
        _txtLoginId.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.LoginId), true,
            DataSourceUpdateMode.OnPropertyChanged);

        // パスワード ラベル
        var lblPassword = new Label
        {
            Text = "パスワード:",
            Location = new Point(20, 60),
            Size = new Size(100, 20),
            AutoSize = true
        };

        // パスワード テキストボックス
        _txtPassword = new TextBox
        {
            Location = new Point(120, 60),
            Size = new Size(250, 25),
            PasswordChar = '*',
            TabIndex = 1
        };
        // ViewModel の Password にバインド
        _txtPassword.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.Password), true,
            DataSourceUpdateMode.OnPropertyChanged);

        // エラーメッセージ ラベル
        _lblError = new Label
        {
            ForeColor = Color.Red,
            Location = new Point(20, 100),
            Size = new Size(350, 40),
            AutoSize = false
        };
        // ViewModel の ErrorMessage にバインド
        _lblError.DataBindings.Add("Text", _viewModel, nameof(LoginDialogViewModel.ErrorMessage));
        _lblError.DataBindings.Add("Visible", _viewModel, nameof(LoginDialogViewModel.ErrorMessage),
            true, DataSourceUpdateMode.Never, "");  // ErrorMessage が空以外の場合のみ表示

        // ログイン ボタン
        _btnLogin = new Button
        {
            Text = "ログイン",
            Location = new Point(120, 145),
            Size = new Size(100, 35),
            TabIndex = 2,
            DialogResult = DialogResult.None,
            Command = _viewModel.LoginCommand
        };
        // IsNotLoading に直接バインド（反転プロパティを使用）
        _btnLogin.DataBindings.Add("Enabled", _viewModel, nameof(LoginDialogViewModel.IsNotLoading),
            true, DataSourceUpdateMode.OnPropertyChanged);

        // キャンセル ボタン
        _btnCancel = new Button
        {
            Text = "キャンセル",
            Location = new Point(230, 145),
            Size = new Size(100, 35),
            TabIndex = 3,
            DialogResult = DialogResult.Cancel,
            Command = _viewModel.CancelCommand
        };

        // フォームに追加
        Controls.Add(lblLoginId);
        Controls.Add(_txtLoginId);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_lblError);
        Controls.Add(_btnLogin);
        Controls.Add(_btnCancel);
    }


}
