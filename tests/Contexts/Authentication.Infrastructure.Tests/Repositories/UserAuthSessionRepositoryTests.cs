namespace SupportAdvance.Contexts.Authentication.Infrastructure.Tests.Repositories;

using System.Data;
using RepoDb;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.Contexts.Authentication.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Providers;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.Infrastructure.Tests.Utilities;
using Xunit;

/// <summary>
/// UserAuthSessionRepository の結合テスト（実DB接続）
///
/// 【接続先】既存の開発DB（3160EPOTAK / SupportAdvance、appsettings.Debug.json と同一）
/// 【注意】この Claude Code 実行環境からは当該DBへのネットワーク到達性がないため、
///         本ファイルは実行未確認の状態で作成されている。実行確認はユーザー環境（開発マシン）で行うこと。
/// 【テストデータ分離】RowId は s_test_row_id_sequence から採番し、既存データと衝突させない。
/// 【AuthorityRowId】FK制約（→ m_employees.row_id）を満たすため、実装済みの System User
///         （SystemCurrentUserService.SystemUserEmployeeRowId = 2147483667）を使用する。
/// 【クリーンアップ】IAsyncLifetime.DisposeAsync で、テスト中に作成した行を物理 DELETE する。
/// </summary>
public class UserAuthSessionRepositoryTests : RepositoryTestBase
{
    private const string ConnectionString =
        "Data Source=3160EPOTAK; Database=SupportAdvance; User ID=sa; Password=Misutamako4^; Encrypt=false";

    private readonly IClock _clock = new SystemClock();
    private IUserAuthSessionRepository _repository = null!;
    private TestSequenceProvider _testSequenceProvider = null!;

    public override Task InitializeAsync()
    {
        // RepoDb GlobalConfiguration 設定（SQL Server用）
        GlobalConfiguration
            .Setup()
            .UseSqlServer();

        // Dapper グローバル型マッピング設定（snake_case カラム ↔ PascalCase プロパティ変換に必須）
        SupportAdvance.Infrastructure.ORM.Dapper.DapperTypeHandlerRegistration.Register();

        var appSettings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string> { { "SupportAdvance", ConnectionString } },
            Database = new DatabaseSettings { Dialect = "SqlServer" }
        };

        _connectionFactory = new DbConnectionFactory(appSettings);
        _testSequenceProvider = new TestSequenceProvider(appSettings);
        var queryLoader = new SqlQueryLoader(appSettings);

        // 【注意】本番用 SequenceProvider（s_row_id_sequence）。
        //         テストでは事前に TestSequenceProvider で RowId を採番して渡すため、
        //         Repository.SaveAsync 内の採番ロジック（RowId==0 の場合のみ）は通常通らない。
        var sequenceProvider = new SequenceProvider(appSettings);

