namespace SupportAdvance.Common.Clocks;

/// <summary>
/// ON／OFF のたびに業務日を 1 日ずつ進める、検証用のクロック
/// </summary>
/// <remarks>
/// <para>【動作】年月日は基点日から始まり、ON（前回終了した業務日の翌日から開始）と、ON 中の 0 時越えによってのみ増加。時刻は常に実際の現在時刻（JST）</para>
/// <list type="bullet">
/// <item><description>一番最初の ON の前: 基点日</description></item>
/// <item><description>ON 中: ON したときの業務日 ＋ ON してから越えた 0 時の回数</description></item>
/// <item><description>OFF 中、および再起動後の ON の前: 最後に終了した業務日（固定）</description></item>
/// </list>
/// <para>【設計】<see cref="OffsetClock"/> とは独立したクロック。最後に終了した業務日は状態ファイルに保存し、アプリケーションを再起動しても続きから進行</para>
/// <para>【重要】検証（Development／Test 環境）専用。本番環境では <see cref="SystemClock"/> を使用</para>
/// <para>【スレッド安全性】すべての状態の読み書きを 1 つのロックで保護（ボタン操作とログ出力の同時呼び出しに対応）</para>
/// <para>【参照】docs/Common/Clocks/BusinessDayClock_技術仕様書.md、BusinessDayClock_詳細設計書.md</para>
/// </remarks>
public sealed class BusinessDayClock : IClock, IBusinessDayClockControl, IDisposable
{
    private static readonly TimeZoneInfo JstTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly Lock _lock = new();
    private readonly TimeProvider _timeProvider;
    private readonly BusinessDayClockStateFile _stateFile;
    private readonly DateOnly _startDate;

    private DateOnly? _lastEndedDate;
    private bool _isOn;
    private DateOnly _sessionDate;
    private DateOnly _onSystemDate;
    private bool _disposed;

    /// <summary>
    /// <see cref="BusinessDayClock"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="startDate">基点日（一番最初の ON で始まる業務日）</param>
    /// <param name="stateFilePath">状態ファイルのパス。<see langword="null"/> の場合は <see cref="DefaultStateFilePath"/></param>
    /// <param name="timeProvider">実際の現在時刻の取得元。<see langword="null"/> の場合は <see cref="TimeProvider.System"/>（テストでは偽の時刻に差し替え）</param>
    /// <remarks>
    /// <para>【注意】コンストラクターでは ON しない。ON のきっかけは Composition Root が決定</para>
    /// <para>【状態の復元】状態ファイルがない・壊れている・保存時の基点日が <paramref name="startDate"/> と異なる場合は、最後に終了した業務日なし（基点日から開始）。
    /// ON のまま異常終了した跡がある場合は、そのとき ON していた業務日を最後に終了した業務日とみなす</para>
    /// </remarks>
    public BusinessDayClock(DateOnly startDate, string? stateFilePath = null, TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _stateFile = new BusinessDayClockStateFile(stateFilePath ?? DefaultStateFilePath);
        _startDate = startDate;

        var state = _stateFile.Load();
        if (state is null || state.StartDate != startDate)
        {
            _lastEndedDate = null;
        }
        else if (state.IsOn)
        {
            _lastEndedDate = state.SessionDate ?? state.LastEndedDate;
        }
        else
        {
            _lastEndedDate = state.LastEndedDate;
        }
    }

