using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Authentication.Domain.Entities;

/// <summary>
/// ユーザーのログイン状態を表す集約ルート（認証セッション）
/// </summary>
/// <remarks>
/// <para>【責務】ログインセッション情報（ユーザー、認証方式、ログイン／ログアウト日時）の保持。「誰がログインしているか」の管理</para>
/// <para>【特徴】認証情報マスター（<c>m_login_credentials</c>）の外部参照を含む。トランザクションテーブル（<c>t_user_auth_sessions</c>）に対応。監査情報は Repository が管理し、Domain 層への含有なし</para>
/// <para>【不変条件】<see cref="Entity{TId}.RowId"/>、<see cref="AuthorityRowId"/>、<see cref="IsAdAuthenticated"/> は生成後変更不可。<see cref="LoggedOutAt"/> のみ、アプリケーション終了時に <see cref="SetLoggedOutAt"/> で更新可能</para>
/// <para>【null契約】未設定の項目（ログアウト日時、認証情報の行ID）は <see langword="null"/> ではなく、各値オブジェクトの Unset で表現。Domain 層での null 確認は不要</para>
/// </remarks>
public sealed class UserAuthSession : AggregateRoot<UserAuthSessionRowId>
{
    /// <summary>
    /// 権限主体（<c>m_employees.row_id</c>）。ログインしたユーザー（従業員）
    /// </summary>
    public AuthorityRowId AuthorityRowId { get; private set; }

    /// <summary>
    /// 認証方式が Windows AD かどうかを示す値
    /// </summary>
    /// <value>AD 認証の場合は <see langword="true"/>、ローカル認証の場合は <see langword="false"/>。生成後変更不可</value>
    public bool IsAdAuthenticated { get; private set; }

    /// <summary>
    /// 認証に成功したかどうかを示す値
    /// </summary>
    /// <value>成功の場合は <see langword="true"/>、失敗の場合は <see langword="false"/>。失敗も監査用に記録するため、生成後変更不可</value>
    public bool LoginSuccess { get; private set; }

    /// <summary>
    /// ログイン操作日時（JST）。成功・失敗とも記録
    /// </summary>
    /// <value>生成後変更不可</value>
    public LocalDateTime LoggedInAt { get; private set; }

    /// <summary>
    /// ログアウト日時（JST）
    /// </summary>
    /// <value>アプリケーションが正常終了した場合のみ設定。異常終了の場合は未設定（<see cref="LoggedOutAt.Unset"/>。監査用）</value>
    public LoggedOutAt LoggedOutAt { get; private set; }

    /// <summary>
    /// ローカル認証で使用した認証情報の行ID
    /// </summary>
    /// <value>ローカル認証の場合のみ設定（<c>m_login_credentials.row_id</c> への参照。監査追跡用）。AD 認証の場合は未設定（<see cref="UsedLoginCredentialsRowId.Unset"/>）。生成後変更不可</value>
    public UsedLoginCredentialsRowId LoginCredentialsRowId { get; private set; }

    /// <summary>
    /// 楽観ロック用のタイムスタンプ
    /// </summary>
    /// <value>Repository が更新時に <c>row_version</c> で競合を検出するための値。<see cref="Reconstruct"/> でのみ設定。<see cref="Create"/> による新規作成では空配列のまま</value>
    public byte[] RowVersion { get; internal set; } = [];

    private UserAuthSession(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        LoggedOutAt loggedOutAt,
        UsedLoginCredentialsRowId loginCredentialsRowId)
    {
        RowId = id ?? throw new ArgumentNullException(nameof(id));
        AuthorityRowId = authorityRowId ?? throw new ArgumentNullException(nameof(authorityRowId));
        IsAdAuthenticated = isAdAuthenticated;
        LoginSuccess = loginSuccess;
        LoggedInAt = loggedInAt;
        LoggedOutAt = loggedOutAt ?? throw new ArgumentNullException(nameof(loggedOutAt));
        LoginCredentialsRowId = loginCredentialsRowId ?? throw new ArgumentNullException(nameof(loginCredentialsRowId));
    }

