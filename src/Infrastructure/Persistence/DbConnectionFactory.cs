using System.Data;
using Microsoft.Data.SqlClient;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL Server 接続ファクトリー実装
/// </summary>
/// <remarks>
/// <para>【責務】IAppSettings から接続文字列を読み込み、接続を生成</para>
/// <para>【設計】appsettings.*.json のすべての設定値は IAppSettings から統一して取得</para>
/// </remarks>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// <see cref="DbConnectionFactory"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="appSettings">接続文字列の取得元</param>
    /// <exception cref="ArgumentNullException"><paramref name="appSettings"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">接続文字列が 1 つも設定されていない場合</exception>
    /// <remarks>
    /// <para>【接続文字列の選択】<c>ConnectionStrings</c> の <c>Default</c> → <c>SupportAdvance</c> → 最初のキーの順</para>
    /// </remarks>
    public DbConnectionFactory(IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        var connectionString = GetConnectionString(appSettings);
        _connectionString = connectionString
                            ?? throw new InvalidOperationException(
                                "Connection string is null");
    }

    private static string? GetConnectionString(IAppSettings appSettings)
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

        throw new InvalidOperationException(
            "No connection string is configured in appsettings.json");
    }

    /// <summary>
    /// SQL Server 接続を生成（自動的に Open される）
    /// </summary>
    /// <returns>接続済み（Open 状態）の DB 接続。破棄は呼び出し元の責務</returns>
    /// <exception cref="Microsoft.Data.SqlClient.SqlException">DB への接続に失敗した場合</exception>
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
