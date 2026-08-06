using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Master.Employee.Domain.Entities;

public class Department : AggregateRoot<DepartmentId>
{
    private RowId _rowId = null!;

    public RowId RowId => _rowId;
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public RowId? ParentDepartmentId { get; private set; }
    public bool IsActive { get; private set; }

    public Department(
        DepartmentId id,
        string name,
        string? description = null,
        RowId? parentDepartmentId = null,
        RowId? rowId = null,
        IClock? clock = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);

        Id = id;
        _rowId = rowId ?? RowId.New();
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
