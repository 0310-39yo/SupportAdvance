using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Domain.Entities;

public class Department : AggregateRoot<RowId>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public RowId? ParentDepartmentId { get; private set; }
    public bool IsActive { get; private set; }

    public Department(
        string name,
        string? description = null,
        RowId? parentDepartmentId = null,
        RowId? rowId = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(name);

        Id = rowId ?? RowId.New();
        Name = name;
        Description = description;
        ParentDepartmentId = parentDepartmentId;
        IsActive = true;
    }

    public void UpdateName(string newName)
    {
        ArgumentNullException.ThrowIfNull(newName);
        Name = newName;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
