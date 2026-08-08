using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Domain.Tests.Entities;

public class EmployeeTests
{
    [Fact]
    public void Constructor_ShouldCreateEmployeeWithCorrectProperties()
    {
        var employeeNumber = "EMP001";
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var departmentId = RowId.From(1);
        var jobTitle = "Engineer";

        var employee = new EmployeeEntity(employeeNumber, firstName, lastName, email, departmentId, jobTitle);

        Assert.Equal(employeeNumber, employee.EmployeeNumber);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email, employee.Email);
        Assert.Equal(departmentId, employee.DepartmentId);
        Assert.Equal(jobTitle, employee.JobTitle);
        Assert.True(employee.IsActive);
        Assert.Equal(0, employee.Id.Value);
    }

    [Fact]
    public void Constructor_WithRowId_ShouldUseProvidedRowId()
    {
        var rowId = RowId.From(50);
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer", null, rowId);

        Assert.Equal(50, employee.Id.Value);
    }

    [Fact]
    public void Constructor_WithHireDate_ShouldSetHireDate()
    {
        var hireDate = new LocalDateTime(new DateTime(2024, 1, 1));
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer", hireDate);

        Assert.Equal(hireDate, employee.HireDate);
    }

    [Fact]
    public void TransferDepartment_ShouldChangeDepartmentId()
    {
        var oldDepartmentId = RowId.From(1);
        var newDepartmentId = RowId.From(2);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", oldDepartmentId, "Engineer");

        employee.TransferDepartment(newDepartmentId);

        Assert.Equal(newDepartmentId, employee.DepartmentId);
    }

    [Fact]
    public void ChangeJobTitle_ShouldUpdateJobTitle()
    {
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer");
        var newJobTitle = "Senior Engineer";

        employee.ChangeJobTitle(newJobTitle);

        Assert.Equal(newJobTitle, employee.JobTitle);
    }

    [Fact]
    public void SetActive_ShouldChangeIsActiveFlag()
    {
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer");
        Assert.True(employee.IsActive);

        employee.SetActive(false);
        Assert.False(employee.IsActive);

        employee.SetActive(true);
        Assert.True(employee.IsActive);
    }

    [Fact]
    public void Constructor_WithNullEmployeeNumber_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        Assert.Throws<ArgumentNullException>(() =>
            new EmployeeEntity(null!, "John", "Doe", "john@example.com", departmentId, "Engineer"));
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        Assert.Throws<ArgumentNullException>(() =>
            new EmployeeEntity("EMP001", null!, "Doe", "john@example.com", departmentId, "Engineer"));
    }

    [Fact]
    public void Constructor_WithNullLastName_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        Assert.Throws<ArgumentNullException>(() =>
            new EmployeeEntity("EMP001", "John", null!, "john@example.com", departmentId, "Engineer"));
    }

    [Fact]
    public void TransferDepartment_WithNullDepartmentId_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer");

        Assert.Throws<ArgumentNullException>(() =>
            employee.TransferDepartment(null!));
    }

    [Fact]
    public void ChangeJobTitle_WithNullJobTitle_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        var employee = new EmployeeEntity("EMP001", "John", "Doe", "john@example.com", departmentId, "Engineer");

        Assert.Throws<ArgumentNullException>(() =>
            employee.ChangeJobTitle(null!));
    }
}
