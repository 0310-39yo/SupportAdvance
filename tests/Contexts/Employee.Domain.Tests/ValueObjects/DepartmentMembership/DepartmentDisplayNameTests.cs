using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.DepartmentMembership;

/// <summary>
/// <see cref="DepartmentDisplayName"/> の単体テスト
/// </summary>
public class DepartmentDisplayNameTests
{
    #region Unset / From

    [Fact]
    public void Unset_ReturnsInstanceWithoutName()
    {
        var result = DepartmentDisplayName.Unset();

        Assert.NotNull(result);
        Assert.False(result.HasName);
        Assert.False(result.IsSet);
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void From_WithValidName_ReturnsSetInstance()
    {
        var result = DepartmentDisplayName.From("営業部");

        Assert.True(result.HasName);
        Assert.Equal("営業部", result.Value);
    }

    [Fact]
    public void From_WithEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentDisplayName.From(string.Empty));
    }

    [Fact]
    public void From_WithNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentDisplayName.From(null!));
    }

    #endregion

    #region TryFrom

    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        var success = DepartmentDisplayName.TryFrom(null, out var result);

        Assert.True(success);
        Assert.False(result.HasName);
    }

    [Fact]
    public void TryFrom_WithValidName_ReturnsTrueAndSet()
    {
        var success = DepartmentDisplayName.TryFrom("企画部", out var result);

        Assert.True(success);
        Assert.True(result.HasName);
        Assert.Equal("企画部", result.Value);
    }

    [Fact]
    public void TryFrom_WithEmpty_ReturnsFalse()
    {
        var success = DepartmentDisplayName.TryFrom(string.Empty, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    #endregion

    #region Equals / GetHashCode / ToString

    [Fact]
    public void Equals_SameName_ReturnsTrue()
    {
        Assert.Equal(DepartmentDisplayName.From("営業部"), DepartmentDisplayName.From("営業部"));
        Assert.Equal(
            DepartmentDisplayName.From("営業部").GetHashCode(),
            DepartmentDisplayName.From("営業部").GetHashCode());
    }

    [Fact]
    public void Equals_TwoUnset_ReturnsTrue()
    {
        Assert.Equal(DepartmentDisplayName.Unset(), DepartmentDisplayName.Unset());
    }

    [Fact]
    public void Equals_UnsetAndSet_ReturnsFalse()
    {
        Assert.NotEqual(DepartmentDisplayName.Unset(), DepartmentDisplayName.From("営業部"));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(DepartmentDisplayName.Unset().Equals((DepartmentDisplayName?)null));
        Assert.False(DepartmentDisplayName.Unset().Equals((object?)null));
    }

    [Fact]
    public void ToString_Unset_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, DepartmentDisplayName.Unset().ToString());
    }

    [Fact]
    public void ToString_Set_ReturnsName()
    {
        Assert.Equal("営業部", DepartmentDisplayName.From("営業部").ToString());
    }

    #endregion
}
