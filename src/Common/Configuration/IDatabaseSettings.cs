namespace SupportAdvance.Common.Configuration;

/// <summary>
/// データベース接続設定インターフェース
/// 【責務】
/// データベース接続に必要な設定値（サーバ名、カタログ名、接続文字列）の管理
/// 【変更理由】
/// - DB サーバの切替（本番環境変更）
/// - DB エンジンの切替（SQL Server → PostgreSQL など）
/// - 接続情報の更新（セキュリティキー更新など）
/// 【設計上の考慮】
/// 将来的に PostgreSQL への切替を想定し、DB固有の設定で拡張可能な設計
/// 現在は SQL Server を基準に設計
/// 【関連ファイル】
/// - Infrastructure/Repositories/*.cs（将来実装時に使用予定）
/// - Infrastructure/DependencyInjection.cs（登録時に使用）
/// </summary>
public interface IDatabaseSettings
{
    /// <summary>
    /// データベースサーバ名
    /// 例: "localhost", "prod-db-server.azure.com"
    /// appsettings.json の AppSettings:DbServerName に対応
    /// </summary>
    string DbServerName { get; init; }

    /// <summary>
    /// データベースカタログ（データベース名）
    /// 例: "AdvanceDB", "TestDB"
    /// appsettings.json の AppSettings:CatalogName に対応
    /// </summary>
    string CatalogName { get; init; }

    /// <summary>
    /// データベース接続文字列のディクショナリ
    /// キー例: "Default", "ReadReplica", "Analytics"
    /// 値例: "Server=localhost;Database=AdvanceDB;Integrated Security=true;"
    /// appsettings.json の AppSettings:ConnectionStrings に対応
    /// 秘密情報を含むため、appsettings.Intrinsic.json で管理・オーバーライド
    /// </summary>
    Dictionary<string, string> ConnectionStrings { get; init; }
}
