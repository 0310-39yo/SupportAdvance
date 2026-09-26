namespace SupportAdvance.Common.Clocks;

/// <summary>
/// オフセット付きクロック実装
/// 過去の日付を設定しながら、現在と同じ速度で時刻が進む
///
/// 例: 2020年1月1日として、実際の時刻と同じ速度で進行
/// </summary>
public class OffsetClock : IClock, IDisposable
{
    private static readonly TimeZoneInfo JstTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly DateTime _offsetDateTime;
    private readonly DateTime _systemBaseTime;
    private bool _disposed;

    /// <summary>
    /// OffsetClockを初期化
    /// </summary>
    /// <param name="offsetDateTime">表示する日時（例：2020年1月1日 10:30:00）</param>
    /// <remarks>
    /// システム時刻とのオフセットの計算。
    /// 以降、現在のシステム時刻と同じ速度での時刻の進行
    /// </remarks>
    public OffsetClock(DateTime offsetDateTime)
    {
        _offsetDateTime = offsetDateTime;
        _systemBaseTime = DateTime.Now;
    }

    /// <summary>
    /// 現在のJST日時を LocalDateTime で取得
    /// オフセット日時 + (現在のシステム時刻 - システム基準時刻) = 表示時刻
    /// </summary>
    public LocalDateTime JstNow
    {
        get
        {
            // 現在のシステム時刻からの経過時間を計算
            var elapsed = DateTime.Now - _systemBaseTime;

            // オフセット日時 + 経過時間 = 表示時刻
            var displayTime = _offsetDateTime.Add(elapsed);

            return new LocalDateTime(DateTime.SpecifyKind(displayTime, DateTimeKind.Unspecified));
        }
    }

    /// <summary>
    /// 本日（00:00:00）のJST日付を LocalDateTime で取得
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

    /// <summary>
    /// リソースを解放
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~OffsetClock()
    {
        Dispose();
    }
}
