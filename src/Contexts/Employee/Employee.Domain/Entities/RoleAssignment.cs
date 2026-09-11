using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ロール割り当てエンティティ（ロール有効期間管理）
///
/// 【ID型】RoleAssignmentRowId（独立した Entity ID）
/// 【親参照】EmployeeRowId（所属する従業員）
/// 【責務】従業員のロール割り当てと有効期間を管理、有効期限チェック
/// </summary>
public sealed class RoleAssignment : Entity<RoleAssignmentRowId>
{
    /// <summary>所属従業員の ID</summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// ロールコードを取得する
    /// </summary>
    public RoleCode RoleCode { get; private set; }

    /// <summary>
    /// 有効開始日を取得する
    /// </summary>
    public EffectiveAt EffectiveDate { get; private set; }

    /// <summary>
    /// 有効終了日時を取得する（無期限の場合は Unlimited）
    /// </summary>
    public ExpirationOn ExpirationDate { get; private set; }

    /// <summary>
    /// 指定されたプロパティから RoleAssignment を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="assignmentRowId">ロール割り当て行ID</param>
    /// <param name="employeeRowId">従業員行ID</param>
    /// <param name="roleCode">ロールコード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時</param>
    private RoleAssignment(
        RoleAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        RoleCode roleCode,
        EffectiveAt effectiveDate,
        ExpirationOn expirationDate)
    {
        RowId = assignmentRowId;
        EmployeeRowId = employeeRowId;
        RoleCode = roleCode;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい RoleAssignment を生成する（ファクトリメソッド）
    /// </summary>
    public static RoleAssignment Create(
        RoleAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        RoleCode roleCode,
        EffectiveAt effectiveDate,
        ExpirationOn? expirationDate = null) =>
        new(assignmentRowId, employeeRowId, roleCode, effectiveDate, expirationDate ?? ExpirationOn.Unlimited);

    /// <summary>
    /// DB から読み込んだ値から RoleAssignment を復元する（ファクトリメソッド）
    /// </summary>
    public static RoleAssignment Reconstruct(
        RoleAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        RoleCode roleCode,
        EffectiveAt effectiveDate,
        ExpirationOn expirationDate) =>
        new(assignmentRowId, employeeRowId, roleCode, effectiveDate, expirationDate);

    /// <summary>
    /// このロール割り当てが指定時点で有効かどうかを判定する
    /// </summary>
    /// <param name="asOf">判定時点</param>
    /// <returns>EffectiveDate 以後かつ ExpirationDate 前なら true</returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // EffectiveDate より前なら無効
        if (asOf < EffectiveDate.Value)
        {
            return false;
        }

        // ExpirationDate が無期限の場合は常に有効
        if (!ExpirationDate.HasExpiration)
        {
            return true;
        }

        // asOf が ExpirationDate より前なら有効、以後なら無効
        return asOf < ExpirationDate.Value;
    }

    /// <summary>
    /// RoleAssignment の文字列表現を取得する
    /// </summary>
    public override string ToString()
        =>
            $"RoleAssignment(RowId={RowId.Value}, Code={RoleCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
}
