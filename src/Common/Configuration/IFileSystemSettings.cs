namespace SupportAdvance.Common.Configuration;

/// <summary>
/// ファイルシステム関連設定インターフェース
/// 【責務】
/// アプリケーションが使用するファイルパス、フォルダ構成の管理
/// 【変更理由】
/// プロジェクト構成の変更、ファイルパスの再構成が必要な場合
/// 【関連ファイル】
/// - HostBuilderFactory.cs（NLog 初期化時に SolutionName, FolderName, ProjectName を使用）
/// - 将来の Repository 実装（FolderPaths を使用予定）
/// </summary>
public interface IFileSystemSettings
{
    /// <summary>
    /// ソリューション名（最上位フォルダ名）
    /// 例: "Advance", "MyProject"
    /// appsettings.json の AppSettings:SolutionName に対応
    /// </summary>
    string SolutionName { get; init; }

    /// <summary>
    /// データの第二階層フォルダ名
    /// 例: "Repository", "Data"
    /// appsettings.json の AppSettings:FolderName に対応
    /// </summary>
    string FolderName { get; init; }

    /// <summary>
    /// プロジェクト名（第三階層フォルダ名）
    /// 例: "CoreSystem", "API"
    /// appsettings.json の AppSettings:ProjectName に対応
    /// </summary>
    string ProjectName { get; init; }

    /// <summary>
    /// 用途別フォルダパスのディクショナリ
    /// 例: { "Data": "C:\\Data", "Logs": "C:\\Logs" }
    /// appsettings.json の AppSettings:FolderPaths に対応
    /// 秘密情報を含む可能性があるため、appsettings.Intrinsic.json で管理
    /// </summary>
    Dictionary<string, string> FolderPaths { get; init; }
}
