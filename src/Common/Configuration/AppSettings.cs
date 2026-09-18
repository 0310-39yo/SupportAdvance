using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Common.Configuration;

/// <summary>
/// appsettings.*.json の <c>AppSettings</c> セクションをバインドする設定クラス
/// </summary>
/// <remarks>
/// <para>【用途】すべての設定値は <see cref="IAppSettings"/> 経由で統一して取得</para>
/// <para>【注意】初期値（<c>YOUR_DOMAIN</c> など）はプレースホルダー。実際の値は appsettings.*.json で設定</para>
/// </remarks>
public class AppSettings : IAppSettings
{
    /// <summary>
    /// <see cref="ApplicationBuildType"/> における Debug ビルドを表す値
    /// </summary>
    public const string DebugBuild = "Debug";

    /// <summary>
    /// <see cref="ApplicationBuildType"/> における Release ビルドを表す値
    /// </summary>
    public const string ReleaseBuild = "Release";

    /// <summary>
    /// クロック設定
    /// JSON バインディング用にクロック設定値を保持します。
    /// 実際のクロック機能は IClock インターフェース経由で取得してください。
    /// </summary>
    public IClockSettings ClockSettings { get; init; } = new ClockSettings();

    /// <inheritdoc />
    public string ActiveDirectoryDomain { get; init; } = "YOUR_DOMAIN";

    /// <inheritdoc />
    public string ApplicationBuildType { get; init; } = DebugBuild;

    /// <inheritdoc />
    public string SolutionName { get; init; } = "SolutionName";

    /// <inheritdoc />
    public string FolderName { get; init; } = "FolderName";

    /// <inheritdoc />
    public string ProjectName { get; init; } = "ProjectName";

    /// <inheritdoc />
    public string DbServerName { get; init; } = "YourDbServerName";

    /// <inheritdoc />
    public string CatalogName { get; init; } = "YourCatalogName";

    /// <inheritdoc />
    public Dictionary<string, string> FolderPaths { get; init; } = new();

    /// <inheritdoc />
    public Dictionary<string, string> ConnectionStrings { get; init; } = new();

    /// <summary>
    /// データベース設定
    /// </summary>
    public DatabaseSettings Database { get; init; } = new();
}
