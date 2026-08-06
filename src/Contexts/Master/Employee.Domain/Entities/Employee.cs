using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Master.Employee.Domain.Entities;

public class Employee : AggregateRoot<EmployeeId>
{
    private RowId _rowId = null!;

    public RowId RowId => _rowId;
    public string EmployeeNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public RowId DepartmentId { get; private set; }
    public string JobTitle { get; private set; }
    public bool IsActive { get; private set; }
    public LocalDateTime? HireDate { get; private set; }

    public Employee(
        EmployeeId id,
        string employeeNumber,
        string firstName,
        string lastName,
        string email,
        RowId departmentId,
        string jobTitle,
        LocalDateTime? hireDate = null,
        RowId? rowId = null,
        IClock? clock = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(employeeNumber);
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(departmentId);
        ArgumentNullException.ThrowIfNull(jobTitle);

        Id = id;
        _rowId = rowId ?? RowId.New();
        EmployeeNumber = employeeNumber;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DepartmentId = departmentId;
        JobTitle = jobTitle;
        HireDate = hireDate;
        IsActive = true;
    }

    public void TransferDepartment(RowId newDepartmentId)
    {
        ArgumentNullException.ThrowIfNull(newDepartmentId);
        DepartmentId = newDepartmentId;
    }

    public void ChangeJobTitle(string newJobTitle)
    {
        ArgumentNullException.ThrowIfNull(newJobTitle);
        JobTitle = newJobTitle;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