    /// <summary>
    /// 状態ファイルの既定のパス
    /// </summary>
    /// <value><c>%LOCALAPPDATA%\SupportAdvance\BusinessDayClock.json</c></value>
    public static string DefaultStateFilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SupportAdvance",
            "BusinessDayClock.json");

    /// <summary>
    /// 現在の業務日時（JST）
    /// </summary>
    /// <value>業務日 ＋ 実際の現在時刻。<see cref="DateTime.Kind"/> は <see cref="DateTimeKind.Unspecified"/></value>
    /// <remarks>
    /// <para>【重要】ON の前・OFF 中・破棄後も例外なし（ログ出力が本プロパティを使用するため）</para>
    /// </remarks>
    public LocalDateTime JstNow
    {
        get
        {
            var systemNow = GetSystemJstNow();
            lock (_lock)
            {
                var businessDate = ComputeBusinessDate(systemNow);
                return new LocalDateTime(businessDate.ToDateTime(TimeOnly.FromDateTime(systemNow)));
            }
        }
    }

    /// <summary>
    /// 現在の業務日の 0 時（JST）
    /// </summary>
    /// <value><see cref="CurrentBusinessDate"/> の 00:00:00</value>
    public LocalDateTime JstToday
    {
        get
        {
            var systemNow = GetSystemJstNow();
            lock (_lock)
            {
                return new LocalDateTime(ComputeBusinessDate(systemNow).ToDateTime(TimeOnly.MinValue));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsOn
    {
        get
        {
            lock (_lock)
            {
                return _isOn;
            }
        }
    }

    /// <inheritdoc/>
    public DateOnly CurrentBusinessDate
    {
        get
        {
            var systemNow = GetSystemJstNow();
            lock (_lock)
            {
                return ComputeBusinessDate(systemNow);
            }
        }
    }

    /// <inheritdoc/>
    public void TurnOn()
    {
        var systemNow = GetSystemJstNow();
        lock (_lock)
        {
            if (_isOn)
            {
                return;
            }

            var sessionDate = _lastEndedDate?.AddDays(1) ?? _startDate;

            // 保存に失敗した場合は状態を変えない（先に保存してから反映）
            _stateFile.Save(new BusinessDayClockState(_startDate, _lastEndedDate, IsOn: true, SessionDate: sessionDate));

            _sessionDate = sessionDate;
            _onSystemDate = DateOnly.FromDateTime(systemNow);
            _isOn = true;
        }
    }

    /// <inheritdoc/>
    public void TurnOff()
    {
        var systemNow = GetSystemJstNow();
        lock (_lock)
        {
            if (!_isOn)
            {
                return;
            }

            var endedDate = ComputeBusinessDate(systemNow);

            // 保存に失敗した場合は状態を変えない（先に保存してから反映）
            _stateFile.Save(new BusinessDayClockState(_startDate, endedDate, IsOn: false, SessionDate: null));

            _lastEndedDate = endedDate;
            _isOn = false;
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            _stateFile.Delete();

            _lastEndedDate = null;
            _isOn = false;
        }
    }

    /// <summary>
    /// リソースの解放
    /// </summary>
    /// <remarks>
    /// <para>【注意】OFF は行わない。DI コンテナーはインスタンスとして登録したシングルトンを破棄しないため、OFF は Composition Root が明示的に呼び出す</para>
    /// <para>【注意】破棄後も <see cref="JstNow"/> は値を返す（終了処理中のログ出力のため）</para>
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    /// <summary>
    /// 状態と実際の日時からの業務日の算出（ロック内で呼び出すこと）
    /// </summary>
    /// <param name="systemJstNow">実際の現在日時（JST）</param>
    /// <returns>現在の業務日</returns>
    private DateOnly ComputeBusinessDate(DateTime systemJstNow)
    {
        if (_isOn)
        {
            // 実際の日付が ON 時より前に戻った場合は 0 日（業務日を戻さない）
            var crossedDays = Math.Max(0, DateOnly.FromDateTime(systemJstNow).DayNumber - _onSystemDate.DayNumber);
            return _sessionDate.AddDays(crossedDays);
        }

        return _lastEndedDate ?? _startDate;
    }

    /// <summary>
    /// 実際の現在日時（JST）の取得
    /// </summary>
    /// <returns><see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> の JST 日時</returns>
    private DateTime GetSystemJstNow()
    {
        var jstNow = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), JstTimeZone);
        return DateTime.SpecifyKind(jstNow.DateTime, DateTimeKind.Unspecified);
    }
}
