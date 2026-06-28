namespace SupportAdvance.Common.Clocks;

/// <summary>
/// クロック設定インターフェース
/// </summary>
public interface IClockSettings
{
    /// <summary>
    /// TickingClockの開始時刻（ISO 8601形式）
    /// 例: "2024-01-01T10:00:00"
    /// </summary>
    string StartTime { get; init; }

    /// <summary>
    /// ティック間隔（秒）
    /// デフォルト: 1
    /// </summary>
    int TickIntervalSeconds { get; init; }

    /// <summary>
    /// クロックタイプ（"System"、"Ticking"、または "Offset"）
    /// </summary>
    string ClockType { get; init; }

    /// <summary>
    /// OffsetClock用のオフセット日時（ISO 8601形式）
    /// 例: "2024-01-01T10:00:00"、"2030-12-31T23:59:59"
    /// ClockType が "Offset" の場合に使用
    /// </summary>
    string? OffsetDateTime { get; init; }
}
