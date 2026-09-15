using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// MockClock クラスの単体テスト
/// 【責務】テスト用に任意の固定時刻を提供するモック実装
/// 【テスト対象】MockClock のコンストラクタ、プロパティ、SetDateTime/Advance/Reset メソッド
/// </summary>
public class MockClockTests
{
    /// <summary>
    /// デフォルトコンストラクタで 2024 年 1 月 1 日に初期化されることを検証
    /// </summary>
    [Fact]
    public void Constructor_WithoutArgs_InitializesToDefaultDate()
    {
        // Act
        var clock = new MockClock();

        // Assert
        Assert.Equal(2024, clock.JstNow.Year);
        Assert.Equal(1, clock.JstNow.Month);
        Assert.Equal(1, clock.JstNow.Day);
        Assert.Equal(0, clock.JstNow.Hour);
        Assert.Equal(0, clock.JstNow.Minute);
        Assert.Equal(0, clock.JstNow.Second);
    }

    /// <summary>
    /// 指定した日時で初期化できることを検証
    /// </summary>
    [Fact]
    public void Constructor_WithSpecificDate_InitializesCorrectly()
    {
        // Arrange
        var fixedDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);

        // Act
        var clock = new MockClock(fixedDate);

        // Assert
        Assert.Equal(fixedDate, clock.JstNow.Value);
    }

    /// <summary>
    /// Local Kind の DateTime を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void Constructor_WithLocalKind_ThrowsArgumentException()
    {
        // Arrange
        var localDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Local);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MockClock(localDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// Utc Kind の DateTime を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void Constructor_WithUtcKind_ThrowsArgumentException()
    {
        // Arrange
        var utcDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MockClock(utcDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// JstNow が現在設定されている固定時刻を返すことを検証
    /// </summary>
    [Fact]
    public void JstNow_ReturnsFixedTime()
    {
        // Arrange
        var fixedDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDate);

        // Act
        var jstNow = clock.JstNow;

        // Assert
        Assert.Equal(fixedDate, jstNow.Value);
    }

    /// <summary>
    /// JstToday が当日の 00:00:00 を返すことを検証
    /// </summary>
    [Fact]
    public void JstToday_ReturnsDatePartOnly()
    {
        // Arrange
        var fixedDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDate);

        // Act
        var jstToday = clock.JstToday;

        // Assert
        Assert.Equal(2024, jstToday.Year);
        Assert.Equal(6, jstToday.Month);
        Assert.Equal(15, jstToday.Day);
        Assert.Equal(0, jstToday.Hour);
        Assert.Equal(0, jstToday.Minute);
        Assert.Equal(0, jstToday.Second);
    }

    /// <summary>
    /// SetDateTime で新しい時刻に変更できることを検証
    /// </summary>
    [Fact]
    public void SetDateTime_ChangesCurrentTime()
    {
        // Arrange
        var clock = new MockClock();
        var newDate = new DateTime(2024, 7, 20, 10, 15, 30, DateTimeKind.Unspecified);

        // Act
        clock.SetDateTime(newDate);

        // Assert
        Assert.Equal(newDate, clock.JstNow.Value);
    }

    /// <summary>
    /// SetDateTime で Local Kind を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void SetDateTime_WithLocalKind_ThrowsArgumentException()
    {
        // Arrange
        var clock = new MockClock();
        var localDate = new DateTime(2024, 7, 20, 10, 15, 30, DateTimeKind.Local);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => clock.SetDateTime(localDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// SetDateTime で Utc Kind を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void SetDateTime_WithUtcKind_ThrowsArgumentException()
    {
        // Arrange
        var clock = new MockClock();
        var utcDate = new DateTime(2024, 7, 20, 10, 15, 30, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => clock.SetDateTime(utcDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// Advance で指定した TimeSpan だけ時刻を進められることを検証
    /// </summary>
    [Fact]
    public void Advance_IncrementsTimeByTimeSpan()
    {
        // Arrange
        var fixedDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDate);
        var advance = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30));

        // Act
        clock.Advance(advance);

        // Assert
        var expected = new DateTime(2024, 6, 15, 16, 0, 45, DateTimeKind.Unspecified);
        Assert.Equal(expected, clock.JstNow.Value);
    }

    /// <summary>
    /// Advance で負の TimeSpan を指定すると時刻が戻ることを検証
    /// </summary>
    [Fact]
    public void Advance_WithNegativeTimeSpan_DecreasesTime()
    {
        // Arrange
        var fixedDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDate);
        var advance = TimeSpan.FromHours(-2);

        // Act
        clock.Advance(advance);

        // Assert
        var expected = new DateTime(2024, 6, 15, 12, 30, 45, DateTimeKind.Unspecified);
        Assert.Equal(expected, clock.JstNow.Value);
    }

    /// <summary>
    /// Reset でデフォルト日時にリセットできることを検証
    /// </summary>
    [Fact]
    public void Reset_WithoutArgs_ResetsToDefaultDate()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Unspecified));

        // Act
        clock.Reset();

        // Assert
        Assert.Equal(2024, clock.JstNow.Year);
        Assert.Equal(1, clock.JstNow.Month);
        Assert.Equal(1, clock.JstNow.Day);
        Assert.Equal(0, clock.JstNow.Hour);
    }

    /// <summary>
    /// Reset で指定した日時にリセットできることを検証
    /// </summary>
    [Fact]
    public void Reset_WithSpecificDate_ResetsToProvividedDate()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Unspecified));
        var resetDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        clock.Reset(resetDate);

        // Assert
        Assert.Equal(resetDate, clock.JstNow.Value);
    }

    /// <summary>
    /// Reset で Local Kind を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void Reset_WithLocalKind_ThrowsArgumentException()
    {
        // Arrange
        var clock = new MockClock();
        var localDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => clock.Reset(localDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// IClock インターフェースが正しく実装されていることを検証
    /// </summary>
    [Fact]
    public void MockClock_ImplementsIClock()
    {
        // Arrange
        var clock = new MockClock();

        // Act & Assert
        Assert.IsAssignableFrom<IClock>(clock);
    }

    /// <summary>
    /// IDisposable インターフェースが実装されていることを検証
    /// </summary>
    [Fact]
    public void MockClock_ImplementsIDisposable()
    {
        // Arrange
        var clock = new MockClock();

        // Act & Assert
        Assert.IsAssignableFrom<IDisposable>(clock);
    }
}
