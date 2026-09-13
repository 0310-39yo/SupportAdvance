using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Application.Dtos;
using SupportAdvance.Contexts.Identity.Application.Queries;
using SupportAdvance.Contexts.Identity.Application.Services;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Domain.Repositories;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Application.UseCases;

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

        // Step 1: ログインID で認証情報を取得
        var credentials = await _loginCredentialsQuery.GetByLoginIdAsync(request.LoginId);
        if (credentials == null)
        {
            throw new InvalidOperationException(
                $"Authentication failed: LoginId '{request.LoginId}' not found.");
        }

        // Step 2: 認証情報が有効か確認
        if (!credentials.IsActive)
        {
            throw new InvalidOperationException(
                $"Authentication failed: Credentials for LoginId '{request.LoginId}' are inactive.");
        }

        // Step 3: パスワード検証
        if (!_passwordHashService.VerifyPassword(request.Password, credentials.PasswordHash))
        {
            throw new InvalidOperationException(
                $"Authentication failed: Invalid password for LoginId '{request.LoginId}'.");
        }

        // Step 4: UserAuthSession を生成
        var now = _clock.JstNow;
        var authorityRowId = AuthorityRowId.From(credentials.MappingEmployeeRowId);
        var loginCredentialsRowId = LoginCredentialsRowId.From(credentials.RowId);

        // RowId を事前採番
        var sessionRowIdValue = await _sequenceProvider.GetNextValueAsync();
        var sessionRowId = UserAuthSessionRowId.From(sessionRowIdValue);

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
