using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Domain.Tests.ValueObjects;

/// <summary>
/// <see cref="LoggedOutAt"/> の単体テスト
/// </summary>
public class LoggedOutAtTests
{
    private static readonly LocalDateTime Sample = new(new DateTime(2026, 9, 26, 17, 0, 0, DateTimeKind.Unspecified));

    #region Unset / From

    [Fact]
    public void Unset_ReturnsInstanceWithoutLogout()
    {
        var result = LoggedOutAt.Unset();

        Assert.NotNull(result);
        Assert.False(result.HasLoggedOut);
        Assert.False(result.IsSet);
        Assert.Equal(LocalDateTime.MinValue, result.Value);
    }

    [Fact]
    public void From_WithValidDateTime_ReturnsSetInstance()
    {
        var result = LoggedOutAt.From(Sample);

        Assert.True(result.HasLoggedOut);
        Assert.True(result.IsSet);
        Assert.Equal(Sample, result.Value);
    }

    #endregion

    #region TryFrom

    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        var success = LoggedOutAt.TryFrom(null, out var result);

        Assert.True(success);
        Assert.False(result.HasLoggedOut);
    }

    [Fact]
    public void TryFrom_WithValidDateTime_ReturnsTrueAndSet()
    {
        var success = LoggedOutAt.TryFrom(Sample, out var result);

        Assert.True(success);
        Assert.True(result.HasLoggedOut);
        Assert.Equal(Sample, result.Value);
    }

    [Fact]
    public void TryFrom_WithMinValue_ReturnsFalse()
    {
        var success = LoggedOutAt.TryFrom(LocalDateTime.MinValue, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFrom_WithMaxValue_ReturnsFalse()
    {
        var success = LoggedOutAt.TryFrom(LocalDateTime.MaxValue, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    #endregion

    #region Equals / GetHashCode / ToString

    [Fact]
    public void Equals_SameSetValue_ReturnsTrue()
    {
        Assert.Equal(LoggedOutAt.From(Sample), LoggedOutAt.From(Sample));
        Assert.Equal(LoggedOutAt.From(Sample).GetHashCode(), LoggedOutAt.From(Sample).GetHashCode());
    }

    [Fact]
    public void Equals_TwoUnset_ReturnsTrue()
    {
        Assert.Equal(LoggedOutAt.Unset(), LoggedOutAt.Unset());
    }

    [Fact]
    public void Equals_UnsetAndSet_ReturnsFalse()
    {
        Assert.NotEqual(LoggedOutAt.Unset(), LoggedOutAt.From(Sample));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var other = new LocalDateTime(new DateTime(2026, 9, 26, 18, 0, 0, DateTimeKind.Unspecified));

        Assert.NotEqual(LoggedOutAt.From(Sample), LoggedOutAt.From(other));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(LoggedOutAt.Unset().Equals((LoggedOutAt?)null));
        Assert.False(LoggedOutAt.Unset().Equals((object?)null));
    }

    [Fact]
    public void ToString_Unset_ReturnsNotLoggedOutLabel()
    {
        Assert.Equal("未ログアウト", LoggedOutAt.Unset().ToString());
    }

    [Fact]
    public void ToString_Set_ReturnsDateTimeString()
    {
        Assert.Equal(Sample.ToString(), LoggedOutAt.From(Sample).ToString());
    }

    #endregion
}
