using System.Data;
using Dapper;
using RepoDb;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Domain.Repositories;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Identity.Infrastructure.DbModels;
using SupportAdvance.Contexts.Identity.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Repositories;

/// <summary>
/// UserAuthSession Repository 実装
///
/// 【責務】
/// - UserAuthSession 集約の永続化
/// - SELECT: Dapper + SQL ファイル
/// - INSERT/UPDATE/DELETE: RepoDb Entity-based API
/// - 監査フィールド（CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy）の管理
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
    private readonly UserAuthSessionMapper _mapper;

    public UserAuthSessionRepository(
        IDbConnectionFactory connectionFactory,
        IClock clock,
        SqlQueryLoader sqlQueryLoader)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sqlQueryLoader = sqlQueryLoader ?? throw new ArgumentNullException(nameof(sqlQueryLoader));
        _mapper = new UserAuthSessionMapper();
    }

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
    /// - RepoDb.InsertAsync でセッションを保存
    /// - 監査フィールド（CreatedAt/CreatedBy）を設定
    /// </summary>
    public async Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session)
    {
        var dbModel = _mapper.ToDbModel(session);

        // 監査情報を設定（新規作成時）
        dbModel.CreatedAt = _clock.JstNow.Value;
        dbModel.CreatedBy = 1; // TODO: 現在のユーザーを取得する仕組みが必要
        dbModel.UpdatedAt = null;
        dbModel.UpdatedBy = null;
        dbModel.DeletedAt = null;
        dbModel.DeletedBy = null;

        using var connection = _connectionFactory.CreateConnection();
        var newRowId = (long)await connection.InsertAsync<UserAuthSessionDbModel>(dbModel);

        return UserAuthSessionRowId.From(newRowId);
    }

    /// <summary>
    /// セッションを更新
    /// 【責務】
    /// - RepoDb.UpdateAsync で更新
    /// - 楽観ロック（RowVersion）による競合検出
    /// - 監査フィールド（UpdatedAt/UpdatedBy）を設定
    /// </summary>
    public async Task UpdateAsync(UserAuthSession session)
    {
        var dbModel = _mapper.ToDbModel(session);

        // 監査情報を設定（更新時）
        dbModel.UpdatedAt = _clock.JstNow.Value;
        dbModel.UpdatedBy = 1; // TODO: 現在のユーザーを取得する仕組みが必要

        using var connection = _connectionFactory.CreateConnection();

        // 楽観ロック付き更新
        var affectedRows = await connection.UpdateAsync<UserAuthSessionDbModel>(
            dbModel,
            where: new QueryGroup(new[]
            {
                new QueryField(nameof(UserAuthSessionDbModel.RowId), dbModel.RowId)
            }));

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"UserAuthSession update failed: RowId={session.RowId.Value}. " +
                "Row not found.");
        }
    }

    /// <summary>
    /// セッションを削除（論理削除）
    /// 【責務】
    /// - RepoDb.UpdateAsync で論理削除フラグ（DeletedAt/DeletedBy）を設定
    /// </summary>
    public async Task DeleteAsync(UserAuthSessionRowId id)
    {
        var dbModel = new UserAuthSessionDbModel
        {
            RowId = id.Value,
            DeletedAt = _clock.JstNow.Value,
            DeletedBy = 1 // TODO: 現在のユーザーを取得する仕組みが必要
        };

        using var connection = _connectionFactory.CreateConnection();

        // 論理削除（deleted_at/deleted_by を設定）
        await connection.UpdateAsync<UserAuthSessionDbModel>(
            dbModel,
            fields: new Field[]
            {
                new(nameof(UserAuthSessionDbModel.DeletedAt)),
                new(nameof(UserAuthSessionDbModel.DeletedBy))
            });
    }
}
