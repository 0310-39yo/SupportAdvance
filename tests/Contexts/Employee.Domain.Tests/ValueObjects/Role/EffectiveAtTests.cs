using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Role;

/// <summary>
/// <see cref="EffectiveAt"/> の単体テスト
/// </summary>
public class EffectiveAtTests
{
    private static readonly LocalDateTime Date = new(new DateTime(2026, 4, 1, 0, 0, 0));

    [Fact]
    public void From_ValidValue_KeepsValueAndToString()
    {
        var effectiveAt = EffectiveAt.From(Date);

        Assert.Equal(Date, effectiveAt.Value);
        Assert.Equal(Date.ToString(), effectiveAt.ToString());
    }

    [Fact]
    public void Equals_SameValueEqualsAndDifferentValueDiffers()
    {
        Assert.Equal(EffectiveAt.From(Date), EffectiveAt.From(Date));
        Assert.Equal(EffectiveAt.From(Date).GetHashCode(), EffectiveAt.From(Date).GetHashCode());
        Assert.NotEqual(EffectiveAt.From(Date), EffectiveAt.From(Date + TimeSpan.FromDays(1)));
        Assert.False(EffectiveAt.From(Date).Equals(null));
    }
}
