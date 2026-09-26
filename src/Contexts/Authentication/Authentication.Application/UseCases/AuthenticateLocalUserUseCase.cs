using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Dtos;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.Services;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Application.UseCases;

/// <summary>
/// ローカル認証 Use Case
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ログインID・パスワードでユーザーを認証</description></item>
/// <item><description>UserAuthSession を生成してセッションを確立</description></item>
/// </list>
/// <para>【フロー】</para>
/// <list type="bullet">
/// <item><description>ILoginCredentialsQuery でログインID から認証情報を取得</description></item>
/// <item><description>IPasswordHashService でパスワード検証</description></item>
/// <item><description>UserAuthSession を生成</description></item>
/// <item><description>Repository で保存</description></item>
/// </list>
/// <para>【設計上の注意】</para>
/// <list type="bullet">
/// <item><description>Employee BC への参照は行わない（BC間独立）</description></item>
/// <item><description>権限確認は Presentation層で別途実施</description></item>
/// <item><description>このUseCase は認証のみに専念</description></item>
/// </list>
/// <para>【認証失敗パターン】</para>
/// <list type="bullet">
/// <item><description>ログインID が見つからない → InvalidOperationException</description></item>
/// <item><description>パスワード不一致 → InvalidOperationException</description></item>
/// <item><description>認証情報が無効（is_active=0）→ InvalidOperationException</description></item>
/// </list>
/// </remarks>
public sealed class AuthenticateLocalUserUseCase
{
    private readonly ILoginCredentialsQuery _loginCredentialsQuery;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUserAuthSessionRepository _sessionRepository;
    private readonly IClock _clock;
    private readonly ISequenceProvider _sequenceProvider;

    /// <summary>
    /// 正体不明ユーザー RowId（ログインID自体が存在しない場合にのみ使用）
    /// </summary>
    /// <remarks>
    /// <para>【用途】失敗ログの current_user_row_id として記録</para>
    /// <para>【重要】ログインID不一致時は credentials が取得できず対象の従業員が特定できないため、このフォールバック値を使う。credentials が取得できている失敗（アカウント無効・パスワード不一致）では、狙われた実在アカウントの credentials.MappingEmployeeRowId を使う</para>
    /// <para>【重要】システム自身が行った自動処理を表す SystemUserEmployeeRowId とは意味が別。こちらは「実在するが特定できない人物によるログイン試行」を表すため UnknownUserEmployeeRowId を使用</para>
    /// <para>【重要】値の実体は WellKnownIds（Common）で一元管理</para>
    /// </remarks>
    private const long UnknownUserId = WellKnownIds.UnknownUserEmployeeRowId;

    /// <summary>
    /// <see cref="AuthenticateLocalUserUseCase"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="loginCredentialsQuery">ログインID から認証情報を取得する問い合わせサービス</param>
    /// <param name="passwordHashService">パスワードの照合を行うサービス</param>
    /// <param name="sessionRepository">認証セッション（成功・失敗とも）の保存先</param>
    /// <param name="clock">ログイン日時（JST）の取得元</param>
    /// <param name="sequenceProvider">認証セッションの行ID の採番元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
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
                AuthorityRowId.From(UnknownUserId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: UsedLoginCredentialsRowId.Unset());
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("ログインIDが見つかりません");
        }

        // Step 2: 認証情報が有効か確認
        if (!credentials.IsActive)
        {
            // 失敗ログを記録
            // 【重要】credentials は取得済みのため、狙われたアカウントの MappingEmployeeRowId を記録する
            // （SystemUserId ではなく実在の従業員を記録することで、不正/誤ログイン試行の追跡に使える）
            var failureSession = UserAuthSession.Create(
                id: sessionRowId,
                AuthorityRowId.From(credentials.MappingEmployeeRowId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: UsedLoginCredentialsRowId.From(credentials.RowId));
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("このアカウントは無効です");
        }

        // Step 3: パスワード検証
        if (!_passwordHashService.VerifyPassword(request.Password, credentials.PasswordHash))
        {
            // 失敗ログを記録
            // 【重要】credentials は取得済みのため、狙われたアカウントの MappingEmployeeRowId を記録する
            var failureSession = UserAuthSession.Create(
                id: sessionRowId,
                AuthorityRowId.From(credentials.MappingEmployeeRowId),
                isAdAuthenticated: false,
                loginSuccess: false,
                loggedInAt: now,
                loginCredentialsRowId: UsedLoginCredentialsRowId.From(credentials.RowId));
            await _sessionRepository.SaveAsync(failureSession);

            throw new InvalidOperationException("パスワードが間違っています");
        }

        // Step 4: 成功時の UserAuthSession を生成
        var authorityRowId = AuthorityRowId.From(credentials.MappingEmployeeRowId);
        var loginCredentialsRowId = UsedLoginCredentialsRowId.From(credentials.RowId);

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
            LoggedInAt = now
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
