using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// BusinessDayClock の単体テスト
/// </summary>
/// <remarks>
/// <para>【方針】実際の時刻は偽の <see cref="TimeProvider"/> で差し替え、0 時越え・再起動を時間を待たずに再現</para>
/// <para>【状態ファイル】テストごとに一時フォルダーに作成し、終了時に削除</para>
/// <para>【参照】docs/Common/Clocks/BusinessDayClock_技術仕様書.md §1.1（動作の概要）</para>
/// </remarks>
public sealed class BusinessDayClockTests : IDisposable
{
    private static readonly DateOnly StartDate = new(2026, 4, 1);

    private readonly string _directory;
    private readonly string _stateFilePath;
    private readonly FakeTimeProvider _time;

    /// <summary>
    /// テストごとの一時フォルダーと偽の時刻（JST 2026-09-18 10:00:00）の準備
    /// </summary>
    public BusinessDayClockTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "BusinessDayClockTests", Guid.NewGuid().ToString("N"));
        _stateFilePath = Path.Combine(_directory, "BusinessDayClock.json");
        _time = new FakeTimeProvider(Jst(2026, 9, 18, 10, 0));
    }

    /// <summary>
    /// 一時フォルダーの削除
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    #region グループ 1: 一番最初の ON の前

    [Fact]
    public void VO_INIT_01_BeforeFirstTurnOn_ReturnsStartDateWithCurrentTime()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        var now = clock.JstNow;

        // Assert
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0), now.Value);
        Assert.False(clock.IsOn);
    }

    [Fact]
    public void VO_INIT_02_JstNow_KindIsUnspecified_AndKeepsSecondsOfCurrentTime()
    {
        // Arrange
        _time.SetJst(2026, 9, 18, 13, 45, 30);
        var clock = CreateClock();

        // Act
        var now = clock.JstNow;

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, now.Value.Kind);
        Assert.Equal(new DateTime(2026, 4, 1, 13, 45, 30), now.Value);
    }

    [Fact]
    public void VO_INIT_03_JstToday_ReturnsMidnightOfBusinessDate()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        var today = clock.JstToday;

        // Assert
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0), today.Value);
    }

    #endregion

    #region グループ 2: ON

    [Fact]
    public void VO_ON_01_FirstTurnOn_StartsFromStartDate()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        clock.TurnOn();

        // Assert
        Assert.True(clock.IsOn);
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0), clock.JstNow.Value);
    }

    [Fact]
    public void VO_ON_02_TurnOnWhileOn_DoesNotAdvanceDate()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();

        // Act
        clock.TurnOn();

        // Assert
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_ON_03_TimePartFollowsCurrentTime()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();

        // Act
        _time.SetJst(2026, 9, 18, 15, 30);

        // Assert
        Assert.Equal(new DateTime(2026, 4, 1, 15, 30, 0), clock.JstNow.Value);
    }

    #endregion

    #region グループ 3: ON 中の 0 時越え

    [Fact]
    public void VO_MIDNIGHT_01_CrossingMidnightWhileOn_AdvancesToNextDay()
    {
        // Arrange
        var clock = CreateClock();
        _time.SetJst(2026, 9, 18, 23, 59);
        clock.TurnOn();

        // Act
        _time.SetJst(2026, 9, 19, 0, 1);

        // Assert
        Assert.Equal(new DateTime(2026, 4, 2, 0, 1, 0), clock.JstNow.Value);
    }

    [Fact]
    public void VO_MIDNIGHT_02_SeveralDaysWhileOn_AdvancesByCrossedDays()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();

        // Act
        _time.SetJst(2026, 9, 21, 9, 0);

        // Assert
        Assert.Equal(new DateOnly(2026, 4, 4), clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_MIDNIGHT_03_SystemDateGoesBack_DoesNotGoBackBusinessDate()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();

        // Act
        _time.SetJst(2026, 9, 17, 10, 0);

        // Assert
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_MIDNIGHT_04_UtcToJstConversion_CrossesDateAtJstMidnight()
    {
        // Arrange（UTC 14:59 = JST 23:59）
        var clock = CreateClock();
        _time.UtcNow = new DateTimeOffset(2026, 9, 18, 14, 59, 0, TimeSpan.Zero);
        clock.TurnOn();

        // Act（UTC 15:00 = JST 翌日 0:00）
        _time.UtcNow = new DateTimeOffset(2026, 9, 18, 15, 0, 0, TimeSpan.Zero);

        // Assert
        Assert.Equal(new DateTime(2026, 4, 2, 0, 0, 0), clock.JstNow.Value);
    }

    #endregion

    #region グループ 4: OFF と次の ON

    [Fact]
    public void VO_OFF_01_AfterTurnOff_DateStaysFixed()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();
        clock.TurnOff();

        // Act
        _time.SetJst(2026, 9, 20, 8, 0);

        // Assert
        Assert.False(clock.IsOn);
        Assert.Equal(new DateTime(2026, 4, 1, 8, 0, 0), clock.JstNow.Value);
    }

    [Fact]
    public void VO_OFF_02_TurnOnAfterTurnOff_StartsFromNextDay()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();
        clock.TurnOff();

        // Act
        clock.TurnOn();

        // Assert
        Assert.Equal(new DateOnly(2026, 4, 2), clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_OFF_03_RepeatedOnOff_AdvancesOneDayEachTime()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        for (var i = 0; i < 5; i++)
        {
            clock.TurnOn();
            clock.TurnOff();
        }

        clock.TurnOn();

        // Assert（5 日分終了した後の 6 日目）
        Assert.Equal(new DateOnly(2026, 4, 6), clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_OFF_04_TurnOffAfterMidnight_NextTurnOnStartsFromDayAfterEndedDate()
    {
        // Arrange（技術仕様書 §1.1 の例）
        var clock = CreateClock();
        clock.TurnOn();
        _time.SetJst(2026, 9, 19, 0, 30);
        clock.TurnOff();

        // Act
        _time.SetJst(2026, 9, 19, 9, 0);
        clock.TurnOn();

        // Assert
        Assert.Equal(new DateTime(2026, 4, 3, 9, 0, 0), clock.JstNow.Value);
    }

    [Fact]
    public void VO_OFF_05_TurnOffBeforeFirstTurnOn_DoesNothing()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        clock.TurnOff();
        var fileCreatedByTurnOff = File.Exists(_stateFilePath);
        clock.TurnOn();

        // Assert（業務日は進まず、状態ファイルも作られない）
        Assert.False(fileCreatedByTurnOff);
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
    }

    #endregion

    #region グループ 5: 状態ファイル（再起動）

    [Fact]
    public void VO_PERSIST_01_Restart_BeforeTurnOn_ReturnsLastEndedDate()
    {
        // Arrange
        var first = CreateClock();
        first.TurnOn();
        first.TurnOff();

        // Act
        var restarted = CreateClock();

        // Assert
        Assert.False(restarted.IsOn);
        Assert.Equal(StartDate, restarted.CurrentBusinessDate);
    }

    [Fact]
    public void VO_PERSIST_02_Restart_TurnOn_StartsFromNextDay()
    {
        // Arrange
        var first = CreateClock();
        first.TurnOn();
        first.TurnOff();

        // Act
        var restarted = CreateClock();
        restarted.TurnOn();

        // Assert
        Assert.Equal(new DateOnly(2026, 4, 2), restarted.CurrentBusinessDate);
    }

    [Fact]
    public void VO_PERSIST_03_AbnormalTerminationWhileOn_NextTurnOnStartsFromDayAfterSessionDate()
    {
        // Arrange（ON のまま OFF せずに終了）
        var first = CreateClock();
        first.TurnOn();

        // Act
        var restarted = CreateClock();
        restarted.TurnOn();

        // Assert
        Assert.Equal(new DateOnly(2026, 4, 2), restarted.CurrentBusinessDate);
    }

    [Fact]
    public void VO_PERSIST_04_StartDateChanged_StartsOverFromNewStartDate()
    {
        // Arrange
        var first = CreateClock();
        first.TurnOn();
        first.TurnOff();

        // Act
        var restarted = new BusinessDayClock(new DateOnly(2027, 1, 1), _stateFilePath, _time);
        restarted.TurnOn();

        // Assert
        Assert.Equal(new DateOnly(2027, 1, 1), restarted.CurrentBusinessDate);
    }

    [Fact]
    public void VO_PERSIST_05_CorruptedStateFile_StartsFromStartDate()
    {
        // Arrange
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_stateFilePath, "{ this is not json");

        // Act
        var clock = CreateClock();
        clock.TurnOn();

        // Assert
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
    }

    [Fact]
    public void VO_PERSIST_06_TurnOnAndTurnOff_WriteStateFile()
    {
        // Arrange
        var clock = CreateClock();

        // Act
        clock.TurnOn();
        var existsAfterOn = File.Exists(_stateFilePath);
        clock.TurnOff();

        // Assert
        Assert.True(existsAfterOn);
        Assert.Contains("\"lastEndedDate\": \"2026-04-01\"", File.ReadAllText(_stateFilePath));
    }

    #endregion

    #region グループ 6: やり直し

    [Fact]
    public void VO_RESET_01_Reset_ReturnsToStartDateAndDeletesStateFile()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();
        clock.TurnOff();
        clock.TurnOn();

        // Act
        clock.Reset();

        // Assert
        Assert.False(clock.IsOn);
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
        Assert.False(File.Exists(_stateFilePath));
    }

    [Fact]
    public void VO_RESET_02_TurnOnAfterReset_StartsFromStartDate()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();
        clock.TurnOff();
        clock.Reset();

        // Act
        clock.TurnOn();

        // Assert
        Assert.Equal(StartDate, clock.CurrentBusinessDate);
    }

    #endregion

    #region グループ 7: インターフェースと破棄

    [Fact]
    public void VO_TYPE_01_ImplementsIClockAndControl()
    {
        // Arrange
        var clock = CreateClock();

        // Assert
        Assert.IsAssignableFrom<IClock>(clock);
        Assert.IsAssignableFrom<IBusinessDayClockControl>(clock);
        Assert.IsAssignableFrom<IDisposable>(clock);
    }

    [Fact]
    public void VO_TYPE_02_AfterDispose_JstNowStillReturnsValue()
    {
        // Arrange
        var clock = CreateClock();
        clock.TurnOn();

        // Act
        clock.Dispose();
        clock.Dispose();

        // Assert
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0), clock.JstNow.Value);
    }

    #endregion

    private BusinessDayClock CreateClock() => new(StartDate, _stateFilePath, _time);

    private static DateTimeOffset Jst(int year, int month, int day, int hour, int minute, int second = 0) =>
        new(year, month, day, hour, minute, second, TimeSpan.FromHours(9));

    /// <summary>
    /// テスト用の偽の時刻
    /// </summary>
    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = now.ToUniversalTime();

        public void SetJst(int year, int month, int day, int hour, int minute, int second = 0) =>
            UtcNow = Jst(year, month, day, hour, minute, second).ToUniversalTime();

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
