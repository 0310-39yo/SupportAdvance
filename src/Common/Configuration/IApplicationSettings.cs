namespace SupportAdvance.Common.Configuration;

/// <summary>
/// アプリケーション基本設定インターフェース
/// 【責務】
/// ビルド構成（Debug / Release）の管理
/// 【変更理由】
/// CI/CD パイプライン、ビルド戦略の変更時に修正される
/// 【関連ファイル】
/// - EnvironmentInfo.cs（コンパイル時に環境を判定）
/// - HostBuilderFactory.cs（NLog 初期化時に使用）
/// </summary>
public interface IApplicationSettings
{
    /// <summary>
    /// アプリケーションのビルドタイプ
    /// 値: "Debug" または "Release"
    /// appsettings.json の AppSettings:ApplicationBuildType に対応
    /// </summary>
    string ApplicationBuildType { get; init; }
}
