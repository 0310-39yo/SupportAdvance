using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUserWithCorrectProperties()
    {
        var loginId = "testuser";
        var email = "test@example.com";
        var hashedPassword = "hashed_password_12345";
        var displayName = "Test User";

        var user = new User(loginId, email, hashedPassword, displayName);

        Assert.Equal(loginId, user.LoginId);
        Assert.Equal(email, user.Email);
        Assert.Equal(hashedPassword, user.HashedPassword);
        Assert.Equal(displayName, user.DisplayName);
        Assert.True(user.IsActive);
        Assert.Equal(0, user.Id.Value);
    }

    [Fact]
    public void Constructor_WithRowId_ShouldUseProvidedRowId()
    {
        var rowId = RowId.From(42);
        var user = new User("test", "test@example.com", "pwd", null, rowId);

        Assert.Equal(42, user.Id.Value);
    }

    [Fact]
    public void Constructor_WithoutRowId_ShouldCreateUnsetRowId()
    {
        var user = new User("test", "test@example.com", "pwd");

        Assert.Equal(0, user.Id.Value);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHashedPassword()
    {
        var user = new User("test", "test@example.com", "old_password");
        var newPassword = "new_hashed_password";

        user.ChangePassword(newPassword);

        Assert.Equal(newPassword, user.HashedPassword);
    }

    [Fact]
    public void ChangePassword_WithNullPassword_ShouldThrow()
    {
        var user = new User("test", "test@example.com", "password");

        Assert.Throws<ArgumentNullException>(() => user.ChangePassword(null!));
    }

    [Fact]
    public void SetActive_ShouldChangeIsActiveFlag()
    {
        var user = new User("test", "test@example.com", "password");
        Assert.True(user.IsActive);

        user.SetActive(false);
        Assert.False(user.IsActive);

        user.SetActive(true);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void UpdateEmail_ShouldChangeEmail()
    {
        var user = new User("test", "old@example.com", "password");
        var newEmail = "new@example.com";

        user.UpdateEmail(newEmail);

        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public void UpdateEmail_WithNullEmail_ShouldThrow()
    {
        var user = new User("test", "test@example.com", "password");

        Assert.Throws<ArgumentNullException>(() => user.UpdateEmail(null!));
    }

    [Fact]
    public void Constructor_WithNullLoginId_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new User(null!, "test@example.com", "password"));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new User("test", null!, "password"));
    }

    [Fact]
    public void Constructor_WithNullPassword_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new User("test", "test@example.com", null!));
    }
}

