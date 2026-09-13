using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL ファイルを埋め込みリソースから読み込むローダー（DI対応版）
///
/// 【責務】
/// - Context 内の Persistence/Sql フォルダから SQL ファイルを解決
/// - DB方言（SqlServer / PostgreSQL）を AppSettings から読み込み
/// - SELECT 操作用（Repository で Dapper 実行時に使用）
///
/// 【使用例】
/// var sql = _queryLoader.LoadQuery("Auth.GetLoginCredentialsByLoginId", typeof(LoginCredentialsRepository));
/// → Identity.Infrastructure/Persistence/Sql/SqlServer/Auth/GetLoginCredentialsByLoginId.sql
/// </summary>
public class SqlQueryLoader
{
    private readonly IAppSettings _appSettings;

    public SqlQueryLoader(IAppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// SQL ファイルを読み込む
    /// </summary>
    /// <param name="queryPath">クエリパス（例："Auth.GetLoginCredentialsByLoginId"）</param>
    /// <param name="repositoryType">Repository の Type（リソース検索用）</param>
    /// <returns>SQL クエリ文字列</returns>
    /// <exception cref="FileNotFoundException">SQL ファイルが見つからない場合</exception>
    public string LoadQuery(string queryPath, Type repositoryType)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryPath, nameof(queryPath));
        ArgumentNullException.ThrowIfNull(repositoryType, nameof(repositoryType));

        // DB方言を取得（未設定時は"SqlServer"にフォールバック）
        var dbDialect = _appSettings.Database.Dialect ?? "SqlServer";

        // repositoryType の Namespace から Context を特定
        // 例: SupportAdvance.Contexts.Identity.Infrastructure.Repositories.LoginCredentialsRepository
        // → SupportAdvance.Contexts.Identity.Infrastructure
        var contextNamespace = repositoryType.Namespace;
        if (string.IsNullOrEmpty(contextNamespace))
            throw new InvalidOperationException($"Cannot determine namespace for type {repositoryType.FullName}");

        // queryPath を解析（例："Auth.GetLoginCredentialsByLoginId" → ["Auth", "GetLoginCredentialsByLoginId"]）
        var parts = queryPath.Split('.');
        if (parts.Length != 2)
            throw new ArgumentException(
                $"Invalid query path format: '{queryPath}'. Expected 'Category.QueryName' (e.g., 'Auth.GetLoginCredentialsByLoginId')",
                nameof(queryPath));

        var category = parts[0];
        var queryName = parts[1];

        // リソース名構築
        // 例: SupportAdvance.Contexts.Identity.Infrastructure.Persistence.Sql.SqlServer.Auth.GetLoginCredentialsByLoginId.sql
        var resourceName = $"{contextNamespace}.Persistence.Sql.{dbDialect}.{category}.{queryName}.sql";

        // Assembly から埋め込みリソースを取得
        var assembly = repositoryType.Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"SQL file not found: {resourceName}\n" +
                $"Expected location: [Context].Infrastructure/Persistence/Sql/{dbDialect}/{category}/{queryName}.sql\n" +
                $"Database Dialect: {dbDialect}\n" +
                $"Assembly: {assembly.FullName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 現在の DB方言を取得
    /// </summary>
    public string GetCurrentDialect()
    {
        return _appSettings.Database.Dialect ?? "SqlServer";
    }
}
