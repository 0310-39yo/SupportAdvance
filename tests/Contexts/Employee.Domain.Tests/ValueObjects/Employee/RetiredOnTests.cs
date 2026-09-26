using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

/// <summary>
/// <see cref="RetiredOn"/> の単体テスト
/// </summary>
public class RetiredOnTests
{
    private static readonly LocalDateTime Date = new(new DateTime(2026, 3, 31, 0, 0, 0));

    [Fact]
    public void Unset_HasNotRetired()
    {
        var retiredOn = RetiredOn.Unset();

        Assert.False(retiredOn.HasRetired);
        Assert.False(retiredOn.IsSet);
        Assert.Equal("現職", retiredOn.ToString());
    }

    [Fact]
    public void From_ValidValue_HasRetired()
    {
        var retiredOn = RetiredOn.From(Date);

        Assert.True(retiredOn.HasRetired);
        Assert.Equal(Date, retiredOn.Value);
        Assert.Equal(Date.ToString(), retiredOn.ToString());
    }

    [Fact]
    public void Equals_SameValueEqualsAndUnsetDiffers()
    {
        Assert.Equal(RetiredOn.From(Date), RetiredOn.From(Date));
        Assert.Equal(RetiredOn.From(Date).GetHashCode(), RetiredOn.From(Date).GetHashCode());
        Assert.Equal(RetiredOn.Unset(), RetiredOn.Unset());
        Assert.NotEqual(RetiredOn.From(Date), RetiredOn.Unset());
        Assert.False(RetiredOn.From(Date).Equals(null));
    }
}
