using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// SystemClock クラスの単体テスト
/// 【責務】実行時のシステムクロックから現在時刻を取得する
/// 【テスト対象】SystemClock の JstNow / JstToday プロパティ
/// </summary>
public class SystemClockTests
{
    /// <summary>
    /// JstNow が現在の JST 時刻を LocalDateTime で返すことを検証
    /// </summary>
    [Fact]
    public void JstNow_ReturnsCurrentJstTime()
    {
        // Arrange
        var clock = new SystemClock();
        var beforeTest = DateTime.Now;

        // Act
        var jstNow = clock.JstNow;

        var afterTest = DateTime.Now;

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, jstNow.Value.Kind);
        // 時刻が beforeTest と afterTest の間（前後5秒）に収まることを検証
        Assert.True(beforeTest.AddSeconds(-5) <= jstNow.Value);
        Assert.True(jstNow.Value <= afterTest.AddSeconds(5));
    }

    /// <summary>
    /// JstToday が当日の 00:00:00 を LocalDateTime で返すことを検証
    /// </summary>
    [Fact]
    public void JstToday_ReturnsCurrentDayAtMidnight()
    {
        // Arrange
        var clock = new SystemClock();
        var expectedDate = DateTime.Today;

        // Act
        var jstToday = clock.JstToday;

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, jstToday.Value.Kind);
        Assert.Equal(expectedDate.Year, jstToday.Year);
        Assert.Equal(expectedDate.Month, jstToday.Month);
        Assert.Equal(expectedDate.Day, jstToday.Day);
        Assert.Equal(0, jstToday.Hour);
        Assert.Equal(0, jstToday.Minute);
        Assert.Equal(0, jstToday.Second);
    }

    /// <summary>
    /// 複数呼び出しで JstNow が前回よりも同じか遅い時刻を返すことを検証（単調性）
    /// </summary>
    [Fact]
    public void JstNow_IsMonotonicallyIncreasing()
    {
        // Arrange
        var clock = new SystemClock();

        // Act
        var firstCall = clock.JstNow;
        var secondCall = clock.JstNow;

        // Assert
        // firstCall <= secondCall でなければならない（時刻は進むか同じ）
        Assert.True(firstCall.CompareTo(secondCall) <= 0);
    }

    /// <summary>
    /// JstToday が複数呼び出しで同じ値を返すことを検証（同じ日付内での呼び出し）
    /// </summary>
    [Fact]
    public void JstToday_ConsistentWithinSameDay()
    {
        // Arrange
        var clock = new SystemClock();

        // Act
        var firstCall = clock.JstToday;
        var secondCall = clock.JstToday;

        // Assert
        Assert.Equal(firstCall.Value, secondCall.Value);
    }

    /// <summary>
    /// IClock インターフェースが正しく実装されていることを検証
    /// </summary>
    [Fact]
    public void SystemClock_ImplementsIClock()
    {
        // Arrange
        var clock = new SystemClock();

        // Act & Assert
        Assert.IsAssignableFrom<IClock>(clock);
    }
}
