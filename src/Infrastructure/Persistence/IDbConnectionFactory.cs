using System.Data;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// データベース接続ファクトリー
///
/// 【責務】SQL Server への接続を生成
/// 【用途】Repository で Dapper 実行時に使用
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// データベース接続を生成する
    /// </summary>
    /// <returns>開かれたデータベース接続</returns>
    IDbConnection CreateConnection();
}
