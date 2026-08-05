using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(RowId roleId);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(RowId roleId);
}
