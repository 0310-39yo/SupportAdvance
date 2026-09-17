using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Dtos;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Contexts.Authentication.Application.Services;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.Repositories;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Application.UseCases;

/// <summary>
/// ローカル認証 Use Case
///
/// 【責務】
/// - ログインID・パスワードでユーザーを認証
/// - UserAuthSession を生成してセッションを確立
///
/// 【フロー】
/// 1. ILoginCredentialsQuery でログインID から認証情報を取得
/// 2. IPasswordHashService でパスワード検証
/// 3. UserAuthSession を生成
/// 4. Repository で保存
///
/// 【設計上の注意】
/// - Employee BC への参照は行わない（BC間独立）
/// - 権限確認は Presentation層で別途実施
/// - このUseCase は認証のみに専念
///
/// 【認証失敗パターン】
/// - ログインID が見つからない → InvalidOperationException
/// - パスワード不一致 → InvalidOperationException
/// - 認証情報が無効（is_active=0）→ InvalidOperationException
/// </summary>
public sealed class AuthenticateLocalUserUseCase
{
    private readonly ILoginCredentialsQuery _loginCredentialsQuery;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUserAuthSessionRepository _sessionRepository;
    private readonly IClock _clock;
    private readonly ISequenceProvider _sequenceProvider;

    /// <summary>
    /// システムユーザー RowId（ログイン失敗時に使用）
    /// 【用途】失敗ログの current_user_row_id として記録
    /// 【重要】値の実体は WellKnownIds（Common）で一元管理。Infrastructure の
    /// SystemCurrentUserService と同じ値を参照するため、ここでは再定義せず委譲する
    /// </summary>
    private const long SystemUserId = WellKnownIds.SystemUserEmployeeRowId;

    public AuthenticateLocalUserUseCase(
        ILoginCredentialsQuery loginCredentialsQuery,
        IPasswordHashService passwordHashService,
        IUserAuthSessionRepository sessionRepository,
        IClock clock,
        ISequenceProvider sequenceProvider)
    {
        _loginCredentialsQuery = loginCredentialsQuery ?? throw new ArgumentNullException(nameof(loginCredentialsQuery));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
    }

    /// <summary>
    /// ローカル認証を実行
    /// </summary>
    /// <param name="request">認証リクエスト</param>
    /// <returns>認証レスポンス</returns>
    /// <exception cref="ArgumentException">入力値が無効</exception>
    /// <exception cref="InvalidOperationException">認証失敗</exception>
    public async Task<AuthenticateLocalUserResponse> ExecuteAsync(AuthenticateLocalUserRequest request)
    {
        ValidateRequest(request);
        var now = _clock.JstNow;
        var sessionRowIdValue = await _sequenceProvider.GetNextValueAsync();
        var sessionRowId = UserAuthSessionRowId.From(sessionRowIdValue);

        // Step 1: ログインID で認証情報を取得
        var credentials = await _loginCredentialsQuery.GetByLoginIdAsync(request.LoginId);
        if (credentials == null)
        {
            // 失敗ログを記録
            var failureSession = UserAuthSession.Create(
                id: sessionRowId,
                AuthorityRowId.From(SystemUserId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: null);
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("ログインIDが見つかりません");
        }

        // Step 2: 認証情報が有効か確認
        if (!credentials.IsActive)
        {
            // 失敗ログを記録
            var failureSession = UserAuthSession.Create(
                id: sessionRowId,
                AuthorityRowId.From(SystemUserId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: LoginCredentialsRowId.From(credentials.RowId));
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("このアカウントは無効です");
        }

        // Step 3: パスワード検証
        if (!_passwordHashService.VerifyPassword(request.Password, credentials.PasswordHash))
        {
            // 失敗ログを記録
            var failureSession = UserAuthSession.Create(
                id: sessionRowId,
                AuthorityRowId.From(SystemUserId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: LoginCredentialsRowId.From(credentials.RowId));
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("パスワードが間違っています");
        }

        // Step 4: 成功時の UserAuthSession を生成
        var authorityRowId = AuthorityRowId.From(credentials.MappingEmployeeRowId);
        var loginCredentialsRowId = LoginCredentialsRowId.From(credentials.RowId);

        var session = UserAuthSession.Create(
            id: sessionRowId,
            authorityRowId,
            isAdAuthenticated: false, // ローカル認証
            loginSuccess: true,
            loggedInAt: now,
            loginCredentialsRowId);

        // Step 5: Repository で保存
        await _sessionRepository.SaveAsync(session);

        // レスポンス作成
        return new AuthenticateLocalUserResponse
        {
            UserAuthSessionRowId = sessionRowId.Value,
            EmployeeRowId = credentials.MappingEmployeeRowId,
            LoginId = credentials.LoginId,
            LoggedInAt = now.Value
        };
    }

    private static void ValidateRequest(AuthenticateLocalUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LoginId))
            throw new ArgumentException("LoginId is required", nameof(request.LoginId));

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request.Password));

        if (request.LoginId.Length > 50)
            throw new ArgumentException("LoginId must be 50 characters or less", nameof(request.LoginId));
    }
}
