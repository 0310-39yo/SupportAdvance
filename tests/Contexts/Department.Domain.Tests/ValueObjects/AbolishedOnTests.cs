namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;

public class AbolishedOnTests
{
    [Fact]
    public void Unset_ReturnsInstance()
    {
        var abolishedOn = AbolishedOn.Unset();
        Assert.False(abolishedOn.IsAbolished);
    }

    [Fact]
    public void From_ValidValue_ReturnsInstance()
    {
        var now = new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0));
        var abolishedOn = AbolishedOn.From(now);
        Assert.True(abolishedOn.IsAbolished);
        Assert.Equal(now, abolishedOn.Value);
    }

    [Fact]
    public void TryFrom_Null_ReturnsUnset()
    {
        var result = AbolishedOn.TryFrom(null, out var abolishedOn);
        Assert.True(result);
        Assert.False(abolishedOn.IsAbolished);
    }

    [Fact]
    public void TryFrom_ValidValue_ReturnsInstance()
    {
        var now = new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0));
        var result = AbolishedOn.TryFrom(now, out var abolishedOn);
        Assert.True(result);
        Assert.Equal(now, abolishedOn.Value);
    }

    [Fact]
    public void TryFromDbValue_Null_ReturnsUnset()
    {
        var result = AbolishedOn.TryFromDbValue(null, out var abolishedOn);
        Assert.True(result);
        Assert.False(abolishedOn.IsAbolished);
    }

    [Fact]
    public void TryFromDbValue_ValidValue_ReturnsInstance()
    {
        var now = new DateTime(2026, 9, 15, 10, 0, 0);
        var result = AbolishedOn.TryFromDbValue(now, out var abolishedOn);
        Assert.True(result);
        Assert.True(abolishedOn.IsAbolished);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var abolishedOn1 = AbolishedOn.Unset();
        var abolishedOn2 = AbolishedOn.Unset();
        Assert.Equal(abolishedOn1, abolishedOn2);
    }

    [Fact]
    public void ToString_Unset_ReturnsNotAbolished()
    {
        var abolishedOn = AbolishedOn.Unset();
        Assert.Equal("Not Abolished", abolishedOn.ToString());
    }
}
