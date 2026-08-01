using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Repositories;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(RowId userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(RowId roleId);
    Task CreateAsync(UserRole userRole);
    Task DeleteAsync(RowId userRoleId);
    Task DeleteByUserAndRoleAsync(RowId userId, RowId roleId);
}
