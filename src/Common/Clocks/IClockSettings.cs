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
    string? StartTime { get; init; }

    /// <summary>
    /// ティック間隔（秒）
    /// デフォルト: 1
    /// </summary>
    int TickIntervalSeconds { get; init; }

    /// <summary>
    /// クロックタイプ（"System"、"Ticking"、"Offset"、または "BusinessDay"）
    /// </summary>
    string? ClockType { get; init; }

    /// <summary>
    /// OffsetClock用のオフセット日時（ISO 8601形式）
    /// 例: "2024-01-01T10:00:00"、"2030-12-31T23:59:59"
    /// ClockType が "Offset" の場合に使用
    /// </summary>
    string? OffsetDateTime { get; init; }

    /// <summary>
    /// BusinessDayClock の基点日（<c>yyyy-MM-dd</c> 形式）
    /// </summary>
    /// <value>一番最初の ON で始まる業務日。ClockType が "BusinessDay" の場合は必須</value>
    string? BusinessDayStartDate { get; init; }

    /// <summary>
    /// BusinessDayClock の状態ファイルのパス
    /// </summary>
    /// <value><see langword="null"/> の場合は <see cref="BusinessDayClock.DefaultStateFilePath"/></value>
    string? BusinessDayStateFilePath { get; init; }
}
