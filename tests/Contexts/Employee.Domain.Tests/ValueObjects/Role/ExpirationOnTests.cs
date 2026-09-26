using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Role;

/// <summary>
/// <see cref="ExpirationOn"/> の単体テスト
/// </summary>
public class ExpirationOnTests
{
    private static readonly LocalDateTime Date = new(new DateTime(2027, 3, 31, 0, 0, 0));

    [Fact]
    public void Unlimited_HasNoExpiration()
    {
        var expirationOn = ExpirationOn.Unlimited;

        Assert.False(expirationOn.HasExpiration);
        Assert.False(expirationOn.IsSet);
        Assert.Equal("無期限", expirationOn.ToString());
    }

    [Fact]
    public void From_ValidValue_HasExpiration()
    {
        var expirationOn = ExpirationOn.From(Date);

        Assert.True(expirationOn.HasExpiration);
        Assert.Equal(Date, expirationOn.Value);
        Assert.Equal(Date.ToString(), expirationOn.ToString());
    }

    [Fact]
    public void Equals_SameValueEqualsAndUnlimitedDiffers()
    {
        Assert.Equal(ExpirationOn.From(Date), ExpirationOn.From(Date));
        Assert.Equal(ExpirationOn.From(Date).GetHashCode(), ExpirationOn.From(Date).GetHashCode());
        Assert.Equal(ExpirationOn.Unlimited, ExpirationOn.Unlimited);
        Assert.NotEqual(ExpirationOn.From(Date), ExpirationOn.Unlimited);
        Assert.False(ExpirationOn.From(Date).Equals(null));
    }
}
