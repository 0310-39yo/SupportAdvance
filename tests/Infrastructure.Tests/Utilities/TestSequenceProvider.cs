using System.Data;
using Microsoft.Data.SqlClient;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure.Tests.Utilities;

/// <summary>
/// テスト用シーケンス s_test_row_id_sequence から RowId を採番するユーティリティ
///
/// 【責務】
/// - s_test_row_id_sequence から次の RowId を取得（本番データと独立）
/// - 複数値採番、単一値採番の両方対応
/// - スレッドセーフな実装（lock による排他制御）
///
/// 【使用方法】
/// var provider = new TestSequenceProvider(appSettings);
/// var rowId = await provider.GetNextValueAsync();
/// </summary>
public class TestSequenceProvider
{
    private readonly string _connectionString;
    private readonly object _lockObject = new();

    public TestSequenceProvider(IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        var connectionString = GetConnectionString(appSettings);
        _connectionString = connectionString
                            ?? throw new InvalidOperationException(
                                "No connection string is configured in appsettings.json");
    }

    internal static string? GetConnectionString(IAppSettings appSettings)
    {
        // 優先順位: "Default" → "SupportAdvance" → 最初のキー
        if (appSettings.ConnectionStrings.TryGetValue("Default", out var result))
        {
            return result;
        }

        if (appSettings.ConnectionStrings.TryGetValue("SupportAdvance", out result))
        {
            return result;
        }

        var firstKey = appSettings.ConnectionStrings.Keys.FirstOrDefault();
        if (firstKey != null && appSettings.ConnectionStrings.TryGetValue(firstKey, out result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 次のテスト用 RowId 単一値を取得する
    /// </summary>
    public async Task<long> GetNextValueAsync()
    {
        var values = await GetNextValuesAsync(1);
        return values[0];
    }

    /// <summary>
    /// 複数個の連続したテスト用 RowId を取得する
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
        var result = new List<long>(count);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "SELECT NEXT VALUE FOR [dbo].[s_test_row_id_sequence];";

            for (var i = 0; i < count; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;

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
                    throw new InvalidOperationException(
                        "Failed to get the next sequence value from [dbo].[s_test_row_id_sequence]. ExecuteScalar returned null.");
                }
            }

            return result;
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                $"SQL Server error occurred while retrieving RowId from test sequence. (SqlException: {ex.Message})",
                ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unexpected error occurred while retrieving RowId from test sequence. ({ex.GetType().Name}: {ex.Message})",
                ex);
        }
    }
}
