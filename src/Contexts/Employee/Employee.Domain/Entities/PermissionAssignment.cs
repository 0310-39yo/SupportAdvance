using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Permission;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 権限割り当てエンティティ（権限有効期間管理）
/// </summary>
/// <remarks>
/// <para>【ID型】PermissionAssignmentRowId（独立した Entity ID）</para>
/// <para>【親参照】EmployeeRowId（所属する従業員）</para>
/// <para>【責務】従業員の権限割り当てと有効期間を管理、有効期限チェック</para>
/// </remarks>
public sealed class PermissionAssignment : Entity<PermissionAssignmentRowId>
{
    /// <summary>
    /// 所属従業員の ID
    /// </summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// 権限コードの取得
    /// </summary>
    public PermissionCode PermissionCode { get; private set; }

    /// <summary>
    /// 有効開始日時の取得
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
    /// <param name="assignmentRowId">権限割り当ての行ID</param>
    /// <param name="employeeRowId">割り当て先の従業員の行ID</param>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日時（JST）</param>
    /// <param name="expirationDate">有効期限。<see langword="null"/> の場合は無期限（<see cref="ExpirationOn.Unlimited"/>）</param>
    /// <returns>生成した権限割り当て</returns>
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
    /// <param name="assignmentRowId">権限割り当ての行ID</param>
    /// <param name="employeeRowId">割り当て先の従業員の行ID</param>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日時（JST）</param>
    /// <param name="expirationDate">有効期限（無期限の場合は <see cref="ExpirationOn.Unlimited"/>）</param>
    /// <returns>復元した権限割り当て</returns>
    public static PermissionAssignment Reconstruct(
        PermissionAssignmentRowId assignmentRowId,
        EmployeeRowId employeeRowId,
        PermissionCode permissionCode,
        EffectiveAt effectiveDate,
        ExpirationOn expirationDate) =>
        new(assignmentRowId, employeeRowId, permissionCode, effectiveDate, expirationDate);

    /// <summary>
    /// この権限割り当てが指定時点で有効かどうかの判定
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
    /// PermissionAssignment の文字列表現の取得
    /// </summary>
    /// <returns><c>PermissionAssignment(RowId=…, Code=…, Effective=…, Expiration=…)</c> 形式のデバッグ用文字列</returns>
    public override string ToString()
        =>
            $"PermissionAssignment(RowId={RowId.Value}, Code={PermissionCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
}
