using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Domain.Entities;

public class Role : AggregateRoot<RowId>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Permissions { get; private set; }

    public Role(
        string name,
        IEnumerable<string> permissions,
        string? description = null,
        RowId? rowId = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(permissions);

        Id = rowId ?? RowId.New();
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
