using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Data;

/// <summary>
/// データベース接続ファクトリー
///
/// 【責務】接続文字列管理、SqlConnection生成
/// 【用途】RepoDb/Dapperでの接続提供
/// 【ライフサイクル】Singleton登録推奨
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connStr = configuration.GetConnectionString("SupportAdvance");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException(
                "Connection string 'SupportAdvance' not found in configuration");
        }

        _connectionString = connStr;

        // Debug出力
        Debug.WriteLine($"[DbConnectionFactory] Connection string loaded: {_connectionString}");
    }

    /// <summary>
    /// 新しいDB接続を作成
    ///
    /// 【責務】接続を開かない（呼び出し側で開く）
    /// 【戻り値】新しい SqlConnection インスタンス
    /// </summary>
    public IDbConnection CreateConnection()
    {
        Debug.WriteLine($"[DbConnectionFactory] Creating connection with: {_connectionString}");
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// 接続文字列を取得（デバッグ用）
    /// </summary>
    internal string GetConnectionString() => _connectionString;
}
