using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Infrastructure.DbModels;
using SupportAdvance.Contexts.Identity.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User, UserDbModel, RowId>, IUserRepository
{
    public UserRepository(
        UserMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<User?> GetByIdAsync(RowId userId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<User?> GetByLoginIdAsync(string loginId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(User user)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task UpdateAsync(User user)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(RowId userId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
