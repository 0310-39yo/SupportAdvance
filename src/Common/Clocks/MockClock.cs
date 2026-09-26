namespace SupportAdvance.Common.Clocks;

/// <summary>
/// 単体テスト用のモッククロック
/// テスト中の任意の日時の設定が可能
/// </summary>
public class MockClock : IClock, IDisposable
{
    private DateTime _fixedDateTime;

    /// <summary>
    /// MockClock の初期化
    /// </summary>
    /// <param name="fixedDateTime">固定する日時（デフォルト: 2024年1月1日 00:00:00）</param>
    /// <exception cref="ArgumentException"><paramref name="fixedDateTime"/> の <see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> 以外の場合</exception>
    public MockClock(DateTime? fixedDateTime = null)
    {
        if (fixedDateTime.HasValue && fixedDateTime.Value.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "MockClock は DateTimeKind.Unspecified である必要があります。",
                nameof(fixedDateTime));
        }

        _fixedDateTime = fixedDateTime ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// 固定された日時の LocalDateTime での取得
    /// </summary>
    public LocalDateTime JstNow => new(_fixedDateTime);

    /// <summary>
    /// 固定された日付を LocalDateTime で取得します（時刻は00:00:00）
    /// </summary>
    public LocalDateTime JstToday => new(new DateTime(_fixedDateTime.Year, _fixedDateTime.Month, _fixedDateTime.Day, 0,
        0, 0, DateTimeKind.Unspecified));

    /// <summary>
    /// 何もしない（解放するリソースなし）
    /// </summary>
    /// <remarks>
    /// <para>【用途】テストでの <c>using</c> 構文に対応するための実装</para>
    /// </remarks>
    public void Dispose()
    {
        // 特に必要な処理はないが、IDisposable を実装
    }

    /// <summary>
    /// テスト中に現在時刻の変更
    /// </summary>
    /// <param name="newDateTime">新しい日時</param>
    /// <exception cref="ArgumentException"><paramref name="newDateTime"/> の <see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> 以外の場合</exception>
    public void SetDateTime(DateTime newDateTime)
    {
        if (newDateTime.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "MockClock は DateTimeKind.Unspecified である必要があります。",
                nameof(newDateTime));
        }

        _fixedDateTime = newDateTime;
    }

    /// <summary>
    /// テスト中の日時の進行
    /// </summary>
    /// <param name="timeSpan">進める時間</param>
    public void Advance(TimeSpan timeSpan)
    {
        _fixedDateTime = _fixedDateTime.Add(timeSpan);
    }

    /// <summary>
    /// テスト中に時刻のリセット
    /// </summary>
    /// <param name="resetDateTime">リセット後の日時（デフォルト: 2024年1月1日 00:00:00）</param>
    /// <exception cref="ArgumentException"><paramref name="resetDateTime"/> の <see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> 以外の場合</exception>
    public void Reset(DateTime? resetDateTime = null)
    {
        if (resetDateTime.HasValue && resetDateTime.Value.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "MockClock は DateTimeKind.Unspecified である必要があります。",
                nameof(resetDateTime));
        }

        _fixedDateTime = resetDateTime ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    }
}
