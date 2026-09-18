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
/// - SqlConnection で接続（IAppSettings.ConnectionStrings["Default"]）
/// - SQL: SELECT NEXT VALUE FOR [dbo].[s_row_id_sequence]
/// - 複数値取得は同じ SQL を count 回実行（シンプル、SQL Server保証）
/// - スレッド安全性：lock で保護
///
/// 【注記】
/// - SQL Server の SEQUENCE は自動的に重複排除を保証
/// - lock による同期は、複数Application instance での競合回避目的
/// 【設計】appsettings.*.json のすべての設定値は IAppSettings から統一して取得
/// </summary>
public class SequenceProvider : ISequenceProvider
{
    private readonly string _connectionString;
    private readonly Lock _lockObject = new();

    /// <summary>
    /// <see cref="SequenceProvider"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="appSettings">接続文字列の取得元</param>
    /// <exception cref="ArgumentNullException"><paramref name="appSettings"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">接続文字列が 1 つも設定されていない場合</exception>
    /// <remarks>
    /// <para>【接続文字列の選択】<c>ConnectionStrings</c> の <c>Default</c> → <c>SupportAdvance</c> → 最初のキーの順</para>
    /// </remarks>
    public SequenceProvider(IAppSettings appSettings)
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
    /// 次の RowId 単一値を取得する
    /// </summary>
    /// <returns>採番した行ID（<c>s_row_id_sequence</c> の次の値）</returns>
    /// <exception cref="SequenceProviderException">DB への接続やシーケンスの取得に失敗した場合</exception>
    public async Task<long> GetNextValueAsync()
    {
        var values = await GetNextValuesAsync(1);
        return values[0];
    }

    /// <summary>
    /// 複数個の連続した RowId を取得する
    /// </summary>
    /// <param name="count">採番する個数（1 以上）</param>
    /// <returns>採番した行ID の一覧（要素数は <paramref name="count"/>）。連番とは限らない値</returns>
    /// <exception cref="ArgumentException"><paramref name="count"/> が 0 以下の場合</exception>
    /// <exception cref="SequenceProviderException">DB への接続やシーケンスの取得に失敗した場合</exception>
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

            const string sql = "SELECT NEXT VALUE FOR [dbo].[s_row_id_sequence];";

            for (var i = 0; i < count; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30; // デフォルト 30秒

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
