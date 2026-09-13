using SupportAdvance.Contexts.Identity.Application.Dtos;
using SupportAdvance.Contexts.Identity.Application.UseCases;
using SupportAdvance.Infrastructure.Services;
using System.Windows.Forms;

namespace SupportAdvance.Presentation.WinTrial.Forms;

/// <summary>
/// ログインダイアログ（WinForms）
///
/// 【責務】
/// - ログインID・パスワード入力フォーム
/// - AuthenticateLocalUserUseCase を実行
/// - 認証成功時にセッション情報を保存
///
/// 【UI パターン】
/// - テキストボックス: LoginId（従業員番号）
/// - パスワードボックス: Password
/// - ボタン: ログイン、キャンセル
/// - ステータスラベル: エラーメッセージ表示
///
/// 【ライフサイクル】
/// - アプリケーション起動時に表示（モーダル）
/// - ログイン成功時に DialogResult.OK で閉じる
/// - キャンセル時に DialogResult.Cancel で閉じる
/// </summary>
public partial class LoginDialog : Form
{
    private readonly AuthenticateLocalUserUseCase _authenticateUseCase;
    private readonly ICurrentUserService _currentUserService;

    public LoginDialog(
        AuthenticateLocalUserUseCase authenticateUseCase,
        ICurrentUserService currentUserService)
    {
        InitializeComponent();
        _authenticateUseCase = authenticateUseCase ?? throw new ArgumentNullException(nameof(authenticateUseCase));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        // フォーム設定
        Text = "ログイン";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(400, 200);
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

        // エラーメッセージ ラベル
        _lblError = new Label
        {
            ForeColor = Color.Red,
            Location = new Point(20, 100),
            Size = new Size(350, 40),
            AutoSize = false,
            Visible = false
        };

        // ログイン ボタン
        _btnLogin = new Button
        {
            Text = "ログイン",
            Location = new Point(120, 145),
            Size = new Size(100, 35),
            TabIndex = 2,
            DialogResult = DialogResult.None
        };
        _btnLogin.Click += BtnLogin_Click;

        // キャンセル ボタン
        _btnCancel = new Button
        {
            Text = "キャンセル",
            Location = new Point(230, 145),
            Size = new Size(100, 35),
            TabIndex = 3,
            DialogResult = DialogResult.Cancel
        };
        _btnCancel.Click += BtnCancel_Click;

        // フォームに追加
        Controls.Add(lblLoginId);
        Controls.Add(_txtLoginId);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_lblError);
        Controls.Add(_btnLogin);
        Controls.Add(_btnCancel);
    }

    /// <summary>
    /// ログイン処理（ボタンクリックイベント）
    /// </summary>
    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (_lblError == null || _txtLoginId == null || _txtPassword == null)
            return;

        _lblError.Visible = false;
        _lblError.Text = string.Empty;

        var loginId = _txtLoginId.Text?.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(password))
        {
            _lblError.Text = "ログインIDとパスワードを入力してください";
            _lblError.Visible = true;
            return;
        }

        try
        {
            _btnLogin!.Enabled = false;

            var request = new AuthenticateLocalUserRequest
            {
                LoginId = loginId,
                Password = password
            };

            var response = await _authenticateUseCase.ExecuteAsync(request);

            // ログイン成功時の処理
            _currentUserService.SetLoggedInUser(response.EmployeeRowId, response.LoginId);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (InvalidOperationException ex)
        {
            _lblError.Text = ex.Message;
            _lblError.Visible = true;
            _txtPassword.Clear();
        }
        catch (Exception ex)
        {
            _lblError.Text = $"予期しないエラー: {ex.Message}";
            _lblError.Visible = true;
        }
        finally
        {
            if (_btnLogin != null)
                _btnLogin.Enabled = true;
        }
    }

    /// <summary>
    /// キャンセル処理（ボタンクリックイベント）
    /// </summary>
    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
