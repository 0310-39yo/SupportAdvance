namespace SupportAdvance.Common.Configuration;

/// <summary>
/// 統合設定インターフェース
/// </summary>
/// <remarks>
/// <para>【役割】appsettings.json から直接バインドされる統合設定。複数の責務別インターフェースを継承し、すべての設定プロパティの提供</para>
/// <para>【使用シーン】</para>
/// <list type="bullet">
/// <item><description>HostBuilderFactory での設定読み込み時</description></item>
/// <item><description>DI コンテナへの登録時（後方互換性）</description></item>
/// </list>
/// <para>【継承関係】IApplicationSettings  → ビルド設定。IFileSystemSettings   → FS設定。IDatabaseSettings     → DB接続設定</para>
/// <para>【注記】IClockSettings は Common.Settings に独立。DI コンテナから別途取得</para>
/// </remarks>
public interface IAppSettings :
    IAdDomain,
    IApplicationSettings,
    IFileSystemSettings,
    IDatabaseSettings
{
    // 統合インターフェース
    // 新規プロパティなし（すべて継承インターフェースから取得）
}
