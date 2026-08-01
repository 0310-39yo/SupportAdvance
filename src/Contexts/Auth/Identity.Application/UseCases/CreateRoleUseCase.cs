using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.UseCases;

public class CreateRoleUseCase
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<RowId> ExecuteAsync(
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

        var role = new Role(name, permissions, description);
        await _roleRepository.CreateAsync(role);

        return role.Id;
    }
}
