namespace SupportAdvance.Contexts.Authentication.Infrastructure.Tests.Mappers;

using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.Contexts.Authentication.Infrastructure.DbModels;
using SupportAdvance.Contexts.Authentication.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Common.Clocks;
using Xunit;

/// <summary>
/// UserAuthSessionMapper の単体テスト（DB非依存の純粋ロジック）
/// </summary>
public class UserAuthSessionMapperTests
{
    #region グループ 1: ToDbModel - 正常系

    [Fact]
    public void VO_MAP_01_ToDbModel_WithValidEntity_MapsAllFields()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var sessionRowId = UserAuthSessionRowId.From(1L);
        var authorityRowId = AuthorityRowId.From(100L);
        var loggedInAt = new LocalDateTime(new DateTime(2026, 9, 16, 10, 0, 0));

        var entity = UserAuthSession.Create(
            sessionRowId,
            authorityRowId,
            isAdAuthenticated: false,
            loginSuccess: true,
            loggedInAt,
            UsedLoginCredentialsRowId.Unset()
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.RowId);
        Assert.Equal(100L, result.CurrentUserRowId);
        Assert.False(result.IsAdAuthenticated);
        Assert.True(result.LoginSuccess);
        Assert.Equal(loggedInAt.Value, result.LoggedInAt);
    }

    [Fact]
    public void VO_MAP_02_ToDbModel_WithLoggedOutSession_SetsLogoutAt()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var sessionRowId = UserAuthSessionRowId.From(2L);
        var authorityRowId = AuthorityRowId.From(101L);
        var loggedInAt = new LocalDateTime(new DateTime(2026, 9, 16, 10, 0, 0));
        var loggedOutAt = new LocalDateTime(new DateTime(2026, 9, 16, 11, 0, 0));

        var entity = UserAuthSession.Create(
            sessionRowId,
            authorityRowId,
            isAdAuthenticated: true,
            loginSuccess: true,
            loggedInAt,
            UsedLoginCredentialsRowId.Unset()
        );
        entity.SetLoggedOutAt(loggedOutAt);

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.LoggedOutAt);
        Assert.Equal(loggedOutAt.Value, result.LoggedOutAt.Value);
    }

    #endregion

    #region グループ 2: ToDomainEntity - 正常系

    [Fact]
    public void VO_MAP_03_ToDomainEntity_WithValidDbModel_MapsAllFields()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var loggedInDateTime = new DateTime(2026, 9, 16, 10, 0, 0);
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = 1L,
            CurrentUserRowId = 100L,
            IsAdAuthenticated = false,
            LoginSuccess = true,
            LoggedInAt = loggedInDateTime,
            LoggedOutAt = null,
            LoginCredentialsRowId = null,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.RowId.Value);
        Assert.Equal(100L, result.AuthorityRowId.Value);
        Assert.False(result.IsAdAuthenticated);
        Assert.True(result.LoginSuccess);
    }

    [Fact]
    public void VO_MAP_04_ToDomainEntity_WithNullLoggedOutAtAndCredentials_ConvertsToUnset()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = 1L,
            CurrentUserRowId = 100L,
            IsAdAuthenticated = true,
            LoginSuccess = true,
            LoggedInAt = new DateTime(2026, 9, 16, 10, 0, 0),
            LoggedOutAt = null,
            LoginCredentialsRowId = null,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result.LoggedOutAt);
        Assert.False(result.LoggedOutAt.HasLoggedOut);
        Assert.NotNull(result.LoginCredentialsRowId);
        Assert.False(result.LoginCredentialsRowId.HasCredentials);
    }

    [Fact]
    public void VO_MAP_05_ToDomainEntity_WithLoggedOutAtAndCredentials_ConvertsToSetValues()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var loggedOutDateTime = new DateTime(2026, 9, 16, 11, 0, 0);
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = 2L,
            CurrentUserRowId = 101L,
            IsAdAuthenticated = false,
            LoginSuccess = true,
            LoggedInAt = new DateTime(2026, 9, 16, 10, 0, 0),
            LoggedOutAt = loggedOutDateTime,
            LoginCredentialsRowId = 55L,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.True(result.LoggedOutAt.HasLoggedOut);
        Assert.Equal(loggedOutDateTime, result.LoggedOutAt.Value.Value);
        Assert.True(result.LoginCredentialsRowId.HasCredentials);
        Assert.Equal(55L, result.LoginCredentialsRowId.Value);
    }

    [Fact]
    public void VO_MAP_06_ToDomainEntity_WithInvalidCredentialsRowId_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = 1L,
            CurrentUserRowId = 100L,
            IsAdAuthenticated = false,
            LoginSuccess = true,
            LoggedInAt = new DateTime(2026, 9, 16, 10, 0, 0),
            LoggedOutAt = null,
            LoginCredentialsRowId = 0L,
            RowVersion = []
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
    }

    [Fact]
    public void VO_MAP_07_ToDbModel_WithUnsetValues_ConvertsToNull()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var entity = UserAuthSession.Create(
            UserAuthSessionRowId.From(3L),
            AuthorityRowId.From(102L),
            isAdAuthenticated: true,
            loginSuccess: true,
            new LocalDateTime(new DateTime(2026, 9, 16, 10, 0, 0)),
            UsedLoginCredentialsRowId.Unset());

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.Null(result.LoggedOutAt);
        Assert.Null(result.LoginCredentialsRowId);
    }

    [Fact]
    public void VO_MAP_08_ToDbModel_WithCredentialsRowId_ConvertsToValue()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var entity = UserAuthSession.Create(
            UserAuthSessionRowId.From(4L),
            AuthorityRowId.From(103L),
            isAdAuthenticated: false,
            loginSuccess: true,
            new LocalDateTime(new DateTime(2026, 9, 16, 10, 0, 0)),
            UsedLoginCredentialsRowId.From(77L));

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.Equal(77L, result.LoginCredentialsRowId);
    }
    #endregion

    #region グループ 3: DateTime変換

    [Fact]
    public void VO_TYPE_01_DateTimeConversion_LocalDateTimeRoundTrip()
    {
        // Arrange
        var mapper = new UserAuthSessionMapper();
        var originalDateTime = new DateTime(2026, 9, 16, 10, 30, 45);
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = 1L,
            CurrentUserRowId = 100L,
            IsAdAuthenticated = false,
            LoginSuccess = true,
            LoggedInAt = originalDateTime,
            LoggedOutAt = null,
            LoginCredentialsRowId = null,
            RowVersion = []
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel);
        var resultDbModel = mapper.ToDbModel(entity);

        // Assert
        Assert.Equal(originalDateTime, resultDbModel.LoggedInAt);
    }

    #endregion
}
