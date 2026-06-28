namespace SupportAdvance.Common.Clocks;

/// <summary>
/// 単体テスト用のモッククロック
/// テスト中に任意の日時を設定できます
/// </summary>
public class MockClock : IClock, IDisposable
{
    private DateTime _fixedDateTime;

    /// <summary>
    /// MockClock を初期化します
    /// </summary>
    /// <param name="fixedDateTime">固定する日時（デフォルト: 2024年1月1日 00:00:00）</param>
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
    /// 固定された日時を LocalDateTime で取得します
    /// </summary>
    public LocalDateTime JstNow => new(_fixedDateTime);

    /// <summary>
    /// 固定された日付を LocalDateTime で取得します（時刻は00:00:00）
    /// </summary>
    public LocalDateTime JstToday => new(new DateTime(_fixedDateTime.Year, _fixedDateTime.Month, _fixedDateTime.Day, 0,
        0, 0, DateTimeKind.Unspecified));

    public void Dispose()
    {
        // 特に必要な処理はないが、IDisposable を実装
    }

    /// <summary>
    /// テスト中に現在時刻を変更します
    /// </summary>
    /// <param name="newDateTime">新しい日時</param>
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
    /// テスト中に日時を進めます
    /// </summary>
    /// <param name="timeSpan">進める時間</param>
    public void Advance(TimeSpan timeSpan)
    {
        _fixedDateTime = _fixedDateTime.Add(timeSpan);
    }

    /// <summary>
    /// テスト中に時刻をリセットします
    /// </summary>
    /// <param name="resetDateTime">リセット後の日時（デフォルト: 2024年1月1日 00:00:00）</param>
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
