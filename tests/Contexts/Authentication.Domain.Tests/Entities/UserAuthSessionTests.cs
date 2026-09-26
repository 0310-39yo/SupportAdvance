using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Domain.Tests.Entities;

public class UserAuthSessionTests
{
    private static readonly LocalDateTime TestDateTime = new(new DateTime(2026, 9, 14, 10, 30, 0, DateTimeKind.Unspecified));
    private static readonly UserAuthSessionRowId TestSessionRowId = UserAuthSessionRowId.From(1);
    private static readonly AuthorityRowId TestAuthorityRowId = AuthorityRowId.From(100);
    private static readonly UsedLoginCredentialsRowId TestLoginCredentialsRowId = UsedLoginCredentialsRowId.From(5);
    private static readonly UsedLoginCredentialsRowId NoCredentials = UsedLoginCredentialsRowId.Unset();

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
        Assert.True(session.LoginCredentialsRowId.HasCredentials);
        Assert.False(session.LoggedOutAt.HasLoggedOut);
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
            NoCredentials);

        Assert.True(session.IsAdAuthenticated);
        Assert.False(session.LoginCredentialsRowId.HasCredentials);
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
    public void Create_WithoutCredentials_ReturnsSessionWithUnsetCredentialsRowId()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

        Assert.NotNull(session.LoginCredentialsRowId);
        Assert.False(session.LoginCredentialsRowId.HasCredentials);
    }

    [Fact]
    public void Create_InitialLoggedOutAt_IsUnset()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

        Assert.NotNull(session.LoggedOutAt);
        Assert.False(session.LoggedOutAt.HasLoggedOut);
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
            LoggedOutAt.From(logoutTime),
            TestLoginCredentialsRowId);

        Assert.Equal(TestSessionRowId, session.RowId);
        Assert.Equal(TestAuthorityRowId, session.AuthorityRowId);
        Assert.False(session.IsAdAuthenticated);
        Assert.True(session.LoginSuccess);
        Assert.Equal(TestDateTime, session.LoggedInAt);
        Assert.True(session.LoggedOutAt.HasLoggedOut);
        Assert.Equal(logoutTime, session.LoggedOutAt.Value);
        Assert.Equal(TestLoginCredentialsRowId, session.LoginCredentialsRowId);
    }

    [Fact]
    public void Reconstruct_WithUnsetLoggedOutAt_ReturnsSessionWithoutLogout()
    {
        var session = UserAuthSession.Reconstruct(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            LoggedOutAt.Unset(),
            NoCredentials);

        Assert.False(session.LoggedOutAt.HasLoggedOut);
    }

    [Fact]
    public void Reconstruct_WithNullLoggedOutAt_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            UserAuthSession.Reconstruct(
                TestSessionRowId,
                TestAuthorityRowId,
                isAdAuthenticated: true,
                loginSuccess: true,
                TestDateTime,
                loggedOutAt: null!,
                NoCredentials));

        Assert.Contains("loggedOutAt", exception.Message);
    }

    [Fact]
    public void Reconstruct_WithNullCredentialsRowId_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            UserAuthSession.Reconstruct(
                TestSessionRowId,
                TestAuthorityRowId,
                isAdAuthenticated: true,
                loginSuccess: true,
                TestDateTime,
                LoggedOutAt.Unset(),
                loginCredentialsRowId: null!));

        Assert.Contains("loginCredentialsRowId", exception.Message);
    }

    [Fact]
    public void SetLoggedOutAt_UpdatesLogoutTime()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

        var logoutTime = new LocalDateTime(new DateTime(2026, 9, 14, 17, 0, 0, DateTimeKind.Unspecified));
        session.SetLoggedOutAt(logoutTime);

        Assert.True(session.LoggedOutAt.HasLoggedOut);
        Assert.Equal(logoutTime, session.LoggedOutAt.Value);
    }

    [Fact]
    public void SetLoggedOutAt_WithMinValue_ThrowsArgumentException()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

        Assert.Throws<ArgumentException>(() => session.SetLoggedOutAt(LocalDateTime.MinValue));
        Assert.False(session.LoggedOutAt.HasLoggedOut);
    }

    [Fact]
    public void IsActive_WithSuccessfulLoginAndNoLogout_ReturnsTrue()
    {
        var session = UserAuthSession.Create(
            TestSessionRowId,
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

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
            TestDateTime,
            NoCredentials);

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
            TestDateTime,
            NoCredentials);

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
            LoggedOutAt.From(logoutTime),
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
            TestDateTime,
            NoCredentials);

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
                TestDateTime,
                NoCredentials));

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
                TestDateTime,
                NoCredentials));

        Assert.Contains("authorityRowId", exception.Message);
    }

    [Fact]
    public void Create_WithNullCredentialsRowId_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            UserAuthSession.Create(
                TestSessionRowId,
                TestAuthorityRowId,
                isAdAuthenticated: true,
                loginSuccess: true,
                TestDateTime,
                loginCredentialsRowId: null!));

        Assert.Contains("loginCredentialsRowId", exception.Message);
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
        // AD認証の場合、認証情報の行IDは未設定（Unset）
        var adSession = UserAuthSession.Create(
            UserAuthSessionRowId.From(2),
            TestAuthorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            TestDateTime,
            NoCredentials);

        // ローカル認証の場合、認証情報の行IDが設定
        var localSession = UserAuthSession.Create(
            UserAuthSessionRowId.From(3),
            TestAuthorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            TestDateTime,
            TestLoginCredentialsRowId);

        Assert.True(adSession.IsAdAuthenticated);
        Assert.False(adSession.LoginCredentialsRowId.HasCredentials);

        Assert.False(localSession.IsAdAuthenticated);
        Assert.True(localSession.LoginCredentialsRowId.HasCredentials);
    }
}
