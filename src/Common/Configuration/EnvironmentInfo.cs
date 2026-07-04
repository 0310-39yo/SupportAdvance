namespace SupportAdvance.Common.Configuration;

/// <summary>
/// アプリケーションの実行環境情報を提供する静的クラス。
/// </summary>
public static class EnvironmentInfo
{
    /// <summary>
    /// 現在のビルド構成に基づく環境名を表す定数文字列。
    /// Debug ビルド時は <see cref="AppSettings.DebugBuild" />、
    /// Release ビルド時は <see cref="AppSettings.ReleaseBuild" /> を返す。
    /// </summary>
    public static readonly string Environment =
#if DEBUG
        AppSettings.DebugBuild;
#else
        AppSettings.ReleaseBuild;
#endif
}
