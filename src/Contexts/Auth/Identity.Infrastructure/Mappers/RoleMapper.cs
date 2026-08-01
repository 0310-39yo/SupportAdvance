using System.Text.Json;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;

public class RoleMapper : IEntityMapper<Role, RoleDbModel, RowId>
{
    public RoleDbModel ToDbModel(Role entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var permissionsJson = JsonSerializer.Serialize(entity.Permissions);

        return new RoleDbModel
        {
            RowId = entity.Id.Value,
            Name = entity.Name,
            Description = entity.Description,
            PermissionsJson = permissionsJson,
        };
    }

    public Role ToDomainEntity(RoleDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var permissions = JsonSerializer.Deserialize<List<string>>(dbModel.PermissionsJson) ?? new List<string>();

        return new Role(
            name: dbModel.Name,
            permissions: permissions,
            description: dbModel.Description,
            rowId: RowId.From(dbModel.RowId));
    }
}
