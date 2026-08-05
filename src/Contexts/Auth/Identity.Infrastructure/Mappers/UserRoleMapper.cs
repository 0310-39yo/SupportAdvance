using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Mappers;

public class UserRoleMapper : IEntityMapper<UserRole, UserRoleDbModel, RowId>
{
    public UserRoleDbModel ToDbModel(UserRole entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserRoleDbModel
        {
            RowId = entity.Id.Value,
            UserId = entity.UserId.Value,
            RoleId = entity.RoleId.Value,
            AssignedAt = entity.AssignedAt.Value,
        };
    }

    public UserRole ToDomainEntity(UserRoleDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // TryFrom で型安全な DateTime → LocalDateTime 変換
        LocalDateTime assignedAt = new LocalDateTime(dbModel.AssignedAt);

        return new UserRole(
            userId: RowId.From(dbModel.UserId),
            roleId: RowId.From(dbModel.RoleId),
            assignedAt: assignedAt,
            rowId: RowId.From(dbModel.RowId));
    }
}
