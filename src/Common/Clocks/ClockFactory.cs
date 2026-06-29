using System.Globalization;

namespace SupportAdvance.Common.Clocks;

/// <summary>
/// クロック実装のファクトリクラス
/// IClockSettings に基づいて、適切なクロック実装を生成する
/// </summary>
public static class ClockFactory
{
    /// <summary>
    /// 設定に基づいて IClock インスタンスを生成する
    /// </summary>
    /// <param name="settings">クロック設定</param>
    /// <returns>生成されたクロック実装（SystemClock, TickingClock, または OffsetClock）</returns>
    /// <exception cref="ArgumentNullException">settings が null の場合</exception>
    /// <exception cref="ArgumentException">無効な ClockType が指定された場合</exception>
    /// <exception cref="FormatException">OffsetDateTime または StartTime の形式が不正な場合</exception>
    public static IClock CreateClock(IClockSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.ClockType?.ToUpperInvariant() switch
        {
            "SYSTEM" => new SystemClock(),
            "TICKING" => CreateTickingClock(settings),
            "OFFSET" => CreateOffsetClock(settings),
            null => throw new ArgumentException("ClockType is null.", nameof(settings)),
            _ => throw new ArgumentException($"Unknown ClockType: '{settings.ClockType}'.", nameof(settings))
        };
    }

    /// <summary>
    /// TickingClock を生成する
    /// 注記：
    /// - StartTime が指定されていない場合、SystemClock.JstNow で現在のJST時刻を取得
    /// - 実行時は TickingClock.JstNow（IClock経由）を通じて時刻を取得
    /// </summary>
    private static TickingClock CreateTickingClock(IClockSettings settings)
    {
        // StartTime を DateTime に変換（ISO 8601形式を想定）
        // StartTime が未指定の場合は、現在のJST時刻を初期値として使用
        var startTime = settings.StartTime != null
            ? DateTime.Parse(settings.StartTime, CultureInfo.InvariantCulture)
            : new SystemClock().JstNow.Value;

        // DateTime.Kind を Unspecified に統一（LocalDateTime の仕様）
        startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);

        // TickIntervalSeconds を TimeSpan に変換
        var tickInterval = settings.TickIntervalSeconds > 0
            ? TimeSpan.FromSeconds(settings.TickIntervalSeconds)
            : (TimeSpan?)null;

        return new TickingClock(startTime, tickInterval);
    }

    /// <summary>
    /// OffsetClock を生成する
    /// 注記：
    /// - OffsetDateTime が未指定の場合、または日付のみの場合、SystemClock.JstNow で現在のJST時刻を取得
    /// - 実行時は OffsetClock.JstNow（IClock経由）を通じて時刻を取得
    /// </summary>
    private static OffsetClock CreateOffsetClock(IClockSettings settings)
    {
        DateTime offsetDateTime;

        if (string.IsNullOrWhiteSpace(settings.OffsetDateTime))
        {
            // OffsetDateTime が指定されていない場合、現在のJST時刻を使用
            offsetDateTime = new SystemClock().JstNow.Value;
        }
        else
        {
            // OffsetDateTime をパース（日付のみまたは日時の形式に対応）
            var parsedDateTime = DateTime.Parse(
                settings.OffsetDateTime,
                CultureInfo.InvariantCulture);

            // 指定された日付に、現在時刻を合わせる
            // OffsetDateTime に時刻部分がある場合：指定された時刻を使用
            // OffsetDateTime に時刻部分がない場合（日付のみ）：現在のJST時刻を使用
            var now = new SystemClock().JstNow.Value;

            // 指定されたテキストに時刻情報があるかチェック
            // 例："2020-01-01" は時刻なし、"2020-01-01T14:30:00" は時刻あり
            var hasTimeComponent = settings.OffsetDateTime.Contains("T") ||
                                   settings.OffsetDateTime.Contains(" ");

            if (hasTimeComponent && parsedDateTime.TimeOfDay != TimeSpan.Zero)
            {
                // 指定された日時をそのまま使用
                offsetDateTime = parsedDateTime;
            }
            else
            {
                // 指定された日付 + 現在時刻を組み合わせる
                offsetDateTime = parsedDateTime.Date.Add(now.TimeOfDay);
            }
        }

        // DateTime.Kind を Unspecified に統一（LocalDateTime の仕様）
        offsetDateTime = DateTime.SpecifyKind(offsetDateTime, DateTimeKind.Unspecified);

        return new OffsetClock(offsetDateTime);
    }
}