    /// <summary>
    /// 認証セッションの新規作成（ログイン試行）
    /// </summary>
    /// <param name="id">セッションの行ID</param>
    /// <param name="authorityRowId">権限主体（ユーザー）の行ID</param>
    /// <param name="isAdAuthenticated">AD 認証の場合は <see langword="true"/>、ローカル認証の場合は <see langword="false"/></param>
    /// <param name="loginSuccess">認証に成功した場合は <see langword="true"/></param>
    /// <param name="loggedInAt">ログイン操作日時（JST）</param>
    /// <param name="loginCredentialsRowId">ローカル認証で使用した認証情報の行ID。使用なしの場合（AD 認証、存在しないログインID での失敗など）は <see cref="UsedLoginCredentialsRowId.Unset"/></param>
    /// <returns>ログアウト日時が未設定（<see cref="LoggedOutAt.Unset"/>）の新しいセッション</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/>、<paramref name="authorityRowId"/>、または <paramref name="loginCredentialsRowId"/> が <see langword="null"/> の場合</exception>
    public static UserAuthSession Create(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        UsedLoginCredentialsRowId loginCredentialsRowId) =>
        new(
            id,
            authorityRowId,
            isAdAuthenticated,
            loginSuccess,
            loggedInAt,
            LoggedOutAt.Unset(),
            loginCredentialsRowId);

    /// <summary>
    /// DB から読み込んだ値による認証セッションの復元（全項目指定）
    /// </summary>
    /// <param name="id">セッションの行ID</param>
    /// <param name="authorityRowId">権限主体（従業員）の行ID</param>
    /// <param name="isAdAuthenticated">AD 認証の場合は <see langword="true"/>、ローカル認証の場合は <see langword="false"/></param>
    /// <param name="loginSuccess">認証に成功した場合は <see langword="true"/></param>
    /// <param name="loggedInAt">ログイン操作日時（JST）</param>
    /// <param name="loggedOutAt">ログアウト日時（JST）。ログアウト操作なしの場合は <see cref="LoggedOutAt.Unset"/></param>
    /// <param name="loginCredentialsRowId">ローカル認証で使用した認証情報の行ID。AD 認証の場合は <see cref="UsedLoginCredentialsRowId.Unset"/></param>
    /// <param name="rowVersion">楽観ロック用の値。<see langword="null"/> の場合は設定なし</param>
    /// <returns>復元したセッション</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/>、<paramref name="authorityRowId"/>、<paramref name="loggedOutAt"/>、または <paramref name="loginCredentialsRowId"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【用途】Infrastructure 層のマッパーからの呼び出し専用。<paramref name="rowVersion"/> は DB から取得した値をそのまま渡す（更新時の競合検出用）</para>
    /// </remarks>
    public static UserAuthSession Reconstruct(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        LoggedOutAt loggedOutAt,
        UsedLoginCredentialsRowId loginCredentialsRowId,
        byte[]? rowVersion = null)
    {
        var session = new UserAuthSession(
            id,
            authorityRowId,
            isAdAuthenticated,
            loginSuccess,
            loggedInAt,
            loggedOutAt,
            loginCredentialsRowId);

        if (rowVersion != null)
        {
            session.RowVersion = rowVersion;
        }

        return session;
    }

    /// <summary>
    /// ログアウト日時の設定
    /// </summary>
    /// <param name="loggedOutAt">ログアウト日時（JST）</param>
    /// <exception cref="ArgumentException"><paramref name="loggedOutAt"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>
    /// <remarks>
    /// <para>【呼び出し元】アプリケーション終了時</para>
    /// <para>【副作用】<see cref="LoggedOutAt"/> の更新</para>
    /// </remarks>
    public void SetLoggedOutAt(LocalDateTime loggedOutAt)
    {
        if (!LoggedOutAt.TryFrom(loggedOutAt, out var value))
        {
            throw new ArgumentException(
                "LoggedOutAt must be a valid system timestamp, not MinValue or MaxValue.",
                nameof(loggedOutAt));
        }

        LoggedOutAt = value;
    }

    /// <summary>
    /// セッションが有効かどうかの判定
    /// </summary>
    /// <returns>ログイン成功、かつ未ログアウトの場合は <see langword="true"/></returns>
    public bool IsActive() => LoginSuccess && !LoggedOutAt.HasLoggedOut;
}
