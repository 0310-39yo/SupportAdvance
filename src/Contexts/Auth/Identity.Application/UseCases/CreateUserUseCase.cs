using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.UseCases;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;

    public CreateUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<RowId> ExecuteAsync(
        string loginId,
        string email,
        string hashedPassword,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(loginId);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(hashedPassword);

        var existingByLoginId = await _userRepository.GetByLoginIdAsync(loginId);
        if (existingByLoginId != null)
        {
            throw new InvalidOperationException($"User with login ID '{loginId}' already exists.");
        }

        var existingByEmail = await _userRepository.GetByEmailAsync(email);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists.");
        }

        var user = new User(loginId, email, hashedPassword, displayName);
        await _userRepository.CreateAsync(user);

        return user.Id;
    }
}
