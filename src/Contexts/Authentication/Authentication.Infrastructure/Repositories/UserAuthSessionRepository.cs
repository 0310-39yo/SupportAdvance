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
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>UserAuthSession 集約の永続化</description></item>
/// <item><description>SELECT: Dapper + SQL ファイル</description></item>
/// <item><description>INSERT/UPDATE/DELETE: RepoDb Entity-based API</description></item>
/// <item><description>監査フィールド（CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy）の管理</description></item>
/// </list>
/// <para>【重要】CreatedBy/UpdatedBy/DeletedBy には session.AuthorityRowId（このセッションの主体）を使用。ログイン試行中は ICurrentUserService が未設定の場合があるため不採用</para>
/// <para>【パターン】</para>
/// <list type="bullet">
/// <item><description>GetByIdAsync: Dapper + SQL</description></item>
/// <item><description>GetLatestByAuthorityRowIdAsync: Dapper + SQL</description></item>
/// <item><description>GetLatestByLoginCredentialsRowIdAsync: Dapper + SQL</description></item>
/// <item><description>SaveAsync: RepoDb Insert</description></item>
/// <item><description>UpdateAsync: RepoDb Update（楽観ロック付き）</description></item>
/// <item><description>DeleteAsync: RepoDb Update（論理削除）</description></item>
/// </list>
/// </remarks>
public class UserAuthSessionRepository : IUserAuthSessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IClock _clock;
    private readonly SqlQueryLoader _sqlQueryLoader;
    private readonly ISequenceProvider _sequenceProvider;
    private readonly UserAuthSessionMapper _mapper;

    /// <summary>
    /// <see cref="UserAuthSessionRepository"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="connectionFactory">DB 接続の生成元</param>
    /// <param name="clock">監査列（<c>*_at</c>）に記録する現在時刻（JST）の取得元</param>
    /// <param name="sqlQueryLoader">検索用 SQL ファイルの読み込み元</param>
    /// <param name="sequenceProvider">行ID の採番元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
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
    /// row_version（timestamp列）を除いた RepoDb Field 一覧の取得
    /// </summary>
    /// <remarks>
    /// <para>【重要】SQL Server の timestamp は自動管理のため、明示的な値の INSERT/UPDATE への包含は不可</para>
    /// </remarks>
    private static IEnumerable<Field> FieldsExcludingRowVersion() =>
        Field.Parse(typeof(UserAuthSessionDbModel)).Where(f => f.Name != "row_version");

    /// <summary>
    /// UPDATE 対象から row_version・created_at・created_by を除いた RepoDb Field 一覧の取得
    /// </summary>
    /// <remarks>
    /// <para>【重要】Mapper.ToDbModel() は CreatedAt/CreatedBy を設定しない（Mapper の責務外）ため、UPDATE 時に DbModel の CreatedAt が既定値（0001-01-01）のまま SET 句に含まれると、SqlDateTime overflow の発生。作成時刻は不変のため UPDATE 対象から除外</para>
    /// </remarks>
    private static IEnumerable<Field> FieldsExcludingRowVersionAndCreatedAudit() =>
        Field.Parse(typeof(UserAuthSessionDbModel))
            .Where(f => f.Name is not ("row_version" or "created_at" or "created_by"));

    /// <summary>
    /// RowId でセッションを取得
    /// </summary>
    /// <param name="id">取得するセッションの行ID</param>
    /// <returns>見つかったセッション。見つからない場合は <see langword="null"/></returns>
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
    /// <param name="authorityRowId">権限主体（従業員）の行ID</param>
    /// <returns>最新のセッション。見つからない場合は <see langword="null"/></returns>
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
    /// <param name="loginCredentialsRowId">認証情報の行ID</param>
    /// <returns>最新のセッション。見つからない場合は <see langword="null"/></returns>
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
    /// </summary>
    /// <param name="session">保存するセッション</param>
    /// <returns>保存したセッションの行ID（採番した場合は採番後の値）</returns>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>RowId が 0 の場合は Sequence で採番</description></item>
    /// <item><description>RepoDb.InsertAsync でセッションを保存</description></item>
    /// <item><description>監査フィールド（CreatedAt/CreatedBy）を設定</description></item>
    /// </list>
    /// <para>【CreatedBy】session.AuthorityRowId（このセッションの主体）を使用。ログイン試行中は ICurrentUserService が未設定の場合があるため、Entity 自身が保持する AuthorityRowId の記録</para>
    /// </remarks>
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
    /// </summary>
    /// <param name="session">更新するセッション。<c>RowVersion</c> は読み込み時の値であること</param>
    /// <exception cref="InvalidOperationException">更新対象の行がない場合（他のユーザーによる更新・削除で <c>row_version</c> が一致しない場合を含む）</exception>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>RepoDb.UpdateAsync で更新</description></item>
    /// <item><description>楽観ロック（RowVersion）による競合検出</description></item>
    /// <item><description>監査フィールド（UpdatedAt/UpdatedBy）を設定</description></item>
    /// </list>
    /// <para>【UpdatedBy】session.AuthorityRowId（このセッションの主体）を使用</para>
    /// </remarks>
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
    /// </summary>
    /// <param name="id">削除するセッションの行ID</param>
    /// <exception cref="InvalidOperationException">セッションが見つからない場合</exception>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>RepoDb.UpdateAsync で論理削除フラグ（DeletedAt/DeletedBy）を設定</description></item>
    /// </list>
    /// <para>【DeletedBy】対象セッションの AuthorityRowId（このセッションの主体）を使用。DeleteAsync は RowId のみ受け取るため、事前の GetByIdAsync での取得が必要</para>
    /// </remarks>
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
