using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Application.UseCases;

public class CreateRoleUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IClock _clock;

    public CreateRoleUseCase(IRoleRepository roleRepository, IClock clock)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<RoleId> ExecuteAsync(
        string name,
        IEnumerable<string> permissions,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(permissions);

        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{name}' already exists.");
        }

        var role = new Role(
            RoleId.New(),
            name,
            permissions,
            description,
            null,
            _clock);
        await _roleRepository.CreateAsync(role);

        return role.Id;
    }
}
