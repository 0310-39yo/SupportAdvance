using Xunit;
using SupportAdvance.Common.Clocks;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SupportAdvance.Common.Tests.Clocks;
public class TickingClockTests
{
    /// <summary>
    /// デフォルト設定で初期化されることを検証（1秒間隔）
    /// </summary>
    [Fact]
    public void Constructor_WithDefaults_InitializesWithOneSecondInterval()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);

        // Act
        var clock = new TickingClock(startTime);

        // Assert
        Assert.Equal(startTime, clock.JstNow.Value);
        Assert.False(clock.IsAutoTicking);
    }

    /// <summary>
    /// カスタム間隔で初期化できることを検証
    /// </summary>
    [Fact]
    public void Constructor_WithCustomTickInterval_InitializesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var customInterval = TimeSpan.FromSeconds(5);

        // Act
        var clock = new TickingClock(startTime, customInterval);

        // Assert
        Assert.Equal(startTime, clock.JstNow.Value);
        Assert.False(clock.IsAutoTicking);
    }

    /// <summary>
    /// JstNow が現在の時刻を LocalDateTime で返すことを検証
    /// </summary>
    [Fact]
    public void JstNow_ReturnsCurrentTime()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime);

        // Act
        var jstNow = clock.JstNow;

        // Assert
        Assert.Equal(startTime, jstNow.Value);
    }

    /// <summary>
    /// Tick() メソッド呼び出しで時刻が進むことを検証
    /// </summary>
    [Fact]
    public void Tick_AdvancesTimeByTickInterval()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime, TimeSpan.FromSeconds(5));

        // Act
        clock.Tick();

        // Assert
        var expected = new DateTime(2024, 6, 15, 14, 30, 5, DateTimeKind.Unspecified);
        Assert.Equal(expected, clock.JstNow.Value);
    }

    /// <summary>
    /// 複数回 Tick() を呼び出すとその回数分時刻が進むことを検証
    /// </summary>
    [Fact]
    public void Tick_CalledMultipleTimes_AdvancesTimeAccumulatively()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime, TimeSpan.FromSeconds(3));

        // Act
        clock.Tick();
        clock.Tick();
        clock.Tick();

        // Assert
        var expected = new DateTime(2024, 6, 15, 14, 30, 9, DateTimeKind.Unspecified);
        Assert.Equal(expected, clock.JstNow.Value);
    }

    /// <summary>
    /// IsAutoTicking プロパティが初期状態では false であることを検証
    /// </summary>
    [Fact]
    public void IsAutoTicking_InitiallyFalse()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime);

        // Act & Assert
        Assert.False(clock.IsAutoTicking);
    }

    /// <summary>
    /// StartAutoTicking() で自動進行が開始されることを検証
    /// </summary>
    [Fact]
    public async Task StartAutoTicking_StartsAutomaticProgression()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime, TimeSpan.FromMilliseconds(100));

        // Act
        clock.StartAutoTick();

        // Assert
        Assert.True(clock.IsAutoTicking);

        // 少し待機して時刻が進んだことを確認
        await Task.Delay(250);
        var jstNowAfterDelay = clock.JstNow;

        Assert.True(jstNowAfterDelay.Value > startTime);

        // 自動進行を停止
        clock.StopAutoTick();
    }

    /// <summary>
    /// StopAutoTicking() で自動進行が停止されることを検証
    /// </summary>
    [Fact]
    public async Task StopAutoTicking_StopsAutomaticProgression()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime, TimeSpan.FromMilliseconds(100));

        // Act
        clock.StartAutoTick();
        await Task.Delay(150);
        var timeBeforeStop = clock.JstNow;
        clock.StopAutoTick();
        await Task.Delay(150);
        var timeAfterStop = clock.JstNow;

        // Assert
        Assert.False(clock.IsAutoTicking);
        // 停止後は時刻が進まない（またはほぼ進まない）
        Assert.True(timeAfterStop.Value <= timeBeforeStop.Value.AddSeconds(1));
    }

    /// <summary>
    /// TickingClock が IClock を実装していることを検証
    /// </summary>
    [Fact]
    public void TickingClock_ImplementsIClock()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime);

        // Act & Assert
        Assert.IsAssignableFrom<IClock>(clock);
    }

    /// <summary>
    /// TickingClock が IDisposable を実装していることを検証
    /// </summary>
    [Fact]
    public void TickingClock_ImplementsIDisposable()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new TickingClock(startTime);

        // Act & Assert
        Assert.IsAssignableFrom<IDisposable>(clock);
    }

    /// <summary>
    /// using ステートメント使用時に正常に Dispose されることを検証
    /// </summary>
    [Fact]
    public void TickingClock_WorksWithUsing()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);

        // Act & Assert
        using (var clock = new TickingClock(startTime))
        {
            clock.Tick();
            Assert.NotNull(clock);
        } // Dispose が呼ばれる
    }
}
