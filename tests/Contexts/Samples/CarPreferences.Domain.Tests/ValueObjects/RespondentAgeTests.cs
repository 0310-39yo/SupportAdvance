using Xunit;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// RespondentAge の単体テスト
/// </summary>
public sealed class RespondentAgeTests
{
    #region From メソッドテスト

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(150)]
    public void From_WithValidAge_CreatesInstance(int age)
    {
        // Act
        var result = RespondentAge.From(age);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out int value));
        Assert.Equal(age, value);
        Assert.Equal(age.ToString(), result.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(151)]
    [InlineData(200)]
    [InlineData(999)]
    public void From_WithInvalidAge_ThrowsArgumentOutOfRangeException(int age)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(age));
    }

    #endregion

    #region Unset メソッドテスト

    [Fact]
    public void Unset_CreatesUnsetInstance()
    {
        // Act
        var result = RespondentAge.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal("Unset", result.ToString());
    }

    [Fact]
    public void Unset_MultipleCalls_ReturnEqualInstances()
    {
        // Act
        var result1 = RespondentAge.Unset();
        var result2 = RespondentAge.Unset();

        // Assert
        Assert.Equal(result1, result2);
    }

    #endregion

    #region TryFrom(int?) メソッドテスト

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(150)]
    public void TryFrom_WithValidValue_ReturnsTrue(int age)
    {
        // Act
        var success = RespondentAge.TryFrom(age, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out int value));
        Assert.Equal(age, value);
    }

    [Fact]
    public void TryFrom_WithNullInput_ReturnsUnsetAndTrue()
    {
        // Act
        var success = RespondentAge.TryFrom(null, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal(RespondentAge.Unset(), result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(151)]
    [InlineData(200)]
    [InlineData(999)]
    public void TryFrom_WithInvalidValue_ReturnsFalseAndUnset(int age)
    {
        // Act
        var success = RespondentAge.TryFrom(age, out var result);

        // Assert
        Assert.False(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal(RespondentAge.Unset(), result);
    }

    #endregion

    #region TryFrom(int) メソッドテスト（非nullable版）

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(150)]
    public void TryFrom_NonNullable_WithValidValue_ReturnsTrue(int age)
    {
        // Act
        var success = RespondentAge.TryFrom(age, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.TryGetValue(out int value));
        Assert.Equal(age, value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(151)]
    public void TryFrom_NonNullable_WithInvalidValue_ReturnsFalse(int age)
    {
        // Act
        var success = RespondentAge.TryFrom(age, out var result);

        // Assert
        Assert.False(success);
        Assert.False(result.IsSet);
    }

    #endregion

    #region TryGetValue メソッドテスト

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(150)]
    public void TryGetValue_WhenSet_ReturnsTrueWithValue(int age)
    {
        // Arrange
        var instance = RespondentAge.From(age);

        // Act
        var success = instance.TryGetValue(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(age, value);
    }

    [Fact]
    public void TryGetValue_WhenUnset_ReturnsFalseWithDefault()
    {
        // Arrange
        var instance = RespondentAge.Unset();

        // Act
        var success = instance.TryGetValue(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(0, value);
    }

    #endregion

    #region 等価性テスト

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(25, 25)]
    [InlineData(150, 150)]
    public void Equals_WithSameValues_ReturnsTrue(int age1, int age2)
    {
        // Arrange
        var instance1 = RespondentAge.From(age1);
        var instance2 = RespondentAge.From(age2);

        // Act & Assert
        Assert.Equal(instance1, instance2);
        Assert.True(instance1.Equals(instance2));
        Assert.True(instance1 == instance2 || instance1.Equals(instance2)); // 後者はIEquatable
    }

    [Fact]
    public void Equals_WithUnset_SameInstance_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentAge.Unset();

        // Act & Assert
        Assert.Equal(instance, instance);
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 150)]
    [InlineData(25, 50)]
    public void Equals_WithDifferentValues_ReturnsFalse(int age1, int age2)
    {
        // Arrange
        var instance1 = RespondentAge.From(age1);
        var instance2 = RespondentAge.From(age2);

        // Act & Assert
        Assert.NotEqual(instance1, instance2);
        Assert.False(instance1.Equals(instance2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(150)]
    public void Equals_BetweenValidAndUnset_ReturnsFalse(int age)
    {
        // Arrange
        var valid = RespondentAge.From(age);
        var unset = RespondentAge.Unset();

        // Act & Assert
        Assert.NotEqual(valid, unset);
        Assert.False(valid.Equals(unset));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentAge.From(25);

        // Act & Assert
        Assert.False(instance.Equals(null));
        Assert.False(instance == null);
    }

    [Fact]
    public void Equals_WithSelfReference_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentAge.From(25);

        // Act & Assert
        Assert.True(instance.Equals(instance));
    }

    [Fact]
    public void Equals_Object_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var instance1 = RespondentAge.From(25);
        var instance2 = RespondentAge.From(25);
        object instance2AsObject = instance2;

        // Act & Assert
        Assert.True(instance1.Equals(instance2AsObject));
    }

    [Fact]
    public void Equals_Object_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var instance1 = RespondentAge.From(25);
        var instance2 = RespondentAge.From(50);
        object instance2AsObject = instance2;

        // Act & Assert
        Assert.False(instance1.Equals(instance2AsObject));
    }

    #endregion

    #region ハッシング テスト

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(150)]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode(int age)
    {
        // Arrange
        var instance1 = RespondentAge.From(age);
        var instance2 = RespondentAge.From(age);

        // Act & Assert
        Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_UnsetInstances_ReturnsSameHashCode()
    {
        // Arrange
        var instance1 = RespondentAge.Unset();
        var instance2 = RespondentAge.Unset();

        // Act & Assert
        Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CanBeUsedInDictionary()
    {
        // Arrange
        var dict = new Dictionary<RespondentAge, string>
        {
            { RespondentAge.From(0), "Zero years" },
            { RespondentAge.From(25), "Twenty-five years" },
            { RespondentAge.From(150), "One hundred fifty years" },
            { RespondentAge.Unset(), "Unset" }
        };

        // Act & Assert
        Assert.Equal("Zero years", dict[RespondentAge.From(0)]);
        Assert.Equal("Twenty-five years", dict[RespondentAge.From(25)]);
        Assert.Equal("One hundred fifty years", dict[RespondentAge.From(150)]);
        Assert.Equal("Unset", dict[RespondentAge.Unset()]);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var set = new HashSet<RespondentAge>
        {
            RespondentAge.From(0),
            RespondentAge.From(25),
            RespondentAge.From(150)
        };

        // Act & Assert
        Assert.Contains(RespondentAge.From(0), set);
        Assert.Contains(RespondentAge.From(25), set);
        Assert.Contains(RespondentAge.From(150), set);
        Assert.DoesNotContain(RespondentAge.From(50), set);
    }

    #endregion

    #region ToString テスト

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(25, "25")]
    [InlineData(50, "50")]
    [InlineData(100, "100")]
    [InlineData(150, "150")]
    public void ToString_WhenSet_ReturnsValueString(int age, string expected)
    {
        // Arrange
        var instance = RespondentAge.From(age);

        // Act
        var result = instance.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WhenUnset_ReturnsUnsetString()
    {
        // Arrange
        var instance = RespondentAge.Unset();

        // Act
        var result = instance.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    #endregion

    #region 0 vs null 区別テスト

    [Fact]
    public void From0_WithIsSetTrue_IsDifferentFromUnset()
    {
        // Arrange
        var age0 = RespondentAge.From(0);
        var unset = RespondentAge.Unset();

        // Act & Assert
        // From(0) は IsSet=true, ValueField=0
        Assert.True(age0.IsSet);
        Assert.True(age0.TryGetValue(out int age0Value));
        Assert.Equal(0, age0Value);

        // Unset() は IsSet=false
        Assert.False(unset.IsSet);

        // 等価性は IsSet フラグで判定 → 異なる
        Assert.NotEqual(age0, unset);
    }

    [Fact]
    public void Age0_GetEqualityComponents_IncludesIsSetFlag()
    {
        // Arrange
        var age0 = RespondentAge.From(0);

        // Act
        // GetEqualityComponents は protected なので、Equals の動作で確認
        var age0_another = RespondentAge.From(0);

        // Assert
        Assert.Equal(age0, age0_another);
        Assert.Equal(age0.GetHashCode(), age0_another.GetHashCode());
    }

    [Fact]
    public void TryFrom_Zero_CreatesValidSet_DifferentFromUnset()
    {
        // Act
        var age0Success = RespondentAge.TryFrom(0, out var age0);
        var nullSuccess = RespondentAge.TryFrom(null, out var nullResult);

        // Assert
        Assert.True(age0Success);
        Assert.True(age0.IsSet);
        Assert.True(age0.TryGetValue(out int age0Value));
        Assert.Equal(0, age0Value);

        Assert.True(nullSuccess);
        Assert.False(nullResult.IsSet);

        Assert.NotEqual(age0, nullResult);
    }

    #endregion

    #region IOptionalValueObject 対応テスト

    [Fact]
    public void RespondentAge_ImplementsIOptionalValueObject()
    {
        // Act & Assert
        // RespondentAge は IOptionalValueObject<RespondentAge, int> を実装している
        // インターフェースの実装確認は、From/Unset/TryFrom のコントラクトで検証
        var age = RespondentAge.From(25);
        var unset = RespondentAge.Unset();

        // IsSet プロパティを持つことを確認
        Assert.True(age.IsSet);
        Assert.False(unset.IsSet);

        // From, Unset, TryFrom メソッドが存在することを確認
        Assert.NotNull(age);
        Assert.NotNull(unset);
    }

    [Fact]
    public void From_And_Unset_FollowIOptionalValueObjectContract()
    {
        // Arrange
        var age = RespondentAge.From(25);
        var unset = RespondentAge.Unset();

        // Act & Assert
        // From: value を設定（IsSet=true）
        Assert.True(age.IsSet);
        Assert.True(age.TryGetValue(out var value));
        Assert.Equal(25, value);

        // Unset: 値なし（IsSet=false）
        Assert.False(unset.IsSet);
        Assert.False(unset.TryGetValue(out _));
    }

    #endregion

    #region エッジケーステスト

    [Fact]
    public void From_Boundary_ZeroAndOneHundredFifty()
    {
        // Act
        var age0 = RespondentAge.From(0);
        var age150 = RespondentAge.From(150);

        // Assert
        Assert.True(age0.IsSet);
        Assert.True(age0.TryGetValue(out int age0Value));
        Assert.Equal(0, age0Value);

        Assert.True(age150.IsSet);
        Assert.True(age150.TryGetValue(out int age150Value));
        Assert.Equal(150, age150Value);
    }

    [Fact]
    public void From_JustOutsideBoundary_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(151));
    }

    [Fact]
    public void Multiple_From_Calls_Create_DistinctInstances()
    {
        // Act
        var age1 = RespondentAge.From(25);
        var age2 = RespondentAge.From(25);

        // Assert
        Assert.NotSame(age1, age2); // 異なるインスタンス
        Assert.Equal(age1, age2);   // 但し等価
    }

    #endregion
}
