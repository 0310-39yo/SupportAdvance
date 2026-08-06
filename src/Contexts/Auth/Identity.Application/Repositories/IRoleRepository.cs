using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(RoleId roleId);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(RoleId roleId);
}
