using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Application.UseCases;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;

    public CreateUserUseCase(IUserRepository userRepository, IClock clock)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<UserId> ExecuteAsync(
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

        var user = new User(
            UserId.New(),
            loginId,
            email,
            hashedPassword,
            displayName,
            null,
            _clock);
        await _userRepository.CreateAsync(user);

        return user.Id;
    }
}
