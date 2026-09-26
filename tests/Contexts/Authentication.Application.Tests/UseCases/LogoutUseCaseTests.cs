using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using Xunit;

namespace SupportAdvance.Contexts.Authentication.Application.Tests.UseCases;

/// <summary>
/// <see cref="LogoutUseCase"/> の単体テスト
/// </summary>
/// <remarks>
/// <para>【方針】DB を使わず、リポジトリを偽実装で差し替える（Application 層の単体テスト）</para>
/// <para>【観点ID】docs/Contexts/Authentication/Application/Application_単体テスト仕様書.md の VO-EXEC／VO-STATE に対応</para>
/// <para>【状態】仕様書の VO-EXEC-03（ログアウト済みセッションで例外）は、現行の実装が例外を送出しないため未実装（仕様書に差異を記載）</para>
/// </remarks>
public class LogoutUseCaseTests
{
    private static readonly DateTime LogoutAt = new(2026, 9, 26, 18, 0, 0);
    private static readonly LocalDateTime LoggedInAt = new(new DateTime(2026, 9, 26, 9, 0, 0));
    private static readonly UserAuthSessionRowId SessionRowId = UserAuthSessionRowId.From(10);

    /// <summary>
    /// 有効なセッションのログアウトで、セッションが更新されることの検証
    /// </summary>
    [Fact]
    public async Task VO_EXEC_01_ExecuteAsync_ExistingSession_UpdatesSession()
    {
        var repository = new FakeSessionRepository(CreateSession());
        var useCase = new LogoutUseCase(repository, new MockClock(LogoutAt));

        await useCase.ExecuteAsync(SessionRowId);

        var updated = Assert.Single(repository.UpdatedSessions);
        Assert.Equal(SessionRowId, updated.RowId);
    }

    /// <summary>
    /// 存在しないセッションで例外となり、更新されないことの検証
    /// </summary>
    [Fact]
    public async Task VO_EXEC_02_ExecuteAsync_MissingSession_ThrowsInvalidOperationExceptionWithoutUpdate()
    {
        var repository = new FakeSessionRepository(null);
        var useCase = new LogoutUseCase(repository, new MockClock(LogoutAt));

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(SessionRowId));

        Assert.Empty(repository.UpdatedSessions);
    }

    /// <summary>
    /// ログアウト日時にクロックの現在時刻が設定され、セッションが有効でなくなることの検証
    /// </summary>
    [Fact]
    public async Task VO_STATE_01_ExecuteAsync_ExistingSession_SetsLoggedOutAtFromClock()
    {
        var session = CreateSession();
        var repository = new FakeSessionRepository(session);
        var useCase = new LogoutUseCase(repository, new MockClock(LogoutAt));

        await useCase.ExecuteAsync(SessionRowId);

        Assert.True(session.LoggedOutAt.HasLoggedOut);
        Assert.Equal(new LocalDateTime(LogoutAt), session.LoggedOutAt.Value);
        Assert.False(session.IsActive());
    }

    /// <summary>
    /// セッションの行ID が <see langword="null"/> の場合の例外の検証
    /// </summary>
    [Fact]
    public async Task VO_ERROR_01_ExecuteAsync_NullSessionRowId_ThrowsArgumentNullException()
    {
        var useCase = new LogoutUseCase(new FakeSessionRepository(null), new MockClock(LogoutAt));

        await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(null!));
    }

    private static UserAuthSession CreateSession() =>
        UserAuthSession.Create(
            SessionRowId,
            AuthorityRowId.From(100),
            isAdAuthenticated: false,
            loginSuccess: true,
            LoggedInAt,
            UsedLoginCredentialsRowId.From(5));

    private sealed class FakeSessionRepository(UserAuthSession? session) : IUserAuthSessionRepository
    {
        public List<UserAuthSession> UpdatedSessions { get; } = [];

        public Task<UserAuthSession?> GetByIdAsync(UserAuthSessionRowId id) => Task.FromResult(session);

        public Task UpdateAsync(UserAuthSession updated)
        {
            UpdatedSessions.Add(updated);
            return Task.CompletedTask;
        }

        public Task<UserAuthSession?> GetLatestByAuthorityRowIdAsync(AuthorityRowId authorityRowId) =>
            throw new NotSupportedException();

        public Task<UserAuthSession?> GetLatestByLoginCredentialsRowIdAsync(LoginCredentialsRowId loginCredentialsRowId) =>
            throw new NotSupportedException();

        public Task<UserAuthSessionRowId> SaveAsync(UserAuthSession created) => throw new NotSupportedException();

        public Task DeleteAsync(UserAuthSessionRowId id) => throw new NotSupportedException();
    }
}
