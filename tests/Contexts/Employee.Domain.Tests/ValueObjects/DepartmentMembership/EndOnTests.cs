using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.DepartmentMembership;

/// <summary>
/// <see cref="EndOn"/> の単体テスト
/// </summary>
public class EndOnTests
{
    private static readonly LocalDateTime Date = new(new DateTime(2026, 12, 31, 0, 0, 0));

    [Fact]
    public void Unset_HasNotEnded()
    {
        var endOn = EndOn.Unset();

        Assert.False(endOn.HasEnded);
        Assert.Null(endOn.Value);
        Assert.Equal("無期限", endOn.ToString());
    }

    [Fact]
    public void From_ValidValue_HasEnded()
    {
        var endOn = EndOn.From(Date);

        Assert.True(endOn.HasEnded);
        Assert.Equal(Date, endOn.Value);
        Assert.Equal(Date.ToString(), endOn.ToString());
    }

    [Fact]
    public void Equals_SameValueEqualsAndUnsetDiffers()
    {
        Assert.Equal(EndOn.From(Date), EndOn.From(Date));
        Assert.Equal(EndOn.From(Date).GetHashCode(), EndOn.From(Date).GetHashCode());
        Assert.Equal(EndOn.Unset(), EndOn.Unset());
        Assert.Equal(0, EndOn.Unset().GetHashCode());
        Assert.NotEqual(EndOn.From(Date), EndOn.Unset());
        Assert.False(EndOn.From(Date).Equals(null));
    }
}
