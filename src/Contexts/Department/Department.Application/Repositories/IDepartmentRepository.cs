namespace SupportAdvance.Contexts.Department.Application.Repositories;

using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署リポジトリのインターフェース
///
/// 【責務】
///   - Department 集約の永続化（保存・取得・削除）
///   - DB との間のマッピング（DbModel ↔ Entity）
/// 【実装者】Infrastructure 層の DepartmentRepository
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// 部署を行IDで取得する
    /// </summary>
    /// <param name="id">部署行ID</param>
    /// <returns>部署（存在しない場合は null）</returns>
    Task<Department?> GetByIdAsync(DepartmentRowId id);

    /// <summary>
    /// 部署をコードで取得する
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
    /// 【責務】
    ///   - UpdatedAt/UpdatedBy を設定
    ///   - 楽観ロック（RowVersion）を管理
    /// </summary>
    /// <param name="department">保存する部署</param>
    Task SaveAsync(Department department);

    /// <summary>
    /// 部署を論理削除する
    /// 【責務】
    ///   - DeletedAt/DeletedBy を設定
    ///   - 実装上の判断：物理削除か論理削除か
    /// </summary>
    /// <param name="id">削除する部署の行ID</param>
    Task DeleteAsync(DepartmentRowId id);
}
