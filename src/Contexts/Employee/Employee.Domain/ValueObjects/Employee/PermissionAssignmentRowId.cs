using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 権限割り当ての行ID（RowId ベース）
/// </summary>
public sealed class PermissionAssignmentRowId : RowId, IEquatable<PermissionAssignmentRowId>
{
    public const long MinValue = 1L;

    private PermissionAssignmentRowId(long value) : base(value, true)
    {
    }

    public static PermissionAssignmentRowId From(long value) => new(value);

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(nameof(normalized), $"PermissionAssignmentRowId must be >= {MinValue}");
    }

    public bool Equals(PermissionAssignmentRowId? other) => other != null && Value == other.Value;

    public override string ToString() => $"PermissionAssignmentRowId({Value})";
}
