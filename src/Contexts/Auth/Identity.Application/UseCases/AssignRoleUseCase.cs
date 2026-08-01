using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Application.UseCases;

public class AssignRoleUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IClock _clock;

    public AssignRoleUseCase(IUserRoleRepository userRoleRepository, IClock clock)
    {
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task ExecuteAsync(RowId userId, RowId roleId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleId);

        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        if (userRoles.Any(ur => ur.RoleId == roleId))
        {
            throw new InvalidOperationException($"User already has role with ID '{roleId.Value}'.");
        }

        var userRole = new UserRole(userId, roleId, _clock.JstNow);
        await _userRoleRepository.CreateAsync(userRole);
    }
}
