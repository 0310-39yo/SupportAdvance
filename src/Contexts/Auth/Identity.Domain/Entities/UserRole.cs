using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Entities;

public class UserRole : AggregateRoot<RowId>
{
    public RowId UserId { get; private set; }
    public RowId RoleId { get; private set; }
    public LocalDateTime AssignedAt { get; private set; }

    public UserRole(
        RowId userId,
        RowId roleId,
        LocalDateTime assignedAt,
        RowId? rowId = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleId);
        ArgumentNullException.ThrowIfNull(assignedAt);

        Id = rowId ?? RowId.New();
        UserId = userId;
        RoleId = roleId;
        AssignedAt = assignedAt;
    }
}
