using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Tests.Entities;

public class UserAuthSessionTests
{
    private static readonly LocalDateTime TestDateTime = new(new DateTime(2026, 9, 14, 10, 30, 0, DateTimeKind.Unspecified));
    private static readonly UserAuthSessionRowId TestSessionRowId = UserAuthSessionRowId.From(1);
    private static readonly AuthorityRowId TestAuthorityRowId = AuthorityRowId.From(100);
    private static readonly LoginCredentialsRowId TestLoginCredentialsRowId = LoginCredentialsRowId.From(5);

    [Fact]
    public void Create_WithLocalAuth_ReturnsValidSession()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            TestLoginCredentialsRowId);

        Assert.Equal(TestSessionRowId, session.RowId);
        Assert.Equal(TestAuthorityRowId, session.AuthorityRowId);
        Assert.False(session.IsAdAuthenticated);
        Assert.True(session.LoginSuccess);
        Assert.Equal(TestDateTime, session.LoggedInAt);
        Assert.Equal(TestLoginCredentialsRowId, session.LoginCredentialsRowId);
        Assert.Null(session.LoggedOutAt);
    }

    [Fact]
    public void Create_WithADAuth_ReturnsValidSession()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            loginCredentialsRowId: null);

        Assert.True(session.IsAdAuthenticated);
        Assert.Null(session.LoginCredentialsRowId);
    }

    [Fact]
    public void Create_WithLoginFailure_ReturnsSessionWithFailure()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: false,
            TestDateTime,
            TestLoginCredentialsRowId);

        Assert.False(session.LoginSuccess);
    }

    [Fact]
    public void Create_WithoutLoginCredentialsRowId_ReturnsSessionWithNull()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime);

        Assert.Null(session.LoginCredentialsRowId);
    }

    [Fact]
    public void Reconstruct_WithAllFields_ReturnsCompleteSession()
    {
        var logoutTime = new LocalDateTime(new DateTime(2026, 9, 14, 18, 0, 0, DateTimeKind.Unspecified));

        var session = UserAuthSession.Reconstruct(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            logoutTime,
            TestLoginCredentialsRowId);

        Assert.Equal(TestSessionRowId, session.RowId);
        Assert.Equal(TestAuthorityRowId, session.AuthorityRowId);
        Assert.False(session.IsAdAuthenticated);
        Assert.True(session.LoginSuccess);
        Assert.Equal(TestDateTime, session.LoggedInAt);
        Assert.Equal(logoutTime, session.LoggedOutAt);
        Assert.Equal(TestLoginCredentialsRowId, session.LoginCredentialsRowId);
    }

    [Fact]
    public void Reconstruct_WithoutLoggedOutAt_ReturnsSessionWithNull()
    {
        var session = UserAuthSession.Reconstruct(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            loggedOutAt: null,
            loginCredentialsRowId: null);

        Assert.Null(session.LoggedOutAt);
    }

    [Fact]
    public void SetLoggedOutAt_UpdatesLogoutTime()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime);

        var logoutTime = new LocalDateTime(new DateTime(2026, 9, 14, 17, 0, 0, DateTimeKind.Unspecified));
        session.SetLoggedOutAt(logoutTime);

        Assert.Equal(logoutTime, session.LoggedOutAt);
    }

    [Fact]
    public void IsActive_WithSuccessfulLoginAndNoLogout_ReturnsTrue()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime);

        Assert.True(session.IsActive());
    }

    [Fact]
    public void IsActive_WithFailedLogin_ReturnsFalse()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: false,
            TestDateTime);

        Assert.False(session.IsActive());
    }

    [Fact]
    public void IsActive_WithSuccessfulLoginButWithLogout_ReturnsFalse()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime);

        var logoutTime = new LocalDateTime(new DateTime(2026, 9, 14, 15, 0, 0, DateTimeKind.Unspecified));
        session.SetLoggedOutAt(logoutTime);

        Assert.False(session.IsActive());
    }

    [Fact]
    public void IsActive_WithReconstructedSessionWithLogout_ReturnsFalse()
    {
        var logoutTime = new LocalDateTime(new DateTime(2026, 9, 14, 16, 0, 0, DateTimeKind.Unspecified));
        var session = UserAuthSession.Reconstruct(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            logoutTime,
            TestLoginCredentialsRowId);

        Assert.False(session.IsActive());
    }

    [Fact]
    public void RowVersion_IsInitializedToEmptyArray()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime);

        Assert.NotNull(session.RowVersion);
        Assert.Empty(session.RowVersion);
    }

    [Fact]
    public void Create_WithNullRowId_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            UserAuthSession.Create(
                null!,
                TestAuthorityRowId,
                isAdAuthenticated: true,
                loginSuccess: true,
                TestDateTime));

        Assert.Contains("id", exception.Message);
    }

    [Fact]
    public void Create_WithNullAuthorityRowId_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            UserAuthSession.Create(
                TestSessionRowId,
                null!,
                isAdAuthenticated: true,
                loginSuccess: true,
                TestDateTime));

        Assert.Contains("authorityRowId", exception.Message);
    }

    [Fact]
    public void PropertiesAreImmutable_AfterCreation()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            TestLoginCredentialsRowId);

        // プロパティがprivate setを持つため、以下の値は変更不可
        Assert.False(session.IsAdAuthenticated);
        Assert.True(session.LoginSuccess);
        Assert.Equal(TestAuthorityRowId, session.AuthorityRowId);
        Assert.Equal(TestLoginCredentialsRowId, session.LoginCredentialsRowId);
        // SetLoggedOutAtのみ変更可能
    }

    [Fact]
    public void ADAndLocalAuthAreMutuallyExclusive()
    {
        // AD認証の場合、LoginCredentialsRowIdはnull
        var adSession = UserAuthSession.Create(
            UserAuthSessionRowId.From(2),
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            loginCredentialsRowId: null);

        // ローカル認証の場合、LoginCredentialsRowIdが設定
        var localSession = UserAuthSession.Create(
            UserAuthSessionRowId.From(3),
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            TestLoginCredentialsRowId);

        Assert.True(adSession.IsAdAuthenticated);
        Assert.Null(adSession.LoginCredentialsRowId);

        Assert.False(localSession.IsAdAuthenticated);
        Assert.NotNull(localSession.LoginCredentialsRowId);
    }
}
