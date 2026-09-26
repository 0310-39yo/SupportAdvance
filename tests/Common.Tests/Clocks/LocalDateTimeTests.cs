using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// LocalDateTime struct の単体テスト
/// 【責務】JST (日本標準時) の不変の日時値オブジェクトの検証
/// 【テスト対象】LocalDateTime struct のコンストラクタ、プロパティ、メソッド
/// </summary>
public class LocalDateTimeTests
{
    /// <summary>
    /// Unspecified Kind の DateTime を正常に構築できることを検証
    /// </summary>
    [Fact]
    public void Constructor_WithUnspecifiedKind_SucceedsCreation()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);

        // Act
        var localDateTime = new LocalDateTime(testDate);

        // Assert
        Assert.Equal(testDate, localDateTime.Value);
        Assert.Equal(DateTimeKind.Unspecified, localDateTime.Value.Kind);
    }

    /// <summary>
    /// Local Kind の DateTime を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void Constructor_WithLocalKind_ThrowsArgumentException()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Local);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LocalDateTime(testDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// Utc Kind の DateTime を指定すると ArgumentException を投出することを検証
    /// </summary>
    [Fact]
    public void Constructor_WithUtcKind_ThrowsArgumentException()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LocalDateTime(testDate));
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
    }

    /// <summary>
    /// Year プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Year_ReturnsCorrectYear()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(2024, localDateTime.Year);
    }

    /// <summary>
    /// Month プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Month_ReturnsCorrectMonth()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(6, localDateTime.Month);
    }

    /// <summary>
    /// Day プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Day_ReturnsCorrectDay()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(15, localDateTime.Day);
    }

    /// <summary>
    /// Hour プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Hour_ReturnsCorrectHour()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(14, localDateTime.Hour);
    }

    /// <summary>
    /// Minute プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Minute_ReturnsCorrectMinute()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(30, localDateTime.Minute);
    }

    /// <summary>
    /// Second プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Second_ReturnsCorrectSecond()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(45, localDateTime.Second);
    }

    /// <summary>
    /// Millisecond プロパティが正しく取得できることを検証
    /// </summary>
    [Fact]
    public void Millisecond_ReturnsCorrectMillisecond()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, 123, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act & Assert
        Assert.Equal(123, localDateTime.Millisecond);
    }

    /// <summary>
    /// Date プロパティが日付部分のみを取得し、時刻が 00:00:00 になることを検証
    /// </summary>
    [Fact]
    public void Date_ReturnsDatePartWithMidnight()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(testDate);

        // Act
        var dateOnly = localDateTime.Date;

        // Assert
        Assert.Equal(2024, dateOnly.Year);
        Assert.Equal(6, dateOnly.Month);
        Assert.Equal(15, dateOnly.Day);
        Assert.Equal(0, dateOnly.Hour);
        Assert.Equal(0, dateOnly.Minute);
        Assert.Equal(0, dateOnly.Second);
    }

    /// <summary>
    /// MinValue 定数が DateTime.MinValue を表すことを検証
    /// </summary>
    [Fact]
    public void MinValue_ReturnsDateTimeMinValue()
    {
        // Act & Assert
        Assert.Equal(DateTime.MinValue, LocalDateTime.MinValue.Value);
    }

    /// <summary>
    /// MaxValue 定数が DateTime.MaxValue を表すことを検証
    /// </summary>
    [Fact]
    public void MaxValue_ReturnsDateTimeMaxValue()
    {
        // Act & Assert
        Assert.Equal(DateTime.MaxValue, LocalDateTime.MaxValue.Value);
    }

    /// <summary>
    /// JST を UTC に正しく変換できることを検証
    /// </summary>
    [Fact]
    public void ToUtc_ConvertsJstToUtcCorrectly()
    {
        // JST 2024年6月15日 14:30:45 は UTC 2024年6月15日 05:30:45
        // Arrange
        var jstTime = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified);
        var localDateTime = new LocalDateTime(jstTime);

        // Act
        var utcTime = localDateTime.ToUtc();

        // Assert
        Assert.Equal(DateTimeKind.Utc, utcTime.Kind);
        // JST は UTC+9 なので、14:30 - 9時間 = 05:30
        Assert.Equal(5, utcTime.Hour);
        Assert.Equal(30, utcTime.Minute);
    }

    /// <summary>
    /// FromUtc が UTC を JST に正しく変換できることを検証
    /// </summary>
    [Fact]
    public void FromUtc_ConvertsUtcToJstCorrectly()
    {
        // Arrange
        var utcTime = new DateTime(2024, 6, 15, 5, 30, 45, DateTimeKind.Utc);

        // Act
        var localDateTime = LocalDateTime.FromUtc(utcTime);

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, localDateTime.Value.Kind);
        // UTC 05:30 + 9時間 = 14:30 JST
        Assert.Equal(14, localDateTime.Hour);
        Assert.Equal(30, localDateTime.Minute);
    }

    /// <summary>
    /// IComparable<LocalDateTime> インターフェース実装により、比較演算が機能することを検証
    /// </summary>
    [Fact]
    public void CompareTo_WorksCorrectly()
    {
        // Arrange
        var earlier = new LocalDateTime(new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Unspecified));
        var later = new LocalDateTime(new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Unspecified));

        // Act & Assert
        Assert.True(earlier.CompareTo(later) < 0);
        Assert.Equal(0, earlier.CompareTo(earlier));
        Assert.True(later.CompareTo(earlier) > 0);
    }

    /// <summary>
    /// IEquatable<LocalDateTime> インターフェース実装により、等値比較が機能することを検証
    /// </summary>
    [Fact]
    public void Equals_ImplementsIEquatable()
    {
        // Arrange
        var date1 = new LocalDateTime(new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified));
        var date2 = new LocalDateTime(new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Unspecified));
        var date3 = new LocalDateTime(new DateTime(2024, 6, 15, 15, 0, 0, DateTimeKind.Unspecified));

        // Act & Assert
        Assert.True(date1.Equals(date2));
        Assert.False(date1.Equals(date3));
    }
}
