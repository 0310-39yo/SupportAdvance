using RepoDb;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Repositories;

/// <summary>
/// UserPreferences Repository 実装
///
/// 【責務】
/// - Domain層の IUserPreferencesRepository インターフェースを実装
/// - DbModel ↔ Domain Entity 相互変換（RowId ValueObject対応）
/// - 論理削除対応
/// 【汎用基底】RepositoryBase<TEntity, TDbModel, TId> を継承して監査情報を自動管理
/// </summary>
public class UserPreferencesRepository
    : RepositoryBase<UserPreferences, UserPreferencesDbModel, RowId>,
      IUserPreferencesRepository
{
    private readonly IUserPreferencesDataAccess _dataAccess;

    public UserPreferencesRepository(
        UserPreferencesMapper mapper,
        ICurrentUserService currentUser,
        IClock clock,
        IUserPreferencesDataAccess dataAccess)
        : base(mapper, currentUser, clock)
    {
        _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
    }

    /// <summary>
    /// ユーザーIDでプリファレンスを取得
    /// </summary>
    public async Task<UserPreferences?> GetAsync(RespondentPersonId userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var dbModel = await _dataAccess.GetByUserIdAsync(userId.Value ?? 0);
        if (dbModel == null)
        {
            return null;
        }

        return MapToDomain(dbModel);
    }

    /// <summary>
    /// row_idでプリファレンスを取得
    /// </summary>
    public async Task<UserPreferences?> GetByRowIdAsync(long rowId)
    {
        var dbModel = await _dataAccess.GetByRowIdAsync(rowId);
        if (dbModel == null)
        {
            return null;
        }

        return MapToDomain(dbModel);
    }

    /// <summary>
    /// 新規プリファレンスを追加
    /// </summary>
    public async Task AddAsync(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var dbModel = MapToDatabaseForInsert(preferences);
        await _dataAccess.InsertAsync(dbModel);
    }

    /// <summary>
    /// プリファレンスを更新
    /// </summary>
    public async Task UpdateAsync(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var dbModel = MapToDatabaseForUpdate(preferences);
        await _dataAccess.UpdateAsync(dbModel);
    }

    /// <summary>
    /// プリファレンスを削除（論理削除）
    ///
    /// 【責務】Domain層での削除操作（Entity.SoftDelete()）を DB に反映
    /// </summary>
    public async Task<bool> DeleteAsync(RespondentPersonId userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        try
        {
            var preferences = await GetAsync(userId);
            if (preferences == null)
            {
                return false;
            }

            var isDeleted = preferences.SoftDelete(Clock);
            if (!isDeleted)
            {
                return false;  // 既に削除済み
            }

            var dbModel = MapToDatabaseForUpdate(preferences);
            await _dataAccess.UpdateAsync(dbModel);

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>
    /// すべてのプリファレンスを取得（アクティブのみ）
    /// </summary>
    public async Task<IReadOnlyList<UserPreferences>> GetAllAsync()
    {
        var dbModels = await _dataAccess.GetAllAsync();

        return dbModels.Select(m => MapToDomain(m))
            .ToList()
            .AsReadOnly();
    }
}
