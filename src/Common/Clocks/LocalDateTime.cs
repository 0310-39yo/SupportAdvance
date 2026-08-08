namespace SupportAdvance.Common.Clocks;

/// <summary>
/// JST (日本標準時) を表現する不変の日時値オブジェクト
/// このstructは、アプリケーション内部で常にJSTとして解釈される日時を表現します。
/// DateTime.Kind は常に Unspecified であり、タイムゾーン情報は型に埋め込まれています。
/// </summary>
public readonly struct LocalDateTime : IComparable<LocalDateTime>, IEquatable<LocalDateTime>
{
    private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    /// <summary>
    /// JST として解釈される DateTime (Kind = Unspecified)
    /// </summary>
    public DateTime Value { get; }

    /// <summary>
    /// LocalDateTime の最小値（DateTime.MinValue）
    /// 【用途】Unset 状態の表現、形式検証での境界値チェック
    /// </summary>
    public static readonly LocalDateTime MinValue = new(DateTime.MinValue);

    /// <summary>
    /// LocalDateTime の最大値（DateTime.MaxValue）
    /// 【用途】形式検証での無効値チェック
    /// </summary>
    public static readonly LocalDateTime MaxValue = new(DateTime.MaxValue);

    /// <summary>
    /// JST日時を指定して LocalDateTime を構築します
    /// </summary>
    /// <param name="jstValue">JST として解釈される DateTime。Kind は Unspecified である必要があります。</param>
    /// <exception cref="ArgumentException">Kind が Unspecified でない場合</exception>
    public LocalDateTime(DateTime jstValue)
    {
        if (jstValue.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "LocalDateTime は DateTimeKind.Unspecified である必要があります。" +
                $"指定された Kind: {jstValue.Kind}",
                nameof(jstValue));
        }

        Value = jstValue;
    }

    /// <summary>
    /// 年を取得します
    /// </summary>
    public int Year => Value.Year;

    /// <summary>
    /// 月を取得します
    /// </summary>
    public int Month => Value.Month;

    /// <summary>
    /// 日を取得します
    /// </summary>
    public int Day => Value.Day;

    /// <summary>
    /// 時を取得します
    /// </summary>
    public int Hour => Value.Hour;

    /// <summary>
    /// 分を取得します
    /// </summary>
    public int Minute => Value.Minute;

    /// <summary>
    /// 秒を取得します
    /// </summary>
    public int Second => Value.Second;

    /// <summary>
    /// ミリ秒を取得します
    /// </summary>
    public int Millisecond => Value.Millisecond;

    /// <summary>
    /// 日付部分のみを持つ LocalDateTime を取得します（時刻は 00:00:00）
    /// </summary>
    public LocalDateTime Date =>
        new(new DateTime(Value.Year, Value.Month, Value.Day, 0, 0, 0, DateTimeKind.Unspecified));

    /// <summary>
    /// このJST日時をUTCに変換します
    /// </summary>
    /// <returns>UTC時刻（Kind = Utc）</returns>
    public DateTime ToUtc()
    {
        var utcValue = TimeZoneInfo.ConvertTime(Value, JstTimeZone, TimeZoneInfo.Utc);
        return DateTime.SpecifyKind(utcValue, DateTimeKind.Utc);
    }

    /// <summary>
    /// UTC時刻をJST（LocalDateTime）に変換します
    /// </summary>
    /// <param name="utcValue">UTC時刻（Kind は Utc である必要があります）</param>
    /// <returns>JST の LocalDateTime</returns>
    /// <exception cref="ArgumentException">Kind が Utc でない場合</exception>
    public static LocalDateTime FromUtc(DateTime utcValue)
    {
        if (utcValue.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "入力は DateTimeKind.Utc である必要があります。" +
                $"指定された Kind: {utcValue.Kind}",
                nameof(utcValue));
        }

        var jstValue = TimeZoneInfo.ConvertTime(utcValue, TimeZoneInfo.Utc, JstTimeZone);
        return new LocalDateTime(DateTime.SpecifyKind(jstValue, DateTimeKind.Unspecified));
    }

    /// <summary>
    /// このLocalDateTimeに指定した TimeSpan を加算します
    /// </summary>
    public static LocalDateTime operator +(LocalDateTime left, TimeSpan right) => new(left.Value.Add(right));

    /// <summary>
    /// このLocalDateTimeから指定した TimeSpan を減算します
    /// </summary>
    public static LocalDateTime operator -(LocalDateTime left, TimeSpan right) => new(left.Value.Subtract(right));

    /// <summary>
    /// 2つの LocalDateTime の差を求めます
    /// </summary>
    public static TimeSpan operator -(LocalDateTime left, LocalDateTime right) => left.Value.Subtract(right.Value);

    /// <summary>
    /// 2つの LocalDateTime が等しいかどうかを判定します
    /// </summary>
    public static bool operator ==(LocalDateTime left, LocalDateTime right) => left.Value == right.Value;

    /// <summary>
    /// 2つの LocalDateTime が異なるかどうかを判定します
    /// </summary>
    public static bool operator !=(LocalDateTime left, LocalDateTime right) => left.Value != right.Value;

    /// <summary>
    /// 左辺が右辺より大きいかどうかを判定します
    /// </summary>
    public static bool operator >(LocalDateTime left, LocalDateTime right) => left.Value > right.Value;

    /// <summary>
    /// 左辺が右辺より小さいかどうかを判定します
    /// </summary>
    public static bool operator <(LocalDateTime left, LocalDateTime right) => left.Value < right.Value;

    /// <summary>
    /// 左辺が右辺以上かどうかを判定します
    /// </summary>
    public static bool operator >=(LocalDateTime left, LocalDateTime right) => left.Value >= right.Value;

    /// <summary>
    /// 左辺が右辺以下かどうかを判定します
    /// </summary>
    public static bool operator <=(LocalDateTime left, LocalDateTime right) => left.Value <= right.Value;

    /// <summary>
    /// このインスタンスと指定したオブジェクトが等しいかどうかを判定します
    /// </summary>
    public override bool Equals(object? obj) => obj is LocalDateTime other && Equals(other);

    /// <summary>
    /// このインスタンスと指定した LocalDateTime が等しいかどうかを判定します
    /// </summary>
    public bool Equals(LocalDateTime other) => Value == other.Value;

    /// <summary>
    /// このインスタンスのハッシュコードを取得します
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// このインスタンスの文字列表現を取得します
    /// </summary>
    public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// このインスタンスと指定した LocalDateTime を比較します
    /// </summary>
    public int CompareTo(LocalDateTime other) => Value.CompareTo(other.Value);
}
