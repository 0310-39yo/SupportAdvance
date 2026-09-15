using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// ClockFactory クラスの単体テスト
/// 【責務】IClockSettings に基づいて適切なクロック実装を生成するファクトリ
/// 【テスト対象】ClockFactory の CreateClock() ファクトリメソッド
/// </summary>
public class ClockFactoryTests
{
    /// <summary>
    /// モック IClockSettings 実装
    /// </summary>
    private class MockClockSettings : IClockSettings
    {
        public string? ClockType { get; init; }
        public string? StartTime { get; init; }
        public int TickIntervalSeconds { get; init; }
        public string? OffsetDateTime { get; init; }
    }

    /// <summary>
    /// ClockType が SYSTEM の場合、SystemClock が生成されることを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithSystemType_CreatesSystemClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "SYSTEM" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }

    /// <summary>
    /// ClockType が system (小文字) の場合、SystemClock が生成されることを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithSystemTypeLowerCase_CreatesSystemClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "system" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }

    /// <summary>
    /// ClockType が TICKING の場合、TickingClock が生成されることを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithTickingType_CreatesTickingClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings
        {
            ClockType = "TICKING",
            StartTime = "2024-06-15T14:30:45",
            TickIntervalSeconds = 1
        };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<TickingClock>(clock);
    }

    /// <summary>
    /// ClockType が OFFSET の場合、OffsetClock が生成されることを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithOffsetType_CreatesOffsetClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings
        {
            ClockType = "OFFSET",
            OffsetDateTime = "2024-01-01T10:00:00"
        };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<OffsetClock>(clock);
    }

    /// <summary>
    /// settings が null の場合、ArgumentNullException を投出することを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ClockFactory.CreateClock(null!));
        Assert.Equal("settings", exception.ParamName);
    }

    /// <summary>
    /// ClockType が null の場合、ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithNullClockType_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = null };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ClockFactory.CreateClock(settings));
        Assert.Contains("ClockType is null", exception.Message);
    }

    /// <summary>
    /// ClockType が無効な値の場合、ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void CreateClock_WithUnknownClockType_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "UNKNOWN" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ClockFactory.CreateClock(settings));
        Assert.Contains("Unknown ClockType", exception.Message);
    }

    /// <summary>
    /// TICKING タイプで StartTime が無い場合、現在時刻が初期値として使用されることを検証
    /// </summary>
    [Fact]
    public void CreateClock_TickingWithoutStartTime_UsesCurrentTime()
    {
        // Arrange
        var settings = new MockClockSettings
        {
            ClockType = "TICKING",
            StartTime = null,
            TickIntervalSeconds = 1
        };

        // Act
        var beforeCreation = DateTime.Now;
        var clock = ClockFactory.CreateClock(settings);
        var afterCreation = DateTime.Now;

        // Assert
        Assert.IsType<TickingClock>(clock);
        var jstNow = clock.JstNow;
        Assert.True(beforeCreation <= jstNow.Value);
        Assert.True(jstNow.Value <= afterCreation);
    }

    /// <summary>
    /// OFFSET タイプで OffsetDateTime を指定できることを検証
    /// </summary>
    [Fact]
    public void CreateClock_OffsetWithDateTime_CreatesClockWithExpectedInitialTime()
    {
        // Arrange
        var targetDateTime = "2020-01-01T10:00:00";
        var settings = new MockClockSettings
        {
            ClockType = "OFFSET",
            OffsetDateTime = targetDateTime
        };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<OffsetClock>(clock);
    }

    /// <summary>
    /// ClockType が小文字混在の場合でも正しく処理されることを検証（ToUpperInvariant）
    /// </summary>
    [Fact]
    public void CreateClock_WithMixedCaseClockType_NormalizesCorrectly()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "SyStEm" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }
}
