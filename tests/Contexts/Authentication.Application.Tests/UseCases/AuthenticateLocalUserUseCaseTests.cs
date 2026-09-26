using SupportAdvance.Application.Abstractions.Identifiers;
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
/// <para>【状態】認証成功時の応答の検証のみ。失敗系（ログインID なし・無効アカウント・パスワード不一致）は今後追加</para>
/// </remarks>
public class AuthenticateLocalUserUseCaseTests
{
    private const long SessionRowIdValue = 1001;
    private const long CredentialsRowId = 2002;
    private const long EmployeeRowId = 3003;
    private const string LoginId = "user01";
    private const string Password = "password";

    /// <summary>
    /// 認証成功の場合の応答の検証
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValidCredentials_ReturnsResponseWithClockNowAsLoggedInAt()
    {
        // Arrange
        var loggedInAt = new DateTime(2026, 9, 26, 10, 30, 0);
        using var clock = new MockClock(loggedInAt);
        var repository = new FakeSessionRepository();
        var useCase = new AuthenticateLocalUserUseCase(
            new FakeLoginCredentialsQuery(isActive: true),
            new FakePasswordHashService(matches: true),
            repository,
            clock,
            new FakeSequenceProvider());

        // Act
        var response = await useCase.ExecuteAsync(new AuthenticateLocalUserRequest { LoginId = LoginId, Password = Password });

        // Assert
        Assert.Equal(new LocalDateTime(loggedInAt), response.LoggedInAt);
        Assert.Equal(SessionRowIdValue, response.UserAuthSessionRowId);
        Assert.Equal(EmployeeRowId, response.EmployeeRowId);
        Assert.Equal(LoginId, response.LoginId);

        var saved = Assert.Single(repository.SavedSessions);
        Assert.True(saved.LoginSuccess);
        Assert.Equal(response.LoggedInAt, saved.LoggedInAt);
    }

    private sealed class FakeLoginCredentialsQuery(bool isActive) : ILoginCredentialsQuery
    {
        public Task<LoginCredentialsQueryResult?> GetByLoginIdAsync(string loginId) =>
            Task.FromResult<LoginCredentialsQueryResult?>(new LoginCredentialsQueryResult
            {
                RowId = CredentialsRowId,
                MappingEmployeeRowId = EmployeeRowId,
                LoginId = loginId,
                PasswordHash = "hash",
                IsActive = isActive
            });

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
}
