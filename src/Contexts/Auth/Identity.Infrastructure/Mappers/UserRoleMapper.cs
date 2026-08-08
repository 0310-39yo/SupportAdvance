using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;

public class UserRoleMapper : IEntityMapper<UserRole, UserRoleDbModel, UserRoleId>
{
    public UserRoleDbModel ToDbModel(UserRole entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserRoleDbModel
        {
            RowId = entity.RowId.Value,
            UserRoleId = entity.Id.Value,
            UserId = entity.UserId.Value,
            RoleId = entity.RoleId.Value,
            AssignedAt = entity.AssignedAt,
            CreatedAt = default,
            CreatedBy = 0,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
        };
    }

    public UserRole ToDomainEntity(UserRoleDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var userRoleId = UserRoleId.From(dbModel.UserRoleId);
        var rowId = RowId.From(dbModel.RowId);

        return new UserRole(
            id: userRoleId,
            userId: RowId.From(dbModel.UserId),
            roleId: RowId.From(dbModel.RoleId),
            assignedAt: dbModel.AssignedAt,
            rowId: rowId,
            clock: clock);
    }
}
