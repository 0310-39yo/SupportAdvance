using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;

/// <summary>
/// Employee を識別する ID（GUID ベース）
/// 【用途】Employee.Id として使用
/// 【生成】通常は EmployeeId.New() で自動生成
/// </summary>
public class EmployeeId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public EmployeeId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static EmployeeId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static EmployeeId From(Guid value) => new(value);
}
