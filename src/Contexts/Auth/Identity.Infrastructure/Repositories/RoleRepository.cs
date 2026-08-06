using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Repositories;

public class RoleRepository : RepositoryBase<Role, RoleDbModel, RoleId>, IRoleRepository
{
    public RoleRepository(
        RoleMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<Role?> GetByIdAsync(RoleId roleId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<Role?> GetByNameAsync(string name)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<Role>> GetAllAsync()
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(Role role)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task UpdateAsync(Role role)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(RoleId roleId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
