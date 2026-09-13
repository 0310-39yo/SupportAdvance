using IdentityEntity = SupportAdvance.Contexts.Identity.Domain.Entities.Identity;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Domain.Repositories;

/// <summary>
/// Identity Repository インターフェース
///
/// 【責務】
/// - Identity 集約の永続化（保存・取得・更新・削除）
/// - ドメインロジックと Infrastructure 層の間の境界
///
/// 【実装】Infrastructure層で Dapper/RepoDb を使用
/// </summary>
public interface IIdentityRepository
{
    /// <summary>
    /// IdentityRowId で Identity を取得
    /// </summary>
    /// <param name="id">IdentityRowId</param>
    /// <returns>Identity Entity、見つからない場合は null</returns>
    Task<IdentityEntity?> GetByIdAsync(IdentityRowId id);

    /// <summary>
    /// LoginId で Identity を取得
    /// </summary>
    /// <param name="loginId">ログインID</param>
    /// <returns>Identity Entity、見つからない場合は null</returns>
    Task<IdentityEntity?> GetByLoginIdAsync(LoginId loginId);

    /// <summary>
    /// EmployeeRowId で Identity を取得
    /// </summary>
    /// <param name="employeeRowId">従業員RowId</param>
    /// <returns>Identity Entity、見つからない場合は null</returns>
    Task<IdentityEntity?> GetByEmployeeRowIdAsync(EmployeeRowId employeeRowId);

    /// <summary>
    /// Identity を保存（新規作成）
    /// </summary>
    /// <param name="identity">保存する Identity Entity</param>
    /// <returns>保存後の IdentityRowId（採番済み）</returns>
    Task<IdentityRowId> SaveAsync(IdentityEntity identity);

    /// <summary>
    /// Identity を更新
    /// </summary>
    /// <param name="identity">更新する Identity Entity</param>
    /// <remarks>楽観ロック（row_version）による競合検出を含む</remarks>
    Task UpdateAsync(IdentityEntity identity);

    /// <summary>
    /// Identity を削除（論理削除）
    /// </summary>
    /// <param name="id">削除対象の IdentityRowId</param>
    /// <remarks>deleted_at / deleted_by を設定して論理削除</remarks>
    Task DeleteAsync(IdentityRowId id);
}
