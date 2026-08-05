using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(RowId userId);
    Task<User?> GetByLoginIdAsync(string loginId);
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(RowId userId);
}
