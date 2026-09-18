namespace SupportAdvance.Common.Clocks;

/// <summary>
/// 現在日時（JST）の取得元の抽象
/// </summary>
/// <remarks>
/// <para>【重要】Domain／Application 層での日時の取得は、必ずこのインターフェース経由（<c>DateTime.Now</c> などの直接使用は禁止）</para>
/// <para>【実装】本番は <see cref="SystemClock"/>。テストは <see cref="MockClock"/>／<see cref="OffsetClock"/>／<see cref="TickingClock"/>。生成は <see cref="ClockFactory"/></para>
/// <para>【参照】docs/Assistance/Guides/LocalDateTime_タイムゾーン_ガイド.md</para>
/// </remarks>
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
