using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Permission;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 権限割り当てエンティティ（権限有効期間管理）
///
/// 【ID型】PermissionAssignmentRowId（独立した Entity ID）
/// 【親参照】EmployeeRowId（所属する従業員）
/// 【責務】従業員の権限割り当てと有効期間を管理、有効期限チェック
/// </summary>
public sealed class PermissionAssignment : Entity<PermissionAssignmentRowId>
{
    /// <summary>
    /// 所属従業員の ID
    /// </summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// 権限コードを取得する
    /// </summary>
    public PermissionCode PermissionCode { get; private set; }

    /// <summary>
    /// 有効開始日時を取得する
    /// </summary>
    public EffectiveAt EffectiveDate { get; private set; }

    /// <summary>
    /// 有効終了日時を取得する（無期限の場合は Unlimited）
    /// </summary>
    public ExpirationOn ExpirationDate { get; private set; }

    /// <summary>
    /// 指定されたプロパティから PermissionAssignment を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="assignmentRowId">権限割り当て行ID</param>
    /// <param name="employeeRowId">従業員行ID</param>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日</param>
    /// <param name="expirationDate">有効終了日</param>
    private PermissionAssignment(
        PermissionAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        PermissionCode permissionCode,
        EffectiveAt effectiveDate,
        ExpirationOn expirationDate)
    {
        RowId = assignmentRowId;
        EmployeeRowId = employeeRowId;
        PermissionCode = permissionCode;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい PermissionAssignment を生成する（ファクトリメソッド）
    /// </summary>
    public static PermissionAssignment Create(
        PermissionAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        PermissionCode permissionCode,
        EffectiveAt effectiveDate,
        ExpirationOn? expirationDate = null) =>
        new(assignmentRowId, employeeRowId, permissionCode, effectiveDate, expirationDate ?? ExpirationOn.Unlimited);

    /// <summary>
    /// DB から読み込んだ値から PermissionAssignment を復元する（ファクトリメソッド）
    /// </summary>
    public static PermissionAssignment Reconstruct(
        PermissionAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        PermissionCode permissionCode,
        EffectiveAt effectiveDate,
        ExpirationOn expirationDate) =>
        new(assignmentRowId, employeeRowId, permissionCode, effectiveDate, expirationDate);

    /// <summary>
    /// この権限割り当てが指定時点で有効かどうかを判定する
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
    /// PermissionAssignment の文字列表現を取得する
    /// </summary>
    public override string ToString()
        =>
            $"PermissionAssignment(RowId={RowId.Value}, Code={PermissionCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
}
