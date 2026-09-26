using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Dtos;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.Services;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using Xunit;

namespace SupportAdvance.Contexts.Authentication.Application.Tests.UseCases;

/// <summary>
/// <see cref="AuthenticateLocalUserUseCase"/> の単体テスト
/// </summary>
/// <remarks>
/// <para>【方針】DB を使わず、依存はすべてテスト内の偽実装で差し替える（Application 層の単体テスト）</para>
/// <para>【観点ID】docs/Contexts/Authentication/Application/Application_単体テスト仕様書.md の VO-EXEC／VO-RESULT／VO-ERROR に対応</para>
/// <para>【状態】仕様書の VO-EXEC-03（複数認証の独立したセッション）、VO-ERROR-04（期限切れ認証情報）は、現行の仕様・実装にない観点のため未実装</para>
/// </remarks>
public class AuthenticateLocalUserUseCaseTests
{
    private const long SessionRowIdValue = 1001;
    private const long CredentialsRowId = 2002;
    private const long EmployeeRowId = 3003;
    private const string LoginId = "user01";
    private const string Password = "password";

    private static readonly DateTime LoggedInAt = new(2026, 9, 26, 10, 30, 0);

    #region グループ 1: 正常系（VO-EXEC／VO-RESULT）

    /// <summary>
    /// 有効な認証情報で認証でき、成功のセッションが 1 件保存されることの検証
    /// </summary>
    [Fact]
    public async Task VO_EXEC_01_ExecuteAsync_ValidCredentials_SavesSuccessfulSession()
    {
        var (useCase, repository) = CreateUseCase();

        await useCase.ExecuteAsync(Request());

        var saved = Assert.Single(repository.SavedSessions);
        Assert.True(saved.LoginSuccess);
        Assert.False(saved.IsAdAuthenticated);
        Assert.Equal(EmployeeRowId, saved.AuthorityRowId.Value);
        Assert.True(saved.LoginCredentialsRowId.HasCredentials);
        Assert.Equal(CredentialsRowId, saved.LoginCredentialsRowId.Value);
        Assert.False(saved.LoggedOutAt.HasLoggedOut);
    }

    /// <summary>
    /// 応答にセッションの行ID が設定されることの検証
    /// </summary>
    [Fact]
    public async Task VO_RESULT_02_ExecuteAsync_ValidCredentials_ReturnsSessionRowId()
    {
        var (useCase, _) = CreateUseCase();

        var response = await useCase.ExecuteAsync(Request());

        Assert.Equal(SessionRowIdValue, response.UserAuthSessionRowId);
    }

    /// <summary>
    /// 応答の従業員行ID・ログインID・ログイン日時（クロックの現在時刻）が正しいことの検証
    /// </summary>
    [Fact]
    public async Task VO_RESULT_03_ExecuteAsync_ValidCredentials_ReturnsEmployeeLoginIdAndClockNowAsLoggedInAt()
    {
        var (useCase, repository) = CreateUseCase();

        var response = await useCase.ExecuteAsync(Request());

        Assert.Equal(EmployeeRowId, response.EmployeeRowId);
        Assert.Equal(LoginId, response.LoginId);
        Assert.Equal(new LocalDateTime(LoggedInAt), response.LoggedInAt);
        Assert.Equal(response.LoggedInAt, Assert.Single(repository.SavedSessions).LoggedInAt);
    }

    #endregion

    #region グループ 2: 異常系（VO-ERROR）

    /// <summary>
    /// パスワード不一致の場合に例外となり、失敗のセッションが記録されることの検証
    /// </summary>
    [Fact]
    public async Task VO_ERROR_01_ExecuteAsync_PasswordMismatch_ThrowsAndRecordsFailedSession()
    {
        var (useCase, repository) = CreateUseCase(passwordMatches: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(Request()));

        var saved = Assert.Single(repository.SavedSessions);
        Assert.False(saved.LoginSuccess);
        Assert.Equal(EmployeeRowId, saved.AuthorityRowId.Value);
        Assert.Equal(CredentialsRowId, saved.LoginCredentialsRowId.Value);
    }

    /// <summary>
    /// 存在しないログインID の場合に例外となり、「特定できない人物」の失敗セッションが記録されることの検証
    /// </summary>
    [Fact]
    public async Task VO_ERROR_02_ExecuteAsync_UnknownLoginId_ThrowsAndRecordsFailedSessionForUnknownUser()
    {
        var (useCase, repository) = CreateUseCase(credentialsExist: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(Request()));

        var saved = Assert.Single(repository.SavedSessions);
        Assert.False(saved.LoginSuccess);
        Assert.Equal(WellKnownIds.UnknownUserEmployeeRowId, saved.AuthorityRowId.Value);
        Assert.False(saved.LoginCredentialsRowId.HasCredentials);
    }

