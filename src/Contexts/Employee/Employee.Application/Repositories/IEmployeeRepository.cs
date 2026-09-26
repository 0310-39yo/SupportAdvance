using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.Repositories;

using Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Domain.ValueObjects.Person;

/// <summary>
/// Employee 集約の Repository インターフェース
/// </summary>
/// <remarks>
/// <para>【責務】Employee エンティティのデータアクセス契約を定義</para>
/// <para>【提供メソッド】GetByIdAsync, GetByRowIdAsync, GetByPersonRowIdAsync, SaveAsync, AddAsync, UpdateAsync, DeleteAsync</para>
/// <para>【SaveAsync】RowVersion で自動判定し、新規作成なら AddAsync、更新なら UpdateAsync へ委譲</para>
/// </remarks>
public interface IEmployeeRepository
{
    /// <summary>
    /// 集約根ID（EmployeeRowId）で Employee の検索
    /// </summary>
    /// <param name="id">Employee の集約根ID</param>
    /// <returns>見つかった Employee インスタンス、または null</returns>
    Task<Employee?> GetByIdAsync(EmployeeRowId id);

    /// <summary>
    /// DB行ID で Employee の検索
    /// </summary>
    /// <param name="rowId">m_employees.row_id</param>
    /// <returns>見つかった Employee インスタンス、または null</returns>
    Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId);

    /// <summary>
    /// BizId（従業員番号）で Employee の検索
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
    /// Employee を保存する（新規作成または更新）
    /// </summary>
    /// <param name="employee">保存対象の Employee</param>
    /// <remarks>
    /// <para>【重要】RowVersion で自動判定し、AddAsync/UpdateAsync へ委譲</para>
    /// - 新規作成（RowVersion が空）： AddAsync を呼び出し、created_at/created_by を設定
    /// - 更新（RowVersion が非空）： UpdateAsync を呼び出し、updated_at/updated_by を設定
    /// - 楽観ロック（row_version）による競合検出（更新時）
    /// </remarks>
    Task SaveAsync(Employee employee);

    /// <summary>
    /// 新規 Employee の DB への登録
    /// </summary>
    /// <param name="employee">登録対象の Employee</param>
    /// <remarks>
    /// 監査カラム（created_at, created_by）は Repository で自動設定
    /// トランザクション管理は呼び出し元で実施
    /// </remarks>
    Task AddAsync(Employee employee);

    /// <summary>
    /// 既存 Employee の DB での更新
    /// </summary>
    /// <param name="employee">更新対象の Employee</param>
    /// <remarks>
    /// 監査カラム（updated_at, updated_by）は Repository で自動設定
    /// 楽観ロック（row_version）による競合検出
    /// </remarks>
    Task UpdateAsync(Employee employee);

    /// <summary>
    /// Employee の論理削除
    /// </summary>
    /// <param name="id">削除対象の Employee ID（EmployeeRowId）</param>
    /// <remarks>
    /// 物理削除ではなく、deleted_at, deleted_by を設定して論理削除。
    /// 楽観ロック（row_version）による競合検出
    /// </remarks>
    Task DeleteAsync(EmployeeRowId id);
}
