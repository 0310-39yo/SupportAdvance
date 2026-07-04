namespace SupportAdvance.Common.Clocks;

public interface IClock
{
    /// <summary>
    /// 現在のJST日時を取得します
    /// </summary>
    /// <remarks>
    /// 返却値の DateTime.Kind は常に Unspecified（タイムゾーン情報なし）
    /// タイムゾーン情報は LocalDateTime の型に埋め込まれています
    /// </remarks>
    LocalDateTime JstNow { get; }

    /// <summary>
    /// 本日（00:00:00）のJST日付を取得します
    /// </summary>
    /// <remarks>
    /// 時刻は常に 00:00:00 です
    /// </remarks>
    LocalDateTime JstToday { get; }
}
