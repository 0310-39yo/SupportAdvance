namespace SupportAdvance.Common.Configuration;

/// <summary>
/// データベース設定
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// DB方言（"SqlServer" または "PostgreSQL"）
    /// </summary>
    public string Dialect { get; init; } = "SqlServer";
}
