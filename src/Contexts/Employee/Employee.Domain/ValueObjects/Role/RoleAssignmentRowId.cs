using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// ロール割り当ての行ID（RowId ベース）
/// </summary>
public sealed class RoleAssignmentRowId : RowId, IEquatable<RoleAssignmentRowId>
{
    public const long MinValue = 1L;

    private RoleAssignmentRowId(long value) : base(value, true)
    {
    }

    public static RoleAssignmentRowId From(long value) => new(value);

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(nameof(normalized), $"RoleAssignmentRowId must be >= {MinValue}");
    }

    public bool Equals(RoleAssignmentRowId? other) => other != null && Value == other.Value;

    public override string ToString() => $"RoleAssignmentRowId({Value})";
}
