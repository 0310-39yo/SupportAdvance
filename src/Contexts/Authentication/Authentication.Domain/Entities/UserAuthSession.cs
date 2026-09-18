using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Authentication.Domain.Entities;

/// <summary>
/// ユーザー認証セッション（ログイン状態を表現）
///
/// 【責務】
/// - ユーザーのログイン状態を記録
/// - ログインセッション情報の保持（ユーザー、認証方式、ログイン/ログアウト時刻）
/// - 「誰がログインしているか」を管理
///
/// 【特徴】
/// - 認証情報マスター（m_login_credentials）の外部参照を含む
/// - トランザクションテーブル（t_user_auth_sessions）に対応
/// - 監査情報はRepository層で管理（Domain層には含めない）
///
/// 【不変性】
/// - UserAuthSessionRowId、AuthorityRowId、IsAdAuthenticated は生成後変更不可
/// - LoggedOutAt のみ、アプリケーション終了時に更新可能
/// </summary>
public sealed class UserAuthSession : AggregateRoot<UserAuthSessionRowId>
{
    /// <summary>
    /// 権限主体（m_employees.row_id）
    /// 【特徴】ログインしたユーザー（従業員）を示す
    /// 【制約】NOT NULL
    /// </summary>
    public AuthorityRowId AuthorityRowId { get; private set; }

    /// <summary>
    /// 認証方式（Windows AD か ローカル認証か）
    /// 【型】bool（true=AD認証、false=ローカル認証）
    /// 【不変性】生成後変更不可
    /// </summary>
    public bool IsAdAuthenticated { get; private set; }

    /// <summary>
    /// 認証成功/失敗（ログイン試行の結果）
    /// 【型】bool（true=成功、false=失敗）
    /// 【用途】失敗ログも記録（監査用）
    /// 【不変性】生成後変更不可
    /// </summary>
    public bool LoginSuccess { get; private set; }

    /// <summary>
    /// ログイン操作日時（成功・失敗共に記録）
    /// 【型】LocalDateTime（JST）
    /// 【特徴】タイムゾーン既知
    /// 【不変性】生成後変更不可
    /// </summary>
    public LocalDateTime LoggedInAt { get; private set; }

    /// <summary>
    /// ログアウト日時（アプリケーション終了時に設定）
    /// 【型】LocalDateTime または null
    /// 【特徴】アプリが正常終了した場合のみ値を持つ。異常終了時は NULL（監査用）
    /// 【可変性】生成後、SetLoggedOutAt() で更新可能
    /// </summary>
    public LocalDateTime? LoggedOutAt { get; private set; }

    /// <summary>
    /// ローカル認証情報マスター RowId
    /// 【型】LoginCredentialsRowId または null
    /// 【特徴】ローカル認証時のみ値を持つ。AD認証時は NULL
    /// 【用途】m_login_credentials.row_id への参照（監査追跡用）
    /// 【不変性】生成後変更不可
    /// </summary>
    public LoginCredentialsRowId? LoginCredentialsRowId { get; private set; }

    /// <summary>
    /// 楽観ロックタイムスタンプ
    /// 【用途】Repository が UpdateAsync 時に row_version で競合検出
    /// 【管理】Reconstruct() でのみ設定（DB から復元時）。Create() の新規作成では空配列のまま
    /// </summary>
    public byte[] RowVersion { get; internal set; } = [];

    private UserAuthSession(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        LocalDateTime? loggedOutAt,
        LoginCredentialsRowId? loginCredentialsRowId)
    {
        RowId = id ?? throw new ArgumentNullException(nameof(id));
        AuthorityRowId = authorityRowId ?? throw new ArgumentNullException(nameof(authorityRowId));
        IsAdAuthenticated = isAdAuthenticated;
        LoginSuccess = loginSuccess;
        LoggedInAt = loggedInAt;
        LoggedOutAt = loggedOutAt;
        LoginCredentialsRowId = loginCredentialsRowId;
    }

    /// <summary>
    /// UserAuthSession を新規作成（ログイン試行）
    /// </summary>
    /// <param name="id">セッション RowId</param>
    /// <param name="authorityRowId">権限主体（ユーザー）</param>
    /// <param name="isAdAuthenticated">認証方式（true=AD、false=ローカル）</param>
    /// <param name="loginSuccess">認証成功/失敗</param>
    /// <param name="loggedInAt">ログイン操作日時</param>
    /// <param name="loginCredentialsRowId">ローカル認証時のマスター RowId（NULLable）</param>
    /// <returns>ログアウト日時が未設定（<see langword="null"/>）の新しいセッション</returns>
    public static UserAuthSession Create(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        LoginCredentialsRowId? loginCredentialsRowId = null)
    {
        return new UserAuthSession(
            id,
            authorityRowId,
            isAdAuthenticated,
            loginSuccess,
            loggedInAt,
            loggedOutAt: null,
            loginCredentialsRowId);
    }

    /// <summary>
    /// DB から復元（全フィールド指定）
    /// 【用途】Repository が DbModel から Domain Entity を構築時に使用
    /// 【重要】rowVersion は楽観ロック用（更新時に競合検出）。DB から取得した値をそのまま渡す
    /// </summary>
    /// <param name="id">セッションの行ID</param>
    /// <param name="authorityRowId">権限主体（従業員）の行ID</param>
    /// <param name="isAdAuthenticated">AD 認証の場合は <see langword="true"/>、ローカル認証の場合は <see langword="false"/></param>
    /// <param name="loginSuccess">認証に成功した場合は <see langword="true"/></param>
    /// <param name="loggedInAt">ログイン操作日時（JST）</param>
    /// <param name="loggedOutAt">ログアウト日時（JST）。<see langword="null"/> はログアウト操作なし</param>
    /// <param name="loginCredentialsRowId">ローカル認証で使用した認証情報の行ID。AD 認証の場合は <see langword="null"/></param>
    /// <param name="rowVersion">楽観ロック用の値。<see langword="null"/> の場合は設定なし</param>
    /// <returns>復元したセッション</returns>
    public static UserAuthSession Reconstruct(
        UserAuthSessionRowId id,
        AuthorityRowId authorityRowId,
        bool isAdAuthenticated,
        bool loginSuccess,
        LocalDateTime loggedInAt,
        LocalDateTime? loggedOutAt,
        LoginCredentialsRowId? loginCredentialsRowId,
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
    /// ログアウト日時を設定（アプリケーション終了時に呼び出す）
    /// </summary>
    /// <param name="loggedOutAt">ログアウト日時</param>
    public void SetLoggedOutAt(LocalDateTime loggedOutAt)
    {
        LoggedOutAt = loggedOutAt;
    }

    /// <summary>
    /// セッションが有効か判定
    /// </summary>
    /// <returns>ログイン成功かつ未ログアウト状態の場合 true</returns>
    public bool IsActive() => LoginSuccess && LoggedOutAt == null;
}
