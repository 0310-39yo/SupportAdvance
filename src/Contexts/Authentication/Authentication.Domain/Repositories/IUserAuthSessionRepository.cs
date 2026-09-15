using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Repositories;

/// <summary>
/// ユーザー認証セッション Repository インターフェース
///
/// 【責務】
/// - UserAuthSession 集約の永続化（保存・取得・更新・削除）
/// - ドメインロジックと Infrastructure 層の間の境界
/// - 監査情報（CreatedAt, UpdatedAt, DeletedAt）の管理
///
/// 【実装】Infrastructure層で Dapper/RepoDb を使用
/// </summary>
public interface IUserAuthSessionRepository
{
    /// <summary>
    /// UserAuthSessionRowId でセッションを取得
    /// </summary>
    /// <param name="id">UserAuthSessionRowId</param>
    /// <returns>UserAuthSession Entity、見つからない場合は null</returns>
    Task<UserAuthSession?> GetByIdAsync(UserAuthSessionRowId id);

    /// <summary>
    /// AuthorityRowId（ユーザー）で最新のセッションを取得
    /// </summary>
    /// <param name="authorityRowId">権限主体（従業員 RowId）</param>
    /// <returns>最新の有効セッション、見つからない場合は null</returns>
    Task<UserAuthSession?> GetLatestByAuthorityRowIdAsync(AuthorityRowId authorityRowId);

    /// <summary>
    /// LoginCredentialsRowId（ローカル認証マスター）で最新のセッションを取得
    /// </summary>
    /// <param name="loginCredentialsRowId">ローカル認証マスター RowId</param>
    /// <returns>最新のセッション、見つからない場合は null</returns>
    Task<UserAuthSession?> GetLatestByLoginCredentialsRowIdAsync(LoginCredentialsRowId loginCredentialsRowId);

    /// <summary>
    /// UserAuthSession を保存（新規作成）
    /// </summary>
    /// <param name="session">保存する UserAuthSession Entity</param>
    /// <returns>保存後の UserAuthSessionRowId（採番済み）</returns>
    Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session);

    /// <summary>
    /// UserAuthSession を更新
    /// </summary>
    /// <param name="session">更新する UserAuthSession Entity</param>
    /// <remarks>楽観ロック（row_version）による競合検出を含む。主に LoggedOutAt 更新用</remarks>
    Task UpdateAsync(UserAuthSession session);

    /// <summary>
    /// UserAuthSession を削除（論理削除）
    /// </summary>
    /// <param name="id">削除対象の UserAuthSessionRowId</param>
    /// <remarks>deleted_at / deleted_by を設定して論理削除</remarks>
    Task DeleteAsync(UserAuthSessionRowId id);
}
