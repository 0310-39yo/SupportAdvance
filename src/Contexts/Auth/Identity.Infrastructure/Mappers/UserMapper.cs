using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Mappers;

public class UserMapper : IEntityMapper<User, UserDbModel, RowId>
{
    public UserDbModel ToDbModel(User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserDbModel
        {
            RowId = entity.Id.Value,
            LoginId = entity.LoginId,
            Email = entity.Email,
            HashedPassword = entity.HashedPassword,
            IsActive = entity.IsActive,
            DisplayName = entity.DisplayName,
        };
    }

    public User ToDomainEntity(UserDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        return new User(
            loginId: dbModel.LoginId,
            email: dbModel.Email,
            hashedPassword: dbModel.HashedPassword,
            displayName: dbModel.DisplayName,
            rowId: RowId.From(dbModel.RowId));
    }
}
