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

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Repositories;

/// <summary>
/// UserPreferences Repository 実装
///
/// 【責務】
/// - Domain層の IUserPreferencesRepository インターフェースを実装
/// - DbModel ↔ Domain Entity 相互変換
/// - 論理削除対応
/// 【汎用基底】RepositoryBase を継承して監査情報を自動管理
/// </summary>
public class UserPreferencesRepository
    : RepositoryBase<UserPreferences, UserPreferencesDbModel>,
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
    /// </summary>
    public async Task<bool> DeleteAsync(RespondentPersonId userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        try
        {
            var deletedAt = Clock.JstNow;
            await _dataAccess.DeleteAsync(userId.Value ?? 0, deletedAt);
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
