using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Contexts.Master.Employee.Domain.Tests.Entities;

public class DepartmentTests
{
    [Fact]
    public void Constructor_ShouldCreateDepartmentWithCorrectProperties()
    {
        var name = "Engineering";
        var description = "Engineering Department";

        var department = new Department(name, description);

        Assert.Equal(name, department.Name);
        Assert.Equal(description, department.Description);
        Assert.True(department.IsActive);
        Assert.Null(department.ParentDepartmentId);
        Assert.Equal(0, department.Id.Value);
    }

    [Fact]
    public void Constructor_WithParentDepartmentId_ShouldSetParentDepartmentId()
    {
        var parentDepartmentId = RowId.From(1);
        var department = new Department("Sub Department", null, parentDepartmentId);

        Assert.Equal(parentDepartmentId, department.ParentDepartmentId);
    }

    [Fact]
    public void Constructor_WithRowId_ShouldUseProvidedRowId()
    {
        var rowId = RowId.From(25);
        var department = new Department("Engineering", null, null, rowId);

        Assert.Equal(25, department.Id.Value);
    }

    [Fact]
    public void UpdateName_ShouldChangeName()
    {
        var department = new Department("Engineering");
        var newName = "Research & Development";

        department.UpdateName(newName);

        Assert.Equal(newName, department.Name);
    }

    [Fact]
    public void SetActive_ShouldChangeIsActiveFlag()
    {
        var department = new Department("Engineering");
        Assert.True(department.IsActive);

        department.SetActive(false);
        Assert.False(department.IsActive);

        department.SetActive(true);
        Assert.True(department.IsActive);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Department(null!));
    }

    [Fact]
    public void UpdateName_WithNullName_ShouldThrow()
    {
        var department = new Department("Engineering");

        Assert.Throws<ArgumentNullException>(() =>
            department.UpdateName(null!));
    }

    [Fact]
    public void Constructor_WithoutRowId_ShouldCreateUnsetRowId()
    {
        var department = new Department("Engineering");

        Assert.Equal(0, department.Id.Value);
    }

    [Fact]
    public void Constructor_AllowsNullDescription()
    {
        var department = new Department("Engineering", null);

        Assert.Null(department.Description);
    }

    [Fact]
    public void Constructor_AllowsNullParentDepartmentId()
    {
        var department = new Department("Engineering", "Desc", null);

        Assert.Null(department.ParentDepartmentId);
    }
}

