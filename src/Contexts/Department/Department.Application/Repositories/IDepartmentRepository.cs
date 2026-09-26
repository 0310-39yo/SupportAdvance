namespace SupportAdvance.Contexts.Department.Application.Repositories;

using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署リポジトリのインターフェース
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>Department 集約の永続化（保存・取得・削除）</description></item>
/// <item><description>DB との間のマッピング（DbModel ↔ Entity）</description></item>
/// </list>
/// <para>【実装者】Infrastructure 層の DepartmentRepository</para>
/// </remarks>
public interface IDepartmentRepository
{
    /// <summary>
    /// 部署の行IDでの取得
    /// </summary>
    /// <param name="id">部署行ID</param>
    /// <returns>部署（存在しない場合は null）</returns>
    Task<Department?> GetByIdAsync(DepartmentRowId id);

    /// <summary>
    /// 部署のコードでの取得
    /// </summary>
    /// <param name="code">部署コード</param>
    /// <returns>部署（存在しない場合は null）</returns>
    Task<Department?> GetByCodeAsync(DepartmentCode code);

    /// <summary>
    /// すべての部署を取得する（廃止済みを含む）
    /// </summary>
    /// <returns>部署のリスト（削除済み論理削除分も含む）</returns>
    Task<IReadOnlyList<Department>> GetAllAsync();

    /// <summary>
    /// 部署を保存する（新規作成または更新）
    /// </summary>
    /// <param name="department">保存する部署</param>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>UpdatedAt/UpdatedBy を設定</description></item>
    /// <item><description>楽観ロック（RowVersion）を管理</description></item>
    /// </list>
    /// </remarks>
    Task SaveAsync(Department department);

    /// <summary>
    /// 部署の論理削除
    /// </summary>
    /// <param name="id">削除する部署の行ID</param>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>DeletedAt/DeletedBy を設定</description></item>
    /// <item><description>実装上の判断：物理削除か論理削除か</description></item>
    /// </list>
    /// </remarks>
    Task DeleteAsync(DepartmentRowId id);
}
