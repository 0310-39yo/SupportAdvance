using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Domain.Tests.ValueObjects;

/// <summary>
/// <see cref="UsedLoginCredentialsRowId"/> の単体テスト
/// </summary>
public class UsedLoginCredentialsRowIdTests
{
    #region Unset / From

    [Fact]
    public void Unset_ReturnsInstanceWithoutCredentials()
    {
        var result = UsedLoginCredentialsRowId.Unset();

        Assert.NotNull(result);
        Assert.False(result.HasCredentials);
        Assert.False(result.IsSet);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(5L)]
    [InlineData(long.MaxValue)]
    public void From_WithValidValue_ReturnsSetInstance(long value)
    {
        var result = UsedLoginCredentialsRowId.From(value);

        Assert.True(result.HasCredentials);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void From_WithInvalidValue_ThrowsArgumentOutOfRangeException(long value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UsedLoginCredentialsRowId.From(value));
    }

    [Fact]
    public void From_WithRequiredLoginCredentialsRowId_ReturnsSetInstanceWithSameValue()
    {
        var required = LoginCredentialsRowId.From(42L);

        var result = UsedLoginCredentialsRowId.From(required);

        Assert.True(result.HasCredentials);
        Assert.Equal(42L, result.Value);
    }

    [Fact]
    public void From_WithNullRequiredLoginCredentialsRowId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => UsedLoginCredentialsRowId.From((LoginCredentialsRowId)null!));
    }

    #endregion

    #region TryFrom

    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        var success = UsedLoginCredentialsRowId.TryFrom(null, out var result);

        Assert.True(success);
        Assert.False(result.HasCredentials);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndSet()
    {
        var success = UsedLoginCredentialsRowId.TryFrom(7L, out var result);

        Assert.True(success);
        Assert.True(result.HasCredentials);
        Assert.Equal(7L, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-5L)]
    public void TryFrom_WithInvalidValue_ReturnsFalse(long value)
    {
        var success = UsedLoginCredentialsRowId.TryFrom(value, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    #endregion

    #region Equals / GetHashCode

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        Assert.Equal(UsedLoginCredentialsRowId.From(3L), UsedLoginCredentialsRowId.From(3L));
        Assert.Equal(UsedLoginCredentialsRowId.From(3L).GetHashCode(), UsedLoginCredentialsRowId.From(3L).GetHashCode());
    }

    [Fact]
    public void Equals_TwoUnset_ReturnsTrue()
    {
        Assert.Equal(UsedLoginCredentialsRowId.Unset(), UsedLoginCredentialsRowId.Unset());
    }

    [Fact]
    public void Equals_UnsetAndSet_ReturnsFalse()
    {
        Assert.NotEqual(UsedLoginCredentialsRowId.Unset(), UsedLoginCredentialsRowId.From(3L));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        Assert.NotEqual(UsedLoginCredentialsRowId.From(3L), UsedLoginCredentialsRowId.From(4L));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(UsedLoginCredentialsRowId.Unset().Equals((UsedLoginCredentialsRowId?)null));
        Assert.False(UsedLoginCredentialsRowId.Unset().Equals((object?)null));
    }

    #endregion
}
