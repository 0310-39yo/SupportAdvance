using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Domain.Tests.ValueObjects;

public class LoginIdTests
{
    [Fact]
    public void From_WithValidValue_ReturnsInstance()
    {
        var loginId = LoginId.From("user123");
        Assert.NotNull(loginId);
        Assert.Equal("user123", loginId.Value);
    }

    [Fact]
    public void From_WithWhitespaceOnly_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => LoginId.From("   "));
        Assert.Contains("cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void From_WithEmpty_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => LoginId.From(""));
        Assert.Contains("cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void From_WithExceeding50Characters_ThrowsArgumentException()
    {
        var longString = new string('a', 51);
        var exception = Assert.Throws<ArgumentException>(() => LoginId.From(longString));
        Assert.Contains("must be 50 characters or less", exception.Message);
    }

    [Fact]
    public void From_With50Characters_ReturnsInstance()
    {
        var fiftyChars = new string('a', 50);
        var loginId = LoginId.From(fiftyChars);
        Assert.Equal(fiftyChars, loginId.Value);
    }

    [Fact]
    public void From_WithLeadingAndTrailingWhitespace_TrimsValue()
    {
        var loginId = LoginId.From("  employee001  ");
        Assert.Equal("employee001", loginId.Value);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrue()
    {
        var result = LoginId.TryFrom("john_doe", out var loginId);
        Assert.True(result);
        Assert.Equal("john_doe", loginId.Value);
    }

    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        var result = LoginId.TryFrom(null, out var loginId);
        Assert.False(result);
        Assert.Null(loginId);
    }

    [Fact]
    public void TryFrom_WithEmpty_ReturnsFalse()
    {
        var result = LoginId.TryFrom("", out var loginId);
        Assert.False(result);
        Assert.Null(loginId);
    }

    [Fact]
    public void TryFrom_WithWhitespaceOnly_ReturnsFalse()
    {
        var result = LoginId.TryFrom("   ", out var loginId);
        Assert.False(result);
        Assert.Null(loginId);
    }

    [Fact]
    public void TryFrom_WithExceeding50Characters_ReturnsFalse()
    {
        var longString = new string('x', 51);
        var result = LoginId.TryFrom(longString, out var loginId);
        Assert.False(result);
        Assert.Null(loginId);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var loginId1 = LoginId.From("admin");
        var loginId2 = LoginId.From("admin");
        Assert.Equal(loginId1, loginId2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var loginId1 = LoginId.From("admin");
        var loginId2 = LoginId.From("user");
        Assert.NotEqual(loginId1, loginId2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        var loginId1 = LoginId.From("test");
        var loginId2 = LoginId.From("test");
        Assert.Equal(loginId1.GetHashCode(), loginId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var loginId = LoginId.From("mylogin");
        Assert.Equal("mylogin", loginId.ToString());
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        var loginId1 = LoginId.From("test123");
        var loginId2 = LoginId.From("test123");
        Assert.True(loginId1 == loginId2);
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        var loginId1 = LoginId.From("test123");
        var loginId2 = LoginId.From("test124");
        Assert.True(loginId1 != loginId2);
    }

    [Fact]
    public void EqualityOperator_WithNull_ReturnsFalse()
    {
        var loginId = LoginId.From("test");
        Assert.False(loginId == null);
    }

    [Fact]
    public void InequalityOperator_WithNull_ReturnsTrue()
    {
        var loginId = LoginId.From("test");
        Assert.True(loginId != null);
    }
}
