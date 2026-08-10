using RepoDb;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using System.Data.SqlClient;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess;

/// <summary>
/// UserPreferences データアクセス実装
///
/// 【責務】RepoDb を使用した低レベルのDB操作
/// 【用途】Repository から呼び出される
/// 【トランザクション】呼び出し側で管理
/// 【RepoDb API】lambda 式で WHERE 条件を指定（1.15.0以上）
/// </summary>
public class UserPreferencesDataAccess : IUserPreferencesDataAccess
{
    private readonly string _connectionString;

    public UserPreferencesDataAccess(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// ユーザーIDでプリファレンス検索
    /// </summary>
    public async Task<UserPreferencesDbModel?> GetByUserIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var results = await connection.QueryAsync<UserPreferencesDbModel>(
            x => x.UserId == userId && x.DeletedAt == null);

        return results.FirstOrDefault();
    }

    /// <summary>
    /// row_idでプリファレンス検索
    /// </summary>
    public async Task<UserPreferencesDbModel?> GetByRowIdAsync(long rowId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var results = await connection.QueryAsync<UserPreferencesDbModel>(
            x => x.RowId == rowId && x.DeletedAt == null);

        return results.FirstOrDefault();
    }

    /// <summary>
    /// 新規プリファレンス挿入（採番された RowId を返す）
    ///
    /// 【責務】DB の IDENTITY/Sequence で採番された RowId を取得
    /// 【戻り値】採番された RowId（long）
    /// </summary>
    public async Task<long> InsertAsync(UserPreferencesDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // RepoDb の InsertAsync は採番値を返す（RowId は IDENTITY カラム）
        var adoptedRowId = await connection.InsertAsync("t_UserPreferences", model);

        return (long)adoptedRowId;
    }

    /// <summary>
    /// プリファレンス更新
    /// </summary>
    public async Task UpdateAsync(UserPreferencesDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var result = await connection.UpdateAsync("t_UserPreferences", model);

        if (result == 0)
        {
            throw new InvalidOperationException(
                $"No record updated for row_id {model.RowId}");
        }
    }

    /// <summary>
    /// プリファレンス削除（論理削除）
    /// </summary>
    public async Task DeleteAsync(int userId, LocalDateTime deletedAt)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var target = await connection.QueryAsync<UserPreferencesDbModel>(
            x => x.UserId == userId && x.DeletedAt == null);

        var record = target.FirstOrDefault();
        if (record == null)
        {
            throw new InvalidOperationException(
                $"No record found or already deleted for user_id {userId}");
        }

        record.DeletedAt = deletedAt.Value;  // LocalDateTime → DateTime 変換
        var result = await connection.UpdateAsync("t_UserPreferences", record);

        if (result == 0)
        {
            throw new InvalidOperationException(
                $"Failed to delete record for user_id {userId}");
        }
    }

    /// <summary>
    /// すべてのプリファレンス取得（アクティブのみ）
    /// </summary>
    public async Task<IEnumerable<UserPreferencesDbModel>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<UserPreferencesDbModel>(
            x => x.DeletedAt == null);
    }
}
