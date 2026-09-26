using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Contexts.Authentication.Application.Dtos;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Infrastructure.Services;

namespace SupportAdvance.Presentation.WpfTrial.ViewModels;

/// <summary>
/// ログインウィンドウ ViewModel（MVVM Toolkit）
///
/// 【責務】
/// - ログインID・パスワード入力の状態管理
/// - AuthenticateLocalUserUseCase の実行
/// - エラーメッセージの管理
/// - ローディング状態の管理
/// - ログイン成功時のセッション保存
///
/// 【UI バインディング】
/// - LoginId （ObservableProperty）
/// - Password （ObservableProperty。PasswordBox は Behaviors/PasswordBoxAssistant 経由でバインド）
/// - ErrorMessage （ObservableProperty）
/// - IsLoading / IsNotLoading （ObservableProperty）
/// - LoginCommand / CancelCommand （RelayCommand）
/// </summary>
public partial class LoginWindowViewModel : ObservableObject
{
    private readonly AuthenticateLocalUserUseCase _authenticateUseCase;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppLogging<LoginWindowViewModel> _logger;

    /// <summary>
    /// ログインID の入力値
    /// </summary>
    [ObservableProperty]
    private string loginId = string.Empty;

    /// <summary>
    /// パスワードの入力値（ログイン失敗時にクリア）
    /// </summary>
    [ObservableProperty]
    private string password = string.Empty;

    /// <summary>
    /// 画面に表示するエラーメッセージ。エラーなしの場合は空文字
    /// </summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// ログイン処理中かどうかを示す値
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// ログイン処理中でないかどうかを示す値（入力欄・ボタンの有効／無効のバインド用）
    /// </summary>
    /// <remarks>【注意】<c>IsLoading</c> の変更時に自動で反転。直接の設定は不要</remarks>
    [ObservableProperty]
    private bool isNotLoading = true;

    /// <summary>
    /// ログイン成功イベント（View 側でウィンドウを閉じるために使用）
    /// </summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>
    /// キャンセルイベント（View 側でウィンドウを閉じるために使用）
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// <see cref="LoginWindowViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="authenticateUseCase">ローカル認証のユースケース</param>
    /// <param name="currentUserService">ログイン成功時にユーザー情報を記録する先</param>
    /// <param name="logger">ログの出力先</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public LoginWindowViewModel(
        AuthenticateLocalUserUseCase authenticateUseCase,
        ICurrentUserService currentUserService,
        IAppLogging<LoginWindowViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(authenticateUseCase);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(logger);

        _authenticateUseCase = authenticateUseCase;
        _currentUserService = currentUserService;
        _logger = logger;

        _logger.LogInformation("LoginWindowViewModel initialized.");
    }

    /// <summary>
    /// IsLoading が変更されたときに IsNotLoading を同期
    /// </summary>
    partial void OnIsLoadingChanged(bool oldValue, bool newValue)
    {
        IsNotLoading = !newValue;
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
