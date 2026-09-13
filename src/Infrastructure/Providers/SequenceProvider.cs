using System.Data;
using Microsoft.Data.SqlClient;
using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Infrastructure.Exceptions;

namespace SupportAdvance.Infrastructure.Providers;

/// <summary>
/// SQL Server シーケンス（dbo.s_row_id_sequence）から RowId を採番する実装
///
/// 【責務】
/// - dbo.s_row_id_sequence から次の RowId を取得
/// - 複数個の採番は単一採番の反復実行で対応
/// - スレッドセーフな実装（lock による排他制御）
/// - SqlException を SequenceProviderException にラッピング
///
/// 【実装パターン】
/// - SqlConnection で接続（IDatabaseSettings.ConnectionStrings["Default"]）
/// - SQL: SELECT NEXT VALUE FOR [dbo].[s_row_id_sequence]
/// - 複数値取得は同じ SQL を count 回実行（シンプル、SQL Server保証）
/// - スレッド安全性：lock で保護
///
/// 【注記】
/// - SQL Server の SEQUENCE は自動的に重複排除を保証
/// - lock による同期は、複数Application instance での競合回避目的
/// </summary>
public class SequenceProvider : ISequenceProvider
{
    private readonly string _connectionString;
    private readonly Lock _lockObject = new Lock();

    public SequenceProvider(IDatabaseSettings databaseSettings)
    {
        ArgumentNullException.ThrowIfNull(databaseSettings);

        var connectionString = GetConnectionString(databaseSettings);
        _connectionString = connectionString
            ?? throw new InvalidOperationException(
                "No connection string is configured in appsettings.json");
    }

    private static string? GetConnectionString(IDatabaseSettings databaseSettings)
    {
        // 優先順位: "Default" → "SupportAdvance" → 最初のキー
        if (databaseSettings.ConnectionStrings.TryGetValue("Default", out var result))
            return result;

        if (databaseSettings.ConnectionStrings.TryGetValue("SupportAdvance", out result))
            return result;

        var firstKey = databaseSettings.ConnectionStrings.Keys.FirstOrDefault();
        if (firstKey != null && databaseSettings.ConnectionStrings.TryGetValue(firstKey, out result))
            return result;

        return null;
    }

    /// <summary>
    /// 次の RowId 単一値を取得する
    /// </summary>
    public async Task<long> GetNextValueAsync()
    {
        var values = await GetNextValuesAsync(count: 1);
        return values[0];
    }

    /// <summary>
    /// 複数個の連続した RowId を取得する
    /// </summary>
    public async Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be greater than 0.", nameof(count));
        }

        return await Task.Run(() =>
        {
            lock (_lockObject)
            {
                return GetNextValuesInternal(count);
            }
        });
    }

    /// <summary>
    /// スレッドセーフ lock 内での実装（内部用）
    /// </summary>
    private List<long> GetNextValuesInternal(int count)
    {
        var result = new List<long>(capacity: count);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "SELECT NEXT VALUE FOR [dbo].[s_row_id_sequence];";

            for (var i = 0; i < count; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;  // デフォルト 30秒

                var value = command.ExecuteScalar();

                if (value is long rowId)
                {
                    result.Add(rowId);
                }
                else if (value is int intValue)
                {
                    result.Add(Convert.ToInt64(intValue));
                }
                else if (value != null)
                {
                    result.Add(Convert.ToInt64(value));
                }
                else
                {
                    throw new SequenceProviderException(
                        "Failed to get the next sequence value from [dbo].[s_row_id_sequence]. ExecuteScalar returned null.");
                }
            }

            return result;
        }
        catch (SqlException ex)
        {
            throw new SequenceProviderException(
                $"SQL Server error occurred while retrieving RowId from sequence. (SqlException: {ex.Message})",
                ex);
        }
        catch (SequenceProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SequenceProviderException(
                $"Unexpected error occurred while retrieving RowId from sequence. ({ex.GetType().Name}: {ex.Message})",
                ex);
        }
    }
}
