using System.Data;
using System.Data.SqlClient;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL Server 接続ファクトリー実装
///
/// 【責務】appsettings.json から接続文字列を読み込み、接続を生成
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IDatabaseSettings databaseSettings)
    {
        ArgumentNullException.ThrowIfNull(databaseSettings);

        var connectionString = GetConnectionString(databaseSettings);
        _connectionString = connectionString
            ?? throw new InvalidOperationException(
                "Connection string is null");
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
