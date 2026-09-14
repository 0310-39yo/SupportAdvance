using System.Data;
using Microsoft.Data.SqlClient;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL Server 接続ファクトリー実装
///
/// 【責務】IAppSettings から接続文字列を読み込み、接続を生成
/// 【設計】appsettings.*.json のすべての設定値は IAppSettings から統一して取得
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

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
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
