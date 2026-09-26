namespace SupportAdvance.Common.Clocks;

/// <summary>
/// テスト・シミュレーション用のクロック実装
/// 指定した開始時刻から、一定間隔（ティック）ごとに時刻を進める
/// 手動進行モード: Tick() メソッドを明示的に呼び出し
/// 自動進行モード: Timer でバックグラウンド自動進行
/// </summary>
public class TickingClock : IClock, IDisposable
{
    private static readonly TimeZoneInfo JstTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly Lock _lockObject = new();
    private readonly TimeSpan _tickInterval;

    private Timer? _autoTickTimer;

    private DateTime _currentTime;
    private bool _disposed;
    private bool _isAutoTicking;

    /// <summary>
    /// TickingClockを初期化（手動進行モード）
    /// </summary>
    /// <param name="startTime">開始時刻</param>
    /// <param name="tickInterval">ティック間隔（デフォルト: 1秒）</param>
    public TickingClock(DateTime startTime, TimeSpan? tickInterval = null)
    {
        _currentTime = startTime;
        _tickInterval = tickInterval ?? TimeSpan.FromSeconds(1);
        _isAutoTicking = false;
    }

    /// <summary>
    /// 自動進行が有効かどうかを取得
    /// </summary>
    public bool IsAutoTicking
    {
        get
        {
            lock (_lockObject)
            {
                return _isAutoTicking;
            }
        }
    }

    /// <summary>
    /// 現在のJST日時を LocalDateTime で取得
    /// 自動進行モード時は、Timer による自動更新
    /// </summary>
    public LocalDateTime JstNow
    {
        get
        {
            lock (_lockObject)
            {
                return new LocalDateTime(_currentTime);
            }
        }
    }

    /// <summary>
    /// 本日（00:00:00）のJST日付を LocalDateTime で取得
    /// </summary>
    public LocalDateTime JstToday
    {
        get
        {
            lock (_lockObject)
            {
                return new LocalDateTime(new DateTime(_currentTime.Year, _currentTime.Month, _currentTime.Day, 0, 0, 0,
                    DateTimeKind.Unspecified));
            }
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

        StopAutoTick();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 自動進行を開始
    /// バックグラウンドで定期的に時刻を進める
    /// </summary>
    public void StartAutoTick()
    {
        lock (_lockObject)
        {
            if (_isAutoTicking)
            {
                return; // 既に自動進行中
            }

            _autoTickTimer = new Timer(
                _ => Tick(),
                null,
                _tickInterval,
                _tickInterval
            );
            _isAutoTicking = true;
        }
    }

    /// <summary>
    /// 自動進行を停止
    /// </summary>
    public void StopAutoTick()
    {
        lock (_lockObject)
        {
            if (!_isAutoTicking)
            {
                return; // 自動進行していない
            }

            _autoTickTimer?.Dispose();
            _autoTickTimer = null;
            _isAutoTicking = false;
        }
    }

    /// <summary>
    /// 時刻を一ティック進める（手動進行用）
    /// </summary>
    public void Tick()
    {
        lock (_lockObject)
        {
            _currentTime = _currentTime.Add(_tickInterval);
        }
    }

    /// <summary>
    /// 時刻を指定した量だけ進める
    /// </summary>
    /// <param name="count">ティック回数</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> が 0 未満の場合</exception>
    public void Tick(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);

        lock (_lockObject)
        {
            for (var i = 0; i < count; i++)
            {
                _currentTime = _currentTime.Add(_tickInterval);
            }
        }
    }

    /// <summary>
    /// 時刻を指定した量だけ進める
    /// </summary>
    /// <param name="timeSpan">進める時間</param>
    public void Advance(TimeSpan timeSpan)
    {
        lock (_lockObject)
        {
            _currentTime = _currentTime.Add(timeSpan);
        }
    }

    /// <summary>
    /// 時刻を指定した日時に設定
    /// </summary>
    /// <param name="dateTime">設定する日時</param>
    public void SetTime(DateTime dateTime)
    {
        lock (_lockObject)
        {
            _currentTime = dateTime;
        }
    }

    /// <summary>
    /// 時刻をリセット
    /// </summary>
    /// <param name="newStartTime">新しい開始時刻</param>
    public void Reset(DateTime newStartTime)
    {
        lock (_lockObject)
        {
            _currentTime = newStartTime;
        }
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~TickingClock()
    {
        Dispose();
    }
}
