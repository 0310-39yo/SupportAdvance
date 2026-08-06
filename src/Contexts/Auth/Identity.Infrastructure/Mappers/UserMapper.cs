using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;

public class UserMapper : IEntityMapper<User, UserDbModel, UserId>
{
    public UserDbModel ToDbModel(User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserDbModel
        {
            RowId = entity.RowId.Value,
            UserId = entity.Id.Value,
            LoginId = entity.LoginId,
            Email = entity.Email,
            HashedPassword = entity.HashedPassword,
            IsActive = entity.IsActive,
            DisplayName = entity.DisplayName,
            CreatedAt = default,
            CreatedBy = 0,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
        };
    }

    public User ToDomainEntity(UserDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var userId = UserId.From(dbModel.UserId);
        var rowId = RowId.From(dbModel.RowId);

        return new User(
            id: userId,
            loginId: dbModel.LoginId,
            email: dbModel.Email,
            hashedPassword: dbModel.HashedPassword,
            displayName: dbModel.DisplayName,
            rowId: rowId,
            clock: clock);
    }
}
