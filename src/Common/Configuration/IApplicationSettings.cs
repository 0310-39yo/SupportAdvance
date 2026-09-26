namespace SupportAdvance.Common.Configuration;

/// <summary>
/// アプリケーション基本設定インターフェース
/// </summary>
/// <remarks>
/// <para>【責務】ビルド構成（Debug / Release）の管理</para>
/// <para>【変更理由】CI/CD パイプライン、ビルド戦略の変更時の修正</para>
/// <para>【関連ファイル】</para>
/// <list type="bullet">
/// <item><description>EnvironmentInfo.cs（コンパイル時に環境を判定）</description></item>
/// <item><description>HostBuilderFactory.cs（NLog 初期化時に使用）</description></item>
/// </list>
/// </remarks>
public interface IApplicationSettings
{
    /// <summary>
    /// アプリケーションのビルドタイプ
    /// 値: "Debug" または "Release"
    /// appsettings.json の AppSettings:ApplicationBuildType に対応
    /// </summary>
    string ApplicationBuildType { get; init; }
}
