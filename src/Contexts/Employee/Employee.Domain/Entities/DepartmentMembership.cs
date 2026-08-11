namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 部署メンバーシップエンティティ（従業員の部署所属）
///
/// 【ID型】DepartmentMembershipRowId（独立した Entity ID）
/// 【親参照】EmployeeRowId（所属する従業員）
/// 【責務】従業員の部署所属管理、有効期限チェック
/// 【コレクション構造】従業員が複数部署に所属可能
/// </summary>
public sealed class DepartmentMembership : Entity<DepartmentMembershipRowId>
{
    /// <summary>所属従業員の ID</summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>部署コード</summary>
    public DepartmentCode DepartmentCode { get; private set; }

    /// <summary>主部署フラグ（true で従業員の主所属）</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>メンバーシップの有効期限（null で無期限）</summary>
    public LocalDateTime? ExpirationDate { get; private set; }

    /// <summary>プライベートコンストラクタ</summary>
    private DepartmentMembership(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime? expirationDate = null)
    {
        RowId = membershipRowId;
        EmployeeRowId = employeeRowId;
        DepartmentCode = departmentCode;
        IsPrimary = isPrimary;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい DepartmentMembership を生成する
    /// </summary>
    public static DepartmentMembership Create(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime? expirationDate = null)
    {
        return new(membershipRowId, employeeRowId, departmentCode, isPrimary, expirationDate);
    }

    /// <summary>
    /// DB から読み込んだ値から DepartmentMembership を復元する
    /// </summary>
    public static DepartmentMembership Reconstruct(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime? expirationDate = null)
    {
        return new(membershipRowId, employeeRowId, departmentCode, isPrimary, expirationDate);
    }

    /// <summary>
    /// このメンバーシップが指定時点で有効かどうかを判定する
    /// </summary>
    public bool IsActive(LocalDateTime asOf)
    {
        if (ExpirationDate == null)
        {
            return true;
        }

        return asOf < ExpirationDate;
    }

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString()
        => $"DepartmentMembership(RowId={RowId.Value}, Code={DepartmentCode}, Primary={IsPrimary})";
}
