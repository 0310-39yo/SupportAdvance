namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// <see cref="DepartmentName"/> の単体テスト
/// </summary>
public class DepartmentNameTests
{
    #region From

    [Theory]
    [InlineData("営業部")]
    [InlineData("Sales")]
    [InlineData("A")]
    public void From_WithValidName_ReturnsInstance(string value)
    {
        var result = DepartmentName.From(value);

        Assert.Equal(value, result.Value);
        Assert.True(result.IsSet);
    }

    [Fact]
    public void From_WithMaxLength_ReturnsInstance()
    {
        var value = new string('あ', DepartmentName.MaxLength);

        var result = DepartmentName.From(value);

        Assert.Equal(DepartmentName.MaxLength, result.Value.Length);
    }

    [Fact]
    public void From_WithOverMaxLength_ThrowsArgumentException()
    {
        var value = new string('あ', DepartmentName.MaxLength + 1);

        Assert.Throws<ArgumentException>(() => DepartmentName.From(value));
    }

    [Fact]
    public void From_WithEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentName.From(string.Empty));
    }

    [Fact]
    public void From_WithNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentName.From(null!));
    }

    #endregion

    #region TryFrom

    [Fact]
    public void TryFrom_WithValidName_ReturnsTrue()
    {
        var success = DepartmentName.TryFrom("企画部", out var result);

        Assert.True(success);
        Assert.Equal("企画部", result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryFrom_WithNullOrEmpty_ReturnsFalse(string? value)
    {
        var success = DepartmentName.TryFrom(value, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFrom_WithOverMaxLength_ReturnsFalse()
    {
        var success = DepartmentName.TryFrom(new string('a', DepartmentName.MaxLength + 1), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    #endregion

    #region Equals / GetHashCode / ToString

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        Assert.Equal(DepartmentName.From("営業部"), DepartmentName.From("営業部"));
        Assert.Equal(DepartmentName.From("営業部").GetHashCode(), DepartmentName.From("営業部").GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        Assert.NotEqual(DepartmentName.From("営業部"), DepartmentName.From("企画部"));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(DepartmentName.From("営業部").Equals((DepartmentName?)null));
        Assert.False(DepartmentName.From("営業部").Equals((object?)null));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        Assert.Equal("営業部", DepartmentName.From("営業部").ToString());
    }

    #endregion
}
