using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Permission;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

public class PermissionCodeTests
{
    [Theory]
    [InlineData("Employee.Create")]
    [InlineData("Employee.Read")]
    [InlineData("Employee.Delete")]
    public void From_WithValidCode_ReturnsInstance(string validCode)
    {
        var permCode = PermissionCode.From(validCode);
        Assert.True(permCode.IsSet);
        Assert.Equal(validCode, permCode.Value);
    }

    [Theory]
    [InlineData("Employee.Read.Own")]
    [InlineData("Resource.Action.Scope")]
    public void From_WithMultipleDots_ReturnsInstance(string validCode)
    {
        var permCode = PermissionCode.From(validCode);
        Assert.True(permCode.IsSet);
        Assert.Equal(validCode, permCode.Value);
    }

    [Fact]
    public void From_WithUnderscore_ReturnsInstance()
    {
        var permCode = PermissionCode.From("employee_read");
        Assert.True(permCode.IsSet);
        Assert.Equal("employee_read", permCode.Value);
    }

    [Fact]
    public void From_MaxLength_ReturnsInstance()
    {
        var validCode = new string('A', 100);
        var permCode = PermissionCode.From(validCode);
        Assert.True(permCode.IsSet);
        Assert.Equal(100, permCode.Value.Length);
    }

    [Fact]
    public void From_WithNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PermissionCode.From(null!));
    }

    [Fact]
    public void From_WithEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PermissionCode.From(""));
    }

    [Fact]
    public void From_With101Characters_ThrowsArgumentException()
    {
        var invalidCode = new string('A', 101);
        Assert.Throws<ArgumentException>(() => PermissionCode.From(invalidCode));
    }

    [Theory]
    [InlineData("Employee-Create")]
    [InlineData("Employee Create")]
    [InlineData("Employee@Create")]
    public void From_WithInvalidCharacters_ThrowsArgumentException(string invalidCode)
    {
        Assert.Throws<ArgumentException>(() => PermissionCode.From(invalidCode));
    }

    [Fact]
    public void TryFrom_WithValidCode_ReturnsTrue()
    {
        var result = PermissionCode.TryFrom("Employee.Create", out var permCode);
        Assert.True(result);
        Assert.True(permCode.IsSet);
        Assert.Equal("Employee.Create", permCode.Value);
    }

    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        var result = PermissionCode.TryFrom(null, out var permCode);
        Assert.False(result);
    }

    [Fact]
    public void TryFrom_WithEmpty_ReturnsFalse()
    {
        var result = PermissionCode.TryFrom("", out var permCode);
        Assert.False(result);
    }

    [Fact]
    public void TryFromDbValue_WithValidCode_ReturnsTrue()
    {
        var result = PermissionCode.TryFromDbValue("Employee.Create", out var permCode);
        Assert.True(result);
    }

    [Fact]
    public void TryFromDbValue_WithNull_ReturnsFalse()
    {
        var result = PermissionCode.TryFromDbValue(null, out var permCode);
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var code1 = PermissionCode.From("Employee.Create");
        var code2 = PermissionCode.From("Employee.Create");
        Assert.Equal(code1, code2);
        Assert.True(code1 == code2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var code1 = PermissionCode.From("Employee.Create");
        var code2 = PermissionCode.From("Employee.Read");
        Assert.NotEqual(code1, code2);
        Assert.False(code1 == code2);
    }

    [Fact]
    public void Equals_CaseSensitive_ReturnsFalse()
    {
        var code1 = PermissionCode.From("Employee.Create");
        var code2 = PermissionCode.From("employee.create");
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var code1 = PermissionCode.From("Employee.Create");
        var code2 = PermissionCode.From("Employee.Create");
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsStringValue()
    {
        var code = PermissionCode.From("Employee.Create");
        Assert.Equal("Employee.Create", code.ToString());
    }
}



