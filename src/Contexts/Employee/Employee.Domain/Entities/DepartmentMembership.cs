using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 部署メンバーシップエンティティ（従業員の部署所属）
///
/// 【ID型】DepartmentMembershipRowId（独立した Entity ID）
/// 【親参照】EmployeeRowId（所属する従業員）
/// 【責務】配属期間の管理
/// 【コレクション構造】従業員が複数部署に所属可能
/// 【独立性】EndOn（配属終了日）は Employee.RetiredOn（雇用終了）と独立
/// - 配置転換時：前部署の EndOn を更新、Employee.RetiredOn は変わらない
/// - 退職時：Employee.RetiredOn は設定、各部署の EndOn は別途管理
/// </summary>
public sealed class DepartmentMembership : Entity<DepartmentMembershipRowId>
{
    /// <summary>
    /// 所属従業員の ID
    /// </summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// 所属部署の ID</summary>
    public DepartmentRowId DepartmentRowId { get; private set; }

    /// <summary>
    /// 部署名（表示用）
    /// </summary>
    public string? DepartmentName { get; private set; }

    /// <summary>
    /// 主部署フラグ（Primary で従業員の主所属）</summary>
    public IsPrimary IsPrimary { get; private set; }

    /// <summary>
    /// 異動終了日（null なら無期限・継続中）
    /// </summary>
    public EndOn EndOn { get; private set; }

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private DepartmentMembership(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        string? departmentName,
        IsPrimary isPrimary,
        EndOn endOn)
    {
        RowId = membershipRowId;
        EmployeeRowId = employeeRowId;
        DepartmentRowId = departmentRowId;
        DepartmentName = departmentName;
        IsPrimary = isPrimary;
        EndOn = endOn;
    }

    /// <summary>
    /// 新しい DepartmentMembership を生成する
    /// </summary>
    public static DepartmentMembership Create(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        IsPrimary isPrimary,
        EndOn? endOn = null,
        string? departmentName = null) =>
        new(membershipRowId, employeeRowId, departmentRowId, departmentName, isPrimary, endOn ?? EndOn.Unset());

    /// <summary>
    /// DB から読み込んだ値から DepartmentMembership を復元する
    /// </summary>
    public static DepartmentMembership Reconstruct(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        IsPrimary isPrimary,
        EndOn endOn,
        string? departmentName = null) =>
        new(membershipRowId, employeeRowId, departmentRowId, departmentName, isPrimary, endOn);

    /// <summary>
    /// このメンバーシップが指定時点で有効かどうかを判定する
    /// </summary>
    public bool IsActive(LocalDateTime asOf)
    {
        // EndOn が null（無期限）なら常に有効
        if (!EndOn.HasEnded)
        {
            return true;
        }

        // asOf が EndOn より前なら有効、以後なら無効
        return asOf < EndOn.Value;
    }

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString()
        => $"DepartmentMembership(RowId={RowId.Value}, DeptRowId={DepartmentRowId}, Primary={IsPrimary})";
}
