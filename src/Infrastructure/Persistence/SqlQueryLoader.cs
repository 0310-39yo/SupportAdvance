namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL クエリファイルローダー
///
/// 【責務】埋め込みリソース（.sql ファイル）から SQL クエリを読み込む
/// 【用途】リポジトリで使用し、SQL を C# コードから分離
/// </summary>
public static class SqlQueryLoader
{
    /// <summary>
    /// SQL クエリファイルを読み込む
    /// </summary>
    /// <param name="queryPath">クエリパス（例："Employee.GetEmployeeByBizId"）</param>
    /// <returns>SQL クエリ文字列</returns>
    /// <exception cref="FileNotFoundException">SQL ファイルが見つからない場合</exception>
    public static string LoadQuery(string queryPath)
    {
        ArgumentNullException.ThrowIfNull(queryPath);

        var assembly = typeof(SqlQueryLoader).Assembly;
        var resourceName = $"SupportAdvance.Infrastructure.Persistence.Sql.{queryPath}.sql";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"SQL ファイルが見つかりません: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
