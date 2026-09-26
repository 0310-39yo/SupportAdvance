namespace SupportAdvance.Common.Clocks;

/// <summary>
/// システムクロック実装
/// 実行時の現在時刻をシステムクロックから取得
/// </summary>
public class SystemClock : IClock
{
    private static readonly TimeZoneInfo JstTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    /// <summary>
    /// 現在のJST日時の LocalDateTime での取得
    /// </summary>
    public LocalDateTime JstNow
    {
        get
        {
            var utcNow = DateTime.UtcNow;
            var jstNow = TimeZoneInfo.ConvertTime(utcNow, TimeZoneInfo.Utc, JstTimeZone);
            // DateTime.Kind を Unspecified に統一（タイムゾーン情報は LocalDateTime の仕様）
            return new LocalDateTime(DateTime.SpecifyKind(jstNow, DateTimeKind.Unspecified));
        }
    }

    /// <summary>
    /// 本日（00:00:00）のJST日付の LocalDateTime での取得
    /// </summary>
    public LocalDateTime JstToday
    {
        get
        {
            var jstNow = JstNow;
            return new LocalDateTime(new DateTime(jstNow.Year, jstNow.Month, jstNow.Day, 0, 0, 0,
                DateTimeKind.Unspecified));
        }
    }
}
