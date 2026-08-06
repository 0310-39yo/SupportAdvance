using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId userId);
    Task<User?> GetByLoginIdAsync(string loginId);
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(UserId userId);
}
