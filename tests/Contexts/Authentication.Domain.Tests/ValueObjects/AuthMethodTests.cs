using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Authentication.Domain.Tests.ValueObjects;

public class AuthMethodTests
{
    [Fact]
    public void LocalAuth_ReturnsLocalAuthInstance()
    {
        var method = AuthMethod.LocalAuth();
        Assert.NotNull(method);
        Assert.True(method.IsLocalAuth());
        Assert.False(method.IsWindowsAD());
    }

    [Fact]
    public void WindowsAD_ReturnsWindowsADInstance()
    {
        var method = AuthMethod.WindowsAD();
        Assert.NotNull(method);
        Assert.False(method.IsLocalAuth());
        Assert.True(method.IsWindowsAD());
    }

    [Fact]
    public void From_WithLocalAuthValue_ReturnsLocalAuthInstance()
    {
        var method = AuthMethod.From(AuthMethod.MethodType.LocalAuth);
        Assert.True(method.IsLocalAuth());
    }

    [Fact]
    public void From_WithWindowsADValue_ReturnsWindowsADInstance()
    {
        var method = AuthMethod.From(AuthMethod.MethodType.WindowsAD);
        Assert.True(method.IsWindowsAD());
    }

    [Fact]
    public void TryFrom_WithZero_ReturnsTrue()
    {
        var result = AuthMethod.TryFrom(0, out var method);
        Assert.True(result);
        Assert.True(method.IsLocalAuth());
    }

    [Fact]
    public void TryFrom_WithOne_ReturnsTrue()
    {
        var result = AuthMethod.TryFrom(1, out var method);
        Assert.True(result);
        Assert.True(method.IsWindowsAD());
    }

    [Fact]
    public void TryFrom_WithInvalidValue_ReturnsFalse()
    {
        var result = AuthMethod.TryFrom(99, out var method);
        Assert.False(result);
        Assert.Null(method);
    }

    [Fact]
    public void TryFrom_WithNegativeValue_ReturnsFalse()
    {
        var result = AuthMethod.TryFrom(-1, out var method);
        Assert.False(result);
        Assert.Null(method);
    }

    [Fact]
    public void IsLocalAuth_WithLocalAuthMethod_ReturnsTrue()
    {
        var method = AuthMethod.LocalAuth();
        Assert.True(method.IsLocalAuth());
    }

    [Fact]
    public void IsLocalAuth_WithWindowsADMethod_ReturnsFalse()
    {
        var method = AuthMethod.WindowsAD();
        Assert.False(method.IsLocalAuth());
    }

    [Fact]
    public void IsWindowsAD_WithWindowsADMethod_ReturnsTrue()
    {
        var method = AuthMethod.WindowsAD();
        Assert.True(method.IsWindowsAD());
    }

    [Fact]
    public void IsWindowsAD_WithLocalAuthMethod_ReturnsFalse()
    {
        var method = AuthMethod.LocalAuth();
        Assert.False(method.IsWindowsAD());
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var method1 = AuthMethod.LocalAuth();
        var method2 = AuthMethod.LocalAuth();
        Assert.Equal(method1, method2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var method1 = AuthMethod.LocalAuth();
        var method2 = AuthMethod.WindowsAD();
        Assert.NotEqual(method1, method2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var method = AuthMethod.LocalAuth();
        Assert.False(method.Equals(null));
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        var method1 = AuthMethod.WindowsAD();
        var method2 = AuthMethod.WindowsAD();
        Assert.Equal(method1.GetHashCode(), method2.GetHashCode());
    }

    [Fact]
    public void ToString_WithLocalAuth_ReturnsLocalAuthString()
    {
        var method = AuthMethod.LocalAuth();
        Assert.Equal("LocalAuth", method.ToString());
    }

    [Fact]
    public void ToString_WithWindowsAD_ReturnsWindowsADString()
    {
        var method = AuthMethod.WindowsAD();
        Assert.Equal("WindowsAD", method.ToString());
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        var method1 = AuthMethod.LocalAuth();
        var method2 = AuthMethod.LocalAuth();
        Assert.True(method1 == method2);
    }

    [Fact]
    public void EqualityOperator_WithDifferentValue_ReturnsFalse()
    {
        var method1 = AuthMethod.LocalAuth();
        var method2 = AuthMethod.WindowsAD();
        Assert.False(method1 == method2);
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        var method1 = AuthMethod.LocalAuth();
        var method2 = AuthMethod.WindowsAD();
        Assert.True(method1 != method2);
    }

    [Fact]
    public void InequalityOperator_WithSameValue_ReturnsFalse()
    {
        var method1 = AuthMethod.WindowsAD();
        var method2 = AuthMethod.WindowsAD();
        Assert.False(method1 != method2);
    }

    [Fact]
    public void EqualityOperator_WithNull_ReturnsFalse()
    {
        var method = AuthMethod.LocalAuth();
        Assert.False(method == null);
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        AuthMethod? method1 = null;
        AuthMethod? method2 = null;
        Assert.True(method1 == method2);
    }
}
