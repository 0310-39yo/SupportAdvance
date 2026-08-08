using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;

/// <summary>
/// Department を識別する ID（GUID ベース）
/// 【用途】Department.Id として使用
/// 【生成】通常は DepartmentId.New() で自動生成
/// </summary>
public class DepartmentId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DepartmentId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static DepartmentId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static DepartmentId From(Guid value) => new(value);
}

