using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Common.Tests.Clocks;

/// <summary>
/// OffsetClock クラスの単体テスト
/// 【責務】過去の日付を設定しながら、現在と同じ速度で時刻が進むクロック実装
/// 【テスト対象】OffsetClock のコンストラクタ、プロパティ、IDisposable 実装
/// </summary>
public class OffsetClockTests
{
    /// <summary>
    /// 指定した日時でオフセットクロックが初期化されることを検証
    /// </summary>
    [Fact]
    public void Constructor_InitializesWithOffsetDateTime()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);

        // Act
        var clock = new OffsetClock(offsetDate);

        // Assert
        var jstNow = clock.JstNow;
        // 初期化直後は、実システム時刻との経過時間が 0 に近いはずなので、指定時刻の値がほぼ返る
        Assert.Equal(DateTimeKind.Unspecified, jstNow.Value.Kind);
        Assert.Equal(offsetDate.Year, jstNow.Year);
        Assert.Equal(offsetDate.Month, jstNow.Month);
        Assert.Equal(offsetDate.Day, jstNow.Day);
    }

    /// <summary>
    /// JstNow が LocalDateTime 型で返されることを検証
    /// </summary>
    [Fact]
    public void JstNow_ReturnsLocalDateTime()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new OffsetClock(offsetDate);

        // Act
        var jstNow = clock.JstNow;

        // Assert
        Assert.IsType<LocalDateTime>(jstNow);
    }

    /// <summary>
    /// OffsetClock が IDisposable を実装していることを検証
    /// </summary>
    [Fact]
    public void OffsetClock_ImplementsIDisposable()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new OffsetClock(offsetDate);

        // Act & Assert
        Assert.IsAssignableFrom<IDisposable>(clock);
    }

    /// <summary>
    /// OffsetClock が IClock を実装していることを検証
    /// </summary>
    [Fact]
    public void OffsetClock_ImplementsIClock()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new OffsetClock(offsetDate);

        // Act & Assert
        Assert.IsAssignableFrom<IClock>(clock);
    }

    /// <summary>
    /// Dispose を複数回呼び出してもエラーが発生しないことを検証
    /// </summary>
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var clock = new OffsetClock(offsetDate);

        // Act & Assert
        clock.Dispose();
        clock.Dispose(); // 2回目の呼び出しもエラーなし
    }

    /// <summary>
    /// using ステートメント使用時に正常に Dispose されることを検証
    /// </summary>
    [Fact]
    public void OffsetClock_WorksWithUsing()
    {
        // Arrange
        var offsetDate = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);

        // Act & Assert
        using (var clock = new OffsetClock(offsetDate))
        {
            var jstNow = clock.JstNow;
            // LocalDateTime は value type なので、常に有効
        } // Dispose が呼ばれる
    }
}
