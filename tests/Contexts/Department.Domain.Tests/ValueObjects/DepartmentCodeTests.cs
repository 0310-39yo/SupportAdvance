namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;

public class DepartmentCodeTests
{
    [Fact]
    public void From_ValidCode_ReturnsInstance()
    {
        var code = DepartmentCode.From("ABCD");
        Assert.Equal("ABCD", code.Value);
    }

    [Fact]
    public void From_InvalidLength_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("ABC"));
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("ABCDE"));
    }

    [Fact]
    public void From_NonAlphanumeric_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("AB-D"));
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("AB CD"));
    }

    [Fact]
    public void TryFrom_ValidCode_ReturnsTrue()
    {
        var result = DepartmentCode.TryFrom("TEST", out var code);
        Assert.True(result);
        Assert.Equal("TEST", code.Value);
    }

    [Fact]
    public void TryFrom_InvalidCode_ReturnsFalse()
    {
        var result = DepartmentCode.TryFrom("INVALID", out _);
        Assert.False(result);
    }

    [Fact]
    public void TryFrom_Null_ReturnsFalse()
    {
        var result = DepartmentCode.TryFrom(null, out _);
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var code1 = DepartmentCode.From("ABCD");
        var code2 = DepartmentCode.From("ABCD");
        Assert.Equal(code1, code2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var code1 = DepartmentCode.From("ABCD");
        var code2 = DepartmentCode.From("EFGH");
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var code = DepartmentCode.From("ABCD");
        Assert.Equal("ABCD", code.ToString());
    }
}
