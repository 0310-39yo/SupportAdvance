using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Entities;

public class Role : AggregateRoot<RoleId>
{
    private RowId _rowId = null!;

    public RowId RowId => _rowId;
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Permissions { get; private set; }

    public Role(
        RoleId id,
        string name,
        IEnumerable<string> permissions,
        string? description = null,
        RowId? rowId = null,
        IClock? clock = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(permissions);

        Id = id;
        _rowId = rowId ?? RowId.New();
        Name = name;
        Description = description;
        Permissions = permissions.ToList().AsReadOnly();
    }

    public void UpdatePermissions(IEnumerable<string> newPermissions)
    {
        ArgumentNullException.ThrowIfNull(newPermissions);
        Permissions = newPermissions.ToList().AsReadOnly();
    }
}
