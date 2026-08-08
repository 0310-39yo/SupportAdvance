using System.Text.Json;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.Mappers;

public class RoleMapper : IEntityMapper<Role, RoleDbModel, RoleId>
{
    public RoleDbModel ToDbModel(Role entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var permissionsJson = JsonSerializer.Serialize(entity.Permissions);

        return new RoleDbModel
        {
            RowId = entity.RowId.Value,
            RoleId = entity.Id.Value,
            Name = entity.Name,
            Description = entity.Description,
            PermissionsJson = permissionsJson,
            CreatedAt = default,
            CreatedBy = 0,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
        };
    }

    public Role ToDomainEntity(RoleDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var permissions = JsonSerializer.Deserialize<List<string>>(dbModel.PermissionsJson) ?? new List<string>();
        var roleId = RoleId.From(dbModel.RoleId);
        var rowId = RowId.From(dbModel.RowId);

        return new Role(
            id: roleId,
            name: dbModel.Name,
            permissions: permissions,
            description: dbModel.Description,
            rowId: rowId,
            clock: clock);
    }
}
