using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Contexts.Identity.Application.Dtos;
using SupportAdvance.Contexts.Identity.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Infrastructure.Services;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

/// <summary>
/// ログインダイアログ ViewModel（MVVM Toolkit）
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
/// - Password （ObservableProperty）
/// - ErrorMessage （ObservableProperty）
/// - IsLoading （ObservableProperty）
/// - LoginCommand （RelayCommand）
/// </summary>
public partial class LoginDialogViewModel : ObservableObject
{
    private readonly AuthenticateLocalUserUseCase _authenticateUseCase;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppLogging<LoginDialogViewModel> _logger;

    [ObservableProperty]
    private string loginId = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isNotLoading = true;

    /// <summary>
    /// ログイン成功イベント
    /// </summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>
    /// キャンセルイベント
    /// </summary>
    public event EventHandler? CancelRequested;

    public LoginDialogViewModel(
        AuthenticateLocalUserUseCase authenticateUseCase,
        ICurrentUserService currentUserService,
        IAppLogging<LoginDialogViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(authenticateUseCase);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(logger);

        _authenticateUseCase = authenticateUseCase;
        _currentUserService = currentUserService;
        _logger = logger;

        _logger.LogInformation("LoginDialogViewModel initialized.");
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
            ErrorMessage = $"予期しないエラー: {ex.Message}";
            _logger.LogWarning($"Unexpected error during login: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