        _repository = new UserAuthSessionRepository(_connectionFactory, _clock, queryLoader, sequenceProvider);
        return Task.CompletedTask;
    }

    /// <summary>
    /// t_user_auth_sessions テーブルのクリーンアップ
    /// </summary>
    protected override async Task CleanupAsync(IDbConnection connection, long rowId)
    {
        await Task.Run(() =>
        {
            ExecuteNonQuery(connection, "DELETE FROM t_user_auth_sessions WHERE row_id = @rowId", rowId);
        });
    }

    /// <summary>
    /// テスト用の UserAuthSession を生成する（AD認証、LoginCredentialsRowId は null）
    /// 【重要】AuthorityRowId は FK制約を満たすため、実装済みの System User を使用
    /// </summary>
    private async Task<UserAuthSession> BuildTestSessionAsync(
        bool loginSuccess = true,
        LocalDateTime? loggedInAt = null)
    {
        var rowId = await _testSequenceProvider.GetNextValueAsync();
        return UserAuthSession.Create(
            UserAuthSessionRowId.From(rowId),
            AuthorityRowId.From(SystemCurrentUserService.SystemUserEmployeeRowId),
            isAdAuthenticated: true,
            loginSuccess: loginSuccess,
            loggedInAt: loggedInAt ?? _clock.JstNow,
            loginCredentialsRowId: null);
    }

    #region グループ 1: SaveAsync（新規作成）

    [Fact]
    public async Task VO_CRUD_01_SaveAsync_WithNewSession_InsertsSaveAsync_WithNewSessionInsertsSuccessfully()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();

            // Act
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            // Assert
            var retrieved = await _repository.GetByIdAsync(savedRowId);
            Assert.NotNull(retrieved);
            Assert.Equal(session.AuthorityRowId.Value, retrieved.AuthorityRowId.Value);
            Assert.True(retrieved.LoginSuccess);
            Assert.Null(retrieved.LoggedOutAt);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_AUDIT_01_SaveAsync_WithNewSession_SetsCratedAtAndBy()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();

            // Act
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            // Assert - CreatedAt/CreatedBy が設定されていることを直接SQLで確認（Entity には露出しないため）
            // 【重要】CreatedBy は session.AuthorityRowId（このセッションの主体）と一致すること
            var createdAt = QueryScalar<DateTime?>(savedRowId.Value, "created_at");
            var createdBy = QueryScalar<long?>(savedRowId.Value, "created_by");
            Assert.NotNull(createdAt);
            Assert.Equal(session.AuthorityRowId.Value, createdBy);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 2: GetByIdAsync

    [Fact]
    public async Task VO_CRUD_03_GetByIdAsync_WithValidId_ReturnsSession()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            // Act
            var result = await _repository.GetByIdAsync(savedRowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedRowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        try
        {
            // Act
            var result = await _repository.GetByIdAsync(UserAuthSessionRowId.From(9999999999));

            // Assert
            Assert.Null(result);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 3: GetLatestByAuthorityRowIdAsync

    [Fact]
    public async Task VO_QUERY_01_GetLatestByAuthorityRowIdAsync_WithValidAuthorityRowId_ReturnsLatestSession()
    {
        try
        {
            // Arrange: 10分前と現在の2セッションを作成
            var earlier = await BuildTestSessionAsync(loggedInAt: _clock.JstNow - TimeSpan.FromMinutes(10));
            var earlierRowId = await _repository.SaveAsync(earlier);
            _createdRowIds.Add(earlierRowId.Value);

            var later = await BuildTestSessionAsync(loggedInAt: _clock.JstNow);
            var laterRowId = await _repository.SaveAsync(later);
            _createdRowIds.Add(laterRowId.Value);

            // Act
            var result = await _repository.GetLatestByAuthorityRowIdAsync(
                AuthorityRowId.From(SystemCurrentUserService.SystemUserEmployeeRowId));

            // Assert - 最新（later）が返ること
            Assert.NotNull(result);
            Assert.Equal(laterRowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 4: UpdateAsync（ログアウト日時設定）

    [Fact]
    public async Task VO_CRUD_05_UpdateAsync_WithExistingSession_SetsLoggedOutAtSuccessfully()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            var loaded = await _repository.GetByIdAsync(savedRowId);
            Assert.NotNull(loaded);

            // Act
            loaded.SetLoggedOutAt(_clock.JstNow);
            await _repository.UpdateAsync(loaded);

            // Assert
            var result = await _repository.GetByIdAsync(savedRowId);
            Assert.NotNull(result);
            Assert.NotNull(result.LoggedOutAt);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_AUDIT_02_UpdateAsync_WithExistingSession_SetsUpdatedAtAndBy()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            var loaded = await _repository.GetByIdAsync(savedRowId);
            Assert.NotNull(loaded);

            // Act
            loaded.SetLoggedOutAt(_clock.JstNow);
            await _repository.UpdateAsync(loaded);

            // Assert - 【重要】UpdatedBy は session.AuthorityRowId（このセッションの主体）と一致すること
            var updatedAt = QueryScalar<DateTime?>(savedRowId.Value, "updated_at");
            var updatedBy = QueryScalar<long?>(savedRowId.Value, "updated_by");
            Assert.NotNull(updatedAt);
            Assert.Equal(loaded.AuthorityRowId.Value, updatedBy);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_ERROR_01_UpdateAsync_WithStaleRowVersion_ThrowsInvalidOperationException()
    {
        try
        {
            // Arrange: 同じセッションを2回取得（両方とも同じ RowVersion を保持）
            var session = await BuildTestSessionAsync();
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            var loaded1 = await _repository.GetByIdAsync(savedRowId);
            var loaded2 = await _repository.GetByIdAsync(savedRowId);
            Assert.NotNull(loaded1);
            Assert.NotNull(loaded2);

            // loaded1 を先に更新（RowVersion が更新される）
            loaded1.SetLoggedOutAt(_clock.JstNow);
            await _repository.UpdateAsync(loaded1);

            // Act & Assert: loaded2 は古い RowVersion のままなので更新失敗（楽観ロック競合）
            loaded2.SetLoggedOutAt(_clock.JstNow);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(loaded2));
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 5: DeleteAsync

    [Fact]
    public async Task VO_CRUD_06_DeleteAsync_WithValidId_SetsDeletedAtLogicallyDeletes()
    {
        try
        {
            // Arrange
            var session = await BuildTestSessionAsync();
            var savedRowId = await _repository.SaveAsync(session);
            _createdRowIds.Add(savedRowId.Value);

            // Act
            await _repository.DeleteAsync(savedRowId);

            // Assert - 論理削除されていることを直接SQLで確認
            // 【重要】DeletedBy は session.AuthorityRowId（このセッションの主体）と一致すること
            var deletedAt = QueryScalar<DateTime?>(savedRowId.Value, "deleted_at");
            var deletedBy = QueryScalar<long?>(savedRowId.Value, "deleted_by");
            Assert.NotNull(deletedAt);
            Assert.Equal(session.AuthorityRowId.Value, deletedBy);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region ヘルパーメソッド

    /// <summary>
    /// 指定した行・カラムの値を直接SQLで取得する（Entity に露出しない監査フィールド検証用）
    /// </summary>
    private T QueryScalar<T>(long rowId, string columnName)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT {columnName} FROM t_user_auth_sessions WHERE row_id = @rowId";
        AddParam(cmd, "@rowId", rowId);
        var value = cmd.ExecuteScalar();
        return value is null or DBNull ? default! : (T)value;
    }

    #endregion
}
