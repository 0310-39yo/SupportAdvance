using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

namespace SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// Employee 集約の Repository インターフェース
///
/// 【責務】Employee エンティティのデータアクセス契約を定義
/// 【提供メソッド】GetByIdAsync, GetByRowIdAsync, GetByPersonRowIdAsync, AddAsync, UpdateAsync, DeleteAsync
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// 集約根ID（EmployeeRowId）で Employee を検索する
    /// </summary>
    /// <param name="id">Employee の集約根ID</param>
    /// <returns>見つかった Employee インスタンス、または null</returns>
    Task<Employee?> GetByIdAsync(EmployeeRowId id);

    /// <summary>
    /// DB行ID で Employee を検索する
    /// </summary>
    /// <param name="rowId">m_employees.row_id</param>
    /// <returns>見つかった Employee インスタンス、または null</returns>
    Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId);

    /// <summary>
    /// BizId（従業員番号）で Employee を検索する
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号、1001以上）</param>
    /// <returns>見つかった Employee インスタンス、または null</returns>
    Task<Employee?> GetByBizIdAsync(int bizId);

    /// <summary>
    /// 人事マスタ行ID で Employee を検索する（複数結果想定）
    /// </summary>
    /// <param name="personRowId">m_persons.row_id</param>
    /// <returns>見つかった Employee インスタンスのリスト</returns>
    Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId);

    /// <summary>
    /// 新規 Employee を DB に登録する
    /// </summary>
    /// <param name="employee">登録対象の Employee</param>
    /// <remarks>
    /// 監査カラム（created_at, created_by）は Repository で自動設定。
    /// トランザクション管理は呼び出し元で実施。
    /// </remarks>
    Task AddAsync(Employee employee);

    /// <summary>
    /// 既存 Employee を DB で更新する
    /// </summary>
    /// <param name="employee">更新対象の Employee</param>
    /// <remarks>
    /// 監査カラム（updated_at, updated_by）は Repository で自動設定。
    /// 楽観ロック（row_version）による競合検出。
    /// </remarks>
    Task UpdateAsync(Employee employee);

    /// <summary>
    /// Employee を論理削除する
    /// </summary>
    /// <param name="id">削除対象の Employee ID（EmployeeRowId）</param>
    /// <remarks>
    /// 物理削除ではなく、deleted_at, deleted_by を設定して論理削除。
    /// 楽観ロック（row_version）による競合検出。
    /// </remarks>
    Task DeleteAsync(EmployeeRowId id);
}
