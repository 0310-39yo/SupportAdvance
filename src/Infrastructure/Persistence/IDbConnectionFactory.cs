using System.Data;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// データベース接続ファクトリー
/// </summary>
/// <remarks>
/// <para>【責務】SQL Server への接続を生成</para>
/// <para>【用途】Repository で Dapper 実行時に使用</para>
/// </remarks>
public interface IDbConnectionFactory
{
    /// <summary>
    /// データベース接続の生成
    /// </summary>
    /// <returns>開かれたデータベース接続</returns>
    IDbConnection CreateConnection();
}