    /// <summary>
    /// 無効なアカウントの場合に例外となり、失敗のセッションが記録されることの検証
    /// </summary>
    [Fact]
    public async Task VO_ERROR_03_ExecuteAsync_InactiveAccount_ThrowsAndRecordsFailedSession()
    {
        var (useCase, repository) = CreateUseCase(isActive: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(Request()));

        var saved = Assert.Single(repository.SavedSessions);
        Assert.False(saved.LoginSuccess);
        Assert.Equal(EmployeeRowId, saved.AuthorityRowId.Value);
    }

    /// <summary>
    /// 入力が不正な場合に例外となり、何も保存されないことの検証
    /// </summary>
    [Theory]
    [InlineData("", Password)]
    [InlineData("   ", Password)]
    [InlineData(LoginId, "")]
    [InlineData(LoginId, "   ")]
    public async Task VO_ERROR_05_ExecuteAsync_EmptyLoginIdOrPassword_ThrowsArgumentExceptionWithoutSaving(string loginId, string password)
    {
        var (useCase, repository) = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new AuthenticateLocalUserRequest { LoginId = loginId, Password = password }));

        Assert.Empty(repository.SavedSessions);
    }

    /// <summary>
    /// ログインID が 50 文字を超える場合に例外となり、何も保存されないことの検証
    /// </summary>
    [Fact]
    public async Task VO_ERROR_06_ExecuteAsync_TooLongLoginId_ThrowsArgumentExceptionWithoutSaving()
    {
        var (useCase, repository) = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new AuthenticateLocalUserRequest { LoginId = new string('a', 51), Password = Password }));

        Assert.Empty(repository.SavedSessions);
    }

    #endregion

    #region ヘルパーと偽実装

    private static AuthenticateLocalUserRequest Request() => new() { LoginId = LoginId, Password = Password };

    private static (AuthenticateLocalUserUseCase UseCase, FakeSessionRepository Repository) CreateUseCase(
        bool credentialsExist = true,
        bool isActive = true,
        bool passwordMatches = true)
    {
        var repository = new FakeSessionRepository();
        var useCase = new AuthenticateLocalUserUseCase(
            new FakeLoginCredentialsQuery(credentialsExist, isActive),
            new FakePasswordHashService(passwordMatches),
            repository,
            new MockClock(LoggedInAt),
            new FakeSequenceProvider());
        return (useCase, repository);
    }

    private sealed class FakeLoginCredentialsQuery(bool exists, bool isActive) : ILoginCredentialsQuery
    {
        public Task<LoginCredentialsQueryResult?> GetByLoginIdAsync(string loginId) =>
            Task.FromResult<LoginCredentialsQueryResult?>(exists
                ? new LoginCredentialsQueryResult
                {
                    RowId = CredentialsRowId,
                    MappingEmployeeRowId = EmployeeRowId,
                    LoginId = loginId,
                    PasswordHash = "hash",
                    IsActive = isActive
                }
                : null);

        public Task<LoginCredentialsQueryResult?> GetByRowIdAsync(long rowId) =>
            throw new NotSupportedException();
    }

    private sealed class FakePasswordHashService(bool matches) : IPasswordHashService
    {
        public bool VerifyPassword(string plainPassword, string passwordHash) => matches;

        public string HashPassword(string plainPassword) => throw new NotSupportedException();
    }

    private sealed class FakeSequenceProvider : ISequenceProvider
    {
        public Task<long> GetNextValueAsync() => Task.FromResult(SessionRowIdValue);

        public Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1) => throw new NotSupportedException();
    }

    private sealed class FakeSessionRepository : IUserAuthSessionRepository
    {
        public List<UserAuthSession> SavedSessions { get; } = [];

        public Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session)
        {
            SavedSessions.Add(session);
            return Task.FromResult(session.RowId);
        }

        public Task<UserAuthSession?> GetByIdAsync(UserAuthSessionRowId id) => throw new NotSupportedException();

        public Task<UserAuthSession?> GetLatestByAuthorityRowIdAsync(AuthorityRowId authorityRowId) =>
            throw new NotSupportedException();

        public Task<UserAuthSession?> GetLatestByLoginCredentialsRowIdAsync(LoginCredentialsRowId loginCredentialsRowId) =>
            throw new NotSupportedException();

        public Task UpdateAsync(UserAuthSession session) => throw new NotSupportedException();

        public Task DeleteAsync(UserAuthSessionRowId id) => throw new NotSupportedException();
    }

    #endregion
}
