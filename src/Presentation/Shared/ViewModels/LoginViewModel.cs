using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Contexts.Authentication.Application.Dtos;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Application.Abstractions.Services;

namespace SupportAdvance.Presentation.Shared.ViewModels;

/// <summary>
/// ログイン画面 ViewModel（MVVM Toolkit。WpfTrial／WinTrial 共通）
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ログインID・パスワード入力の状態管理</description></item>
/// <item><description>AuthenticateLocalUserUseCase の実行</description></item>
/// <item><description>エラーメッセージの管理</description></item>
/// <item><description>ローディング状態の管理</description></item>
/// <item><description>ログイン成功時のセッション保存</description></item>
/// </list>
/// <para>【UI バインディング】</para>
/// <list type="bullet">
/// <item><description>LoginId （ObservableProperty）</description></item>
/// <item><description>Password （ObservableProperty。WPF の PasswordBox は Behaviors/PasswordBoxAssistant 経由でバインド）</description></item>
/// <item><description>ErrorMessage （ObservableProperty）</description></item>
/// <item><description>IsLoading （ObservableProperty。入力欄・ボタンの有効／無効は InverseBooleanConverter で反転してバインド）</description></item>
/// <item><description>LoginCommand / CancelCommand （RelayCommand）</description></item>
/// </list>
/// </remarks>
public partial class LoginViewModel : ObservableObject
{
    private readonly AuthenticateLocalUserUseCase _authenticateUseCase;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppLogging<LoginViewModel> _logger;

    /// <summary>
    /// ログインID の入力値
    /// </summary>
    [ObservableProperty]
    private string _loginId = string.Empty;

    /// <summary>
    /// パスワードの入力値（ログイン失敗時にクリア）
    /// </summary>
    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>
    /// 画面に表示するエラーメッセージ。エラーなしの場合は空文字
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// ログイン処理中かどうかを示す値
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// ログイン成功イベント（View 側で画面を閉じるために使用）
    /// </summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>
    /// キャンセルイベント（View 側で画面を閉じるために使用）
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// <see cref="LoginViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="authenticateUseCase">ローカル認証のユースケース</param>
    /// <param name="currentUserService">ログイン成功時にユーザー情報を記録する先</param>
    /// <param name="logger">ログの出力先</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public LoginViewModel(
        AuthenticateLocalUserUseCase authenticateUseCase,
        ICurrentUserService currentUserService,
        IAppLogging<LoginViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(authenticateUseCase);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(logger);

        _authenticateUseCase = authenticateUseCase;
        _currentUserService = currentUserService;
        _logger = logger;

        _logger.LogInformation("LoginViewModel initialized.");
    }

    /// <summary>
    /// キャンセル処理（RelayCommand）
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        _logger.LogInformation("Login cancelled by user.");
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ログイン処理（RelayCommand）
    /// </summary>
    [RelayCommand]
    public async Task Login()
    {
        // エラーメッセージをクリア
        ErrorMessage = string.Empty;

        // 入力値の検証
        if (string.IsNullOrWhiteSpace(LoginId) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "ログインIDとパスワードを入力してください";
            return;
        }

        try
        {
            IsLoading = true;

            var request = new AuthenticateLocalUserRequest
            {
                LoginId = LoginId.Trim(),
                Password = Password
            };

            var response = await _authenticateUseCase.ExecuteAsync(request);

            // ログイン成功時の処理
            _currentUserService.SetLoggedInUser(response.EmployeeRowId, response.LoginId);

            _logger.LogInformation($"User '{LoginId}' logged in successfully.");

            // ログイン成功イベントを発火
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Password = string.Empty;  // パスワードをクリア
            _logger.LogWarning($"Login failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            ErrorMessage = "予期しないエラーが発生しました。しばらくしてからやり直してください";
            _logger.LogError("Unexpected error during login", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
