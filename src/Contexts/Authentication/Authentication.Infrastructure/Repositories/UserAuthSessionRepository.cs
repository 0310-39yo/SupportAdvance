using System.Linq;
using Dapper;
using RepoDb;
using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.Contexts.Authentication.Infrastructure.DbModels;
using SupportAdvance.Contexts.Authentication.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Repositories;

/// <summary>
/// UserAuthSession Repository 実装
///
/// 【責務】
/// - UserAuthSession 集約の永続化
/// - SELECT: Dapper + SQL ファイル
/// - INSERT/UPDATE/DELETE: RepoDb Entity-based API
/// - 監査フィールド（CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy）の管理
///   【重要】CreatedBy/UpdatedBy/DeletedBy には session.AuthorityRowId（このセッションの主体）を使用。
///           ログイン試行中は ICurrentUserService が未設定の場合があるため採用しない
///
/// 【パターン】
/// - GetByIdAsync: Dapper + SQL
/// - GetLatestByAuthorityRowIdAsync: Dapper + SQL
/// - GetLatestByLoginCredentialsRowIdAsync: Dapper + SQL
/// - SaveAsync: RepoDb Insert
/// - UpdateAsync: RepoDb Update（楽観ロック付き）
/// - DeleteAsync: RepoDb Update（論理削除）
/// </summary>
public class UserAuthSessionRepository : IUserAuthSessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IClock _clock;
    private readonly SqlQueryLoader _sqlQueryLoader;
    private readonly ISequenceProvider _sequenceProvider;
    private readonly UserAuthSessionMapper _mapper;

    public UserAuthSessionRepository(
        IDbConnectionFactory connectionFactory,
        IClock clock,
        SqlQueryLoader sqlQueryLoader,
        ISequenceProvider sequenceProvider)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sqlQueryLoader = sqlQueryLoader ?? throw new ArgumentNullException(nameof(sqlQueryLoader));
        _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
        _mapper = new UserAuthSessionMapper();
    }

    /// <summary>
    /// row_version（timestamp列）を除いた RepoDb Field 一覧を取得する
    /// 【重要】SQL Server の timestamp は自動管理のため、明示的な値を INSERT/UPDATE に含められない。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersion() =>
        Field.Parse(typeof(UserAuthSessionDbModel)).Where(f => f.Name != "row_version");

    /// <summary>
    /// UPDATE 対象から row_version・created_at・created_by を除いた RepoDb Field 一覧を取得する
    /// 【重要】Mapper.ToDbModel() は CreatedAt/CreatedBy を設定しない（Mapper の責務外）ため、
    ///         UPDATE 時に DbModel の CreatedAt が既定値（0001-01-01）のまま SET 句に含まれると
    ///         SqlDateTime overflow が発生する。作成時刻は不変のため UPDATE 対象から除外する。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersionAndCreatedAudit() =>
        Field.Parse(typeof(UserAuthSessionDbModel))
            .Where(f => f.Name is not ("row_version" or "created_at" or "created_by"));

    /// <summary>
    /// RowId でセッションを取得
    /// </summary>
    public async Task<UserAuthSession?> GetByIdAsync(UserAuthSessionRowId id)
    {
        var sql = _sqlQueryLoader.LoadQuery("Sessions.GetUserAuthSessionById", typeof(UserAuthSessionRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QueryFirstOrDefaultAsync<UserAuthSessionDbModel>(
            sql,
            new { RowId = id.Value });

        return dbModel != null ? _mapper.ToDomainEntity(dbModel) : null;
    }

    /// <summary>
    /// 従業員の最新セッションを取得
    /// </summary>
    public async Task<UserAuthSession?> GetLatestByAuthorityRowIdAsync(AuthorityRowId authorityRowId)
    {
        var sql = _sqlQueryLoader.LoadQuery("Sessions.GetLatestUserAuthSessionByAuthorityRowId", typeof(UserAuthSessionRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QueryFirstOrDefaultAsync<UserAuthSessionDbModel>(
            sql,
            new { CurrentUserRowId = authorityRowId.Value });

        return dbModel != null ? _mapper.ToDomainEntity(dbModel) : null;
    }

    /// <summary>
    /// ローカル認証マスターの最新セッションを取得
    /// </summary>
    public async Task<UserAuthSession?> GetLatestByLoginCredentialsRowIdAsync(LoginCredentialsRowId loginCredentialsRowId)
    {
        var sql = _sqlQueryLoader.LoadQuery("Sessions.GetLatestUserAuthSessionByLoginCredentialsRowId", typeof(UserAuthSessionRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QueryFirstOrDefaultAsync<UserAuthSessionDbModel>(
            sql,
            new { LoginCredentialsRowId = loginCredentialsRowId.Value });

        return dbModel != null ? _mapper.ToDomainEntity(dbModel) : null;
    }

    /// <summary>
    /// セッションを保存（新規作成）
    /// 【責務】
    /// - RowId が 0 の場合は Sequence で採番
    /// - RepoDb.InsertAsync でセッションを保存
    /// - 監査フィールド（CreatedAt/CreatedBy）を設定
    /// 【CreatedBy】session.AuthorityRowId（このセッションの主体）を使用。
    ///              ログイン試行中は ICurrentUserService が未設定の場合があるため、
    ///              Entity 自身が保持する AuthorityRowId を記録する
    /// </summary>
    public async Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session)
    {
        var dbModel = _mapper.ToDbModel(session);

        // 新規作成時は RowId を採番
        if (dbModel.RowId == 0)
        {
            dbModel.RowId = await _sequenceProvider.GetNextValueAsync();
        }

        // 監査情報を設定（新規作成時）
        dbModel.CreatedAt = _clock.JstNow.Value;
        dbModel.CreatedBy = session.AuthorityRowId.Value;
        dbModel.UpdatedAt = null;
        dbModel.UpdatedBy = null;
        dbModel.DeletedAt = null;
        dbModel.DeletedBy = null;

        using var connection = _connectionFactory.CreateConnection();
        await connection.InsertAsync<UserAuthSessionDbModel>(dbModel, fields: FieldsExcludingRowVersion());

        return UserAuthSessionRowId.From(dbModel.RowId);
    }

    /// <summary>
    /// セッションを更新
    /// 【責務】
    /// - RepoDb.UpdateAsync で更新
    /// - 楽観ロック（RowVersion）による競合検出
    /// - 監査フィールド（UpdatedAt/UpdatedBy）を設定
    /// 【UpdatedBy】session.AuthorityRowId（このセッションの主体）を使用
    /// </summary>
    public async Task UpdateAsync(UserAuthSession session)
    {
        var dbModel = _mapper.ToDbModel(session);

        // 監査情報を設定（更新時）
        dbModel.UpdatedAt = _clock.JstNow.Value;
        dbModel.UpdatedBy = session.AuthorityRowId.Value;

        using var connection = _connectionFactory.CreateConnection();

        // 楽観ロック付き更新（row_version で競合検出）
        // 【重要】WHERE 句に row_version を含めることで、他ユーザーによる更新を検出
        // 【重要】fields で row_version・created_at・created_by を SET 句から除外
        var affectedRows = await connection.UpdateAsync<UserAuthSessionDbModel>(
            dbModel,
            where: new QueryGroup(new[]
            {
                new QueryField("row_id", dbModel.RowId),
                new QueryField("row_version", session.RowVersion)
            }),
            fields: FieldsExcludingRowVersionAndCreatedAudit());

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"UserAuthSession update failed: RowId={session.RowId.Value}. " +
                "Session was updated by another user (concurrency conflict detected by row_version).");
        }
    }

    /// <summary>
    /// セッションを削除（論理削除）
    /// 【責務】
    /// - RepoDb.UpdateAsync で論理削除フラグ（DeletedAt/DeletedBy）を設定
    /// 【DeletedBy】対象セッションの AuthorityRowId（このセッションの主体）を使用。
    ///              DeleteAsync は RowId のみ受け取るため、事前に GetByIdAsync で取得する
    /// </summary>
    public async Task DeleteAsync(UserAuthSessionRowId id)
    {
        var existingSession = await GetByIdAsync(id)
            ?? throw new InvalidOperationException($"UserAuthSession with RowId={id.Value} not found");

        var dbModel = new UserAuthSessionDbModel
        {
            RowId = id.Value,
            DeletedAt = _clock.JstNow.Value,
            DeletedBy = existingSession.AuthorityRowId.Value
        };

        using var connection = _connectionFactory.CreateConnection();

        // 論理削除（deleted_at/deleted_by を設定）
        await connection.UpdateAsync<UserAuthSessionDbModel>(
            dbModel,
            where: new QueryGroup(new[]
            {
                new QueryField("row_id", dbModel.RowId)
            }),
            fields: new Field[]
            {
                new("deleted_at"),
                new("deleted_by")
            });
    }
}
