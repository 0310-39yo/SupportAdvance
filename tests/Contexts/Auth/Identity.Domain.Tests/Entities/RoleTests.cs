using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Tests.Entities;

public class RoleTests
{
    [Fact]
    public void Constructor_ShouldCreateRoleWithCorrectProperties()
    {
        var name = "Administrator";
        var permissions = new[] { "read", "write", "delete" };
        var description = "Administrator role";

        var role = new Role(name, permissions, description);

        Assert.Equal(name, role.Name);
        Assert.Equal(description, role.Description);
        Assert.Equal(3, role.Permissions.Count);
        Assert.True(role.Permissions.Contains("read"));
    }

    [Fact]
    public void Constructor_WithRowId_ShouldUseProvidedRowId()
    {
        var rowId = RowId.From(100);
        var role = new Role("Admin", new[] { "read" }, null, rowId);

        Assert.Equal(100, role.Id.Value);
    }

    [Fact]
    public void Constructor_WithoutRowId_ShouldCreateUnsetRowId()
    {
        var role = new Role("Admin", new[] { "read" });

        Assert.Equal(0, role.Id.Value);
    }

    [Fact]
    public void UpdatePermissions_ShouldReplacePermissions()
    {
        var role = new Role("User", new[] { "read" });
        var newPermissions = new[] { "read", "write" };

        role.UpdatePermissions(newPermissions);

        Assert.Equal(2, role.Permissions.Count);
        Assert.Contains("write", role.Permissions);
    }

    [Fact]
    public void Permissions_ShouldBeReadOnly()
    {
        var role = new Role("User", new[] { "read" });

        Assert.False(role.Permissions is List<string>);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Role(null!, new[] { "read" }));
    }

    [Fact]
    public void Constructor_WithNullPermissions_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Role("Admin", null!));
    }

    [Fact]
    public void UpdatePermissions_WithNull_ShouldThrow()
    {
        var role = new Role("Admin", new[] { "read" });

        Assert.Throws<ArgumentNullException>(() =>
            role.UpdatePermissions(null!));
    }

    [Fact]
    public void UpdatePermissions_ShouldCreateNewReadOnlyCollection()
    {
        var role = new Role("User", new[] { "read" });
        var oldPermissions = role.Permissions;

        role.UpdatePermissions(new[] { "write" });

        Assert.NotSame(oldPermissions, role.Permissions);
    }
}
