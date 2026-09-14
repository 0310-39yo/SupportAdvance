using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;

/// <summary>
/// 部署メンバーシップの行ID（RowId ベース）
/// </summary>
public sealed class DepartmentMembershipRowId : RowId, IEquatable<DepartmentMembershipRowId>
{
    public const long MinValue = 1L;

    private DepartmentMembershipRowId(long value) : base(value, true)
    {
    }

    public static DepartmentMembershipRowId From(long value) => new(value);

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized),
                $"DepartmentMembershipRowId must be >= {MinValue}");
        }
    }

    public bool Equals(DepartmentMembershipRowId? other) => other != null && Value == other.Value;

    public override string ToString() => $"DepartmentMembershipRowId({Value})";
}
