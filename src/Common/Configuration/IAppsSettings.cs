namespace SupportAdvance.Common.Configuration;

/// <summary>
/// 統合設定インターフェース
/// 【役割】
/// appsettings.json から直接バインドされる統合設定。
/// 複数の責務別インターフェースを継承し、すべての設定プロパティを提供する。
/// 【使用シーン】
/// - HostBuilderFactory での設定読み込み時
/// - DI コンテナへの登録時（後方互換性）
/// 【継承関係】
/// IApplicationSettings  → ビルド設定
/// IFileSystemSettings   → FS設定
/// IDatabaseSettings     → DB接続設定
/// 【注記】
/// IClockSettings は Common.Settings に独立。DI コンテナから別途取得。
/// </summary>
public interface IAppsSettings :
    IApplicationSettings,
    IFileSystemSettings,
    IDatabaseSettings
{
    // 統合インターフェース
    // 新規プロパティなし（すべて継承インターフェースから取得）
}
