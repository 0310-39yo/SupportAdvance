using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Repositories;

public class UserRoleRepository : RepositoryBase<UserRole, UserRoleDbModel, RowId>, IUserRoleRepository
{
    public UserRoleRepository(
        UserRoleMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<IEnumerable<UserRole>> GetByUserIdAsync(RowId userId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<UserRole>> GetByRoleIdAsync(RowId roleId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(UserRole userRole)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(RowId userRoleId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteByUserAndRoleAsync(RowId userId, RowId roleId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
