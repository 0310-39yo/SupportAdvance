namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

public class RoleCodeTests
{
    [Theory]
    [InlineData("ADMIN")]
    [InlineData("MANAGER")]
    [InlineData("VIEWER")]
    public void From_WithValidCode_ReturnsInstance(string validCode)
    {
        var roleCode = RoleCode.From(validCode);
        Assert.True(roleCode.IsSet);
        Assert.Equal(validCode, roleCode.Value);
    }

    [Theory]
    [InlineData("user_viewer")]
    [InlineData("admin_role")]
    public void From_WithUnderscore_ReturnsInstance(string validCode)
    {
        var roleCode = RoleCode.From(validCode);
        Assert.True(roleCode.IsSet);
        Assert.Equal(validCode, roleCode.Value);
    }

    [Theory]
    [InlineData("admin.role")]
    [InlineData("user.viewer")]
    public void From_WithDot_ReturnsInstance(string validCode)
    {
        var roleCode = RoleCode.From(validCode);
        Assert.True(roleCode.IsSet);
        Assert.Equal(validCode, roleCode.Value);
    }

    [Fact]
    public void From_WithNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RoleCode.From(null!));
    }

    [Fact]
    public void From_WithEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RoleCode.From(""));
    }

    [Fact]
    public void From_With51Characters_ThrowsArgumentException()
    {
        var invalidCode = new string('A', 51);
        Assert.Throws<ArgumentException>(() => RoleCode.From(invalidCode));
    }

    [Theory]
    [InlineData("A-D")]
    [InlineData("A D")]
    [InlineData("A@D")]
    public void From_WithInvalidCharacters_ThrowsArgumentException(string invalidCode)
    {
        Assert.Throws<ArgumentException>(() => RoleCode.From(invalidCode));
    }

    [Fact]
    public void TryFrom_WithValidCode_ReturnsTrue()
    {
        var result = RoleCode.TryFrom("ADMIN", out var roleCode);
        Assert.True(result);
        Assert.True(roleCode.IsSet);
        Assert.Equal("ADMIN", roleCode.Value);
    }

    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        var result = RoleCode.TryFrom(null, out var roleCode);
        Assert.False(result);
    }

    [Fact]
    public void TryFrom_WithEmpty_ReturnsFalse()
    {
        var result = RoleCode.TryFrom("", out var roleCode);
        Assert.False(result);
    }

    [Fact]
    public void TryFromDbValue_WithValidCode_ReturnsTrue()
    {
        var result = RoleCode.TryFromDbValue("ADMIN", out var roleCode);
        Assert.True(result);
    }

    [Fact]
    public void TryFromDbValue_WithNull_ReturnsFalse()
    {
        var result = RoleCode.TryFromDbValue(null, out var roleCode);
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var code1 = RoleCode.From("ADMIN");
        var code2 = RoleCode.From("ADMIN");
        Assert.Equal(code1, code2);
        Assert.True(code1 == code2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var code1 = RoleCode.From("ADMIN");
        var code2 = RoleCode.From("VIEWER");
        Assert.NotEqual(code1, code2);
        Assert.False(code1 == code2);
    }

    [Fact]
    public void Equals_CaseSensitive_ReturnsFalse()
    {
        var code1 = RoleCode.From("ADMIN");
        var code2 = RoleCode.From("admin");
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var code1 = RoleCode.From("ADMIN");
        var code2 = RoleCode.From("ADMIN");
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsStringValue()
    {
        var code = RoleCode.From("ADMIN");
        Assert.Equal("ADMIN", code.ToString());
    }
}



