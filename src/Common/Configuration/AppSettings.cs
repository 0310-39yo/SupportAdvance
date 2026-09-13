using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Common.Configuration;

public class AppSettings : IAppSettings
{
    /// <summary>
    /// DebugBuild = "Debug"
    /// </summary>
    public const string DebugBuild = "Debug";

    /// <summary>
    /// ReleaseBuild = "Release"
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
}
