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
        public string? BusinessDayStartDate { get; init; }
        public string? BusinessDayStateFilePath { get; init; }
    }

    /// <summary>
    /// VO_FACTORY_01: ClockType が SYSTEM の場合、SystemClock が生成される
    /// </summary>
    [Fact]
    public void VO_FACTORY_01_WithSystemType_CreatesSystemClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "SYSTEM" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }

    /// <summary>
    /// VO_FACTORY_02: ClockType が system (小文字) の場合でも SystemClock が生成される
    /// </summary>
    [Fact]
    public void VO_FACTORY_02_WithSystemTypeLowerCase_CreatesSystemClockInstance()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "system" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }

    /// <summary>
    /// VO_FACTORY_03: ClockType が TICKING の場合、TickingClock が生成される
    /// </summary>
    [Fact]
    public void VO_FACTORY_03_WithTickingType_CreatesTickingClockInstance()
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
    /// VO_FACTORY_04: ClockType が OFFSET の場合、OffsetClock が生成される
    /// </summary>
    [Fact]
    public void VO_FACTORY_04_WithOffsetType_CreatesOffsetClockInstance()
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
    /// VO_FACTORY_05: ClockType が BusinessDay の場合、ON していない BusinessDayClock が基点日で生成される
    /// </summary>
    [Fact]
    public void VO_FACTORY_05_WithBusinessDayType_CreatesBusinessDayClockAtStartDate()
    {
        // Arrange（実環境の状態ファイルに触れないよう、存在しない一時パスを指定）
        var settings = new MockClockSettings
        {
            ClockType = "BusinessDay",
            BusinessDayStartDate = "2026-04-01",
            BusinessDayStateFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "state.json")
        };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        var businessDayClock = Assert.IsType<BusinessDayClock>(clock);
        Assert.False(businessDayClock.IsOn);
        Assert.Equal(new DateOnly(2026, 4, 1), businessDayClock.CurrentBusinessDate);
    }

    /// <summary>
    /// VO_ERROR_01: settings が null の場合、ArgumentNullException を投出
    /// </summary>
    [Fact]
    public void VO_ERROR_01_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ClockFactory.CreateClock(null!));
        Assert.Equal("settings", exception.ParamName);
    }

    /// <summary>
    /// VO_ERROR_02: ClockType が null の場合、ArgumentException を投出
    /// </summary>
    [Fact]
    public void VO_ERROR_02_WithNullClockType_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = null };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ClockFactory.CreateClock(settings));
        Assert.Contains("ClockType is null", exception.Message);
    }

    /// <summary>
    /// VO_ERROR_03: ClockType が無効な値の場合、ArgumentException を投出
    /// </summary>
    [Fact]
    public void VO_ERROR_03_WithUnknownClockType_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "UNKNOWN" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ClockFactory.CreateClock(settings));
        Assert.Contains("Unknown ClockType", exception.Message);
    }

    /// <summary>
    /// VO_EDGE_01: ClockType が小文字混在の場合でも正しく処理される（ToUpperInvariant）
    /// </summary>
    [Fact]
    public void VO_EDGE_01_WithMixedCaseClockType_NormalizesCorrectly()
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "SyStEm" };

        // Act
        var clock = ClockFactory.CreateClock(settings);

        // Assert
        Assert.IsType<SystemClock>(clock);
    }

    /// <summary>
    /// VO_EDGE_02: TICKING タイプで StartTime が null の場合、現在時刻が初期値として使用される
    /// </summary>
    [Fact]
    public void VO_EDGE_02_TickingWithoutStartTime_UsesCurrentTime()
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
    /// VO_EDGE_03: OFFSET タイプで OffsetDateTime を指定できる
    /// </summary>
    [Fact]
    public void VO_EDGE_03_OffsetWithDateTime_CreatesClockWithExpectedInitialTime()
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
    /// VO_ERROR_04: ClockType が BusinessDay で BusinessDayStartDate が未指定の場合、ArgumentException を投出
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VO_ERROR_04_WithBusinessDayTypeWithoutStartDate_ThrowsArgumentException(string? startDate)
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "BUSINESSDAY", BusinessDayStartDate = startDate };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ClockFactory.CreateClock(settings));
    }

    /// <summary>
    /// VO_ERROR_05: ClockType が BusinessDay で BusinessDayStartDate が yyyy-MM-dd 形式でない場合、FormatException を投出
    /// </summary>
    [Theory]
    [InlineData("2026/04/01")]
    [InlineData("2026-04-01T10:00:00")]
    [InlineData("invalid")]
    public void VO_ERROR_05_WithBusinessDayTypeWithInvalidStartDate_ThrowsFormatException(string startDate)
    {
        // Arrange
        var settings = new MockClockSettings { ClockType = "BusinessDay", BusinessDayStartDate = startDate };

        // Act & Assert
        Assert.Throws<FormatException>(() => ClockFactory.CreateClock(settings));
    }
}
