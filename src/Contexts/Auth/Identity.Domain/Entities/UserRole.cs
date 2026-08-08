using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Entities;

public class UserRole : AggregateRoot<UserRoleId>
{
    private RowId _rowId = null!;

    public RowId RowId => _rowId;
    public RowId UserId { get; private set; }
    public RowId RoleId { get; private set; }
    public LocalDateTime AssignedAt { get; private set; }

    public UserRole(
        UserRoleId id,
        RowId userId,
        RowId roleId,
        LocalDateTime assignedAt,
        RowId? rowId = null,
        IClock? clock = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleId);
        ArgumentNullException.ThrowIfNull(assignedAt);

        Id = id;
        _rowId = rowId ?? RowId.New();
        UserId = userId;
        RoleId = roleId;
        AssignedAt = assignedAt;
    }
}
