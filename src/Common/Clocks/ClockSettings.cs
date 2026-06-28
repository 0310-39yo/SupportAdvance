using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Common.Clocks;

/// <summary>
/// クロック設定の具体実装
/// appsettings.json からのバインディング用
/// 注意：すべての時刻取得は IClock インターフェース経由で行う必要があります
/// DateTime.Now の直接使用は禁止
/// </summary>
public class ClockSettings : IClockSettings
{
    /// <summary>
    /// TickingClockの開始時刻（ISO 8601形式）
    /// デフォルト: 2024-01-01T00:00:00 を使用
    /// appsettings.json で明示的に指定することを推奨
    /// </summary>
    public string StartTime { get; init; } = "2024-01-01T00:00:00";

    /// <summary>
    /// ティック間隔（秒）
    /// </summary>
    public int TickIntervalSeconds { get; init; } = 1;

    /// <summary>
    /// クロックタイプ（"System"、"Ticking"、または "Offset"）
    /// </summary>
    public string ClockType { get; init; } = "System";

    /// <summary>
    /// OffsetClock用のオフセット日時（ISO 8601形式）
    /// ClockType が "Offset" の場合に使用
    /// </summary>
    public string? OffsetDateTime { get; init; } = null;
}
