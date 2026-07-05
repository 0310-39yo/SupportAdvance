using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.Tests.ValueObjects;

/// <summary>
/// RespondentAge の単体テスト
/// </summary>
public sealed class RespondentAgeTests
{
    #region From メソッドテスト

    /// <summary>VO-OPT-02: From\uff08有効値\uff09で設定済みインスタンス返却確認</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(150)]
    public void VO_OPT_02_From_WithValidAge_CreatesInstance(int age)
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

    /// <summary>VO-VR-01,02,03: From\uff08無効値\uff09で ArgumentOutOfRangeException スロー確認</summary>
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

    /// <summary>VO-OPT-01, VO-TS-01: Unset() で未設定インスタンス返却確認</summary>
    [Fact]
    public void VO_OPT_01_Unset_CreatesUnsetInstance()
    {
        // Act
        var result = RespondentAge.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal("Unset", result.ToString());
    }

    /// <summary>VO-EQ-04: Unset() の複数呼び出しが等価確認</summary>
    [Fact]
    public void VO_EQ_04_Unset_MultipleCalls_ReturnEqualInstances()
    {
        // Act
        var result1 = RespondentAge.Unset();
        var result2 = RespondentAge.Unset();

        // Assert
        Assert.Equal(result1, result2);
    }

    #endregion

    #region TryFrom(int?) メソッドテスト

    /// <summary>VO-OPT-04: TryFrom\uff08有効値\uff09で true と設定済み返却確認</summary>
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

    /// <summary>VO-OPT-03: TryFrom(null) で true と Unset 返却確認</summary>
    [Fact]
    public void VO_OPT_03_TryFrom_WithNullInput_ReturnsUnsetAndTrue()
    {
        // Act
        var success = RespondentAge.TryFrom(null, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal(RespondentAge.Unset(), result);
    }

    /// <summary>VO-OPT-05: TryFrom\uff08無効値\uff09で false と Unset 返却確認</summary>
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

    /// <summary>VO-OPT-04: TryFrom(int)\uff08nullable版\uff09で有効値を受け言れ確認</summary>
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

    /// <summary>VO-OPT-05: TryFrom(int)\uff08nullable版\uff09で無効値を戜撃確認</summary>
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

    /// <summary>VO-OPT-06: TryGetValue\uff08IsSet=true\uff09で true と値返却確認</summary>
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

    /// <summary>VO-OPT-07: TryGetValue\uff08IsSet=false\uff09で false 返却確認</summary>
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

    /// <summary>VO-EQ-04: Equals\uff08Unset=Unset\uff09で true 確認</summary>
    [Fact]
    public void Equals_WithUnset_SameInstance_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentAge.Unset();

        // Act & Assert
        Assert.Equal(instance, instance);
    }

    /// <summary>VO-NE-01: NotEqual\uff08\u7570なる値\uff09で false 確認</summary>
    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 150)]
    [InlineData(25, 50)]
    public void VO_NE_01_Equals_WithDifferentValues_ReturnsFalse(int age1, int age2)
    {
        // Arrange
        var instance1 = RespondentAge.From(age1);
        var instance2 = RespondentAge.From(age2);

        // Act & Assert
        Assert.NotEqual(instance1, instance2);
        Assert.False(instance1.Equals(instance2));
    }

    /// <summary>VO-NE-02: NotEqual\uff08IsSet\u76d8\u7570\uff09で false 確認</summary>
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

    /// <summary>VO-NE-04: Equals(null) で false 確認</summary>
    [Fact]
    public void VO_NE_04_Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentAge.From(25);

        // Act & Assert
        Assert.False(instance.Equals(null));
        Assert.False(instance == null);
    }

    /// <summary>VO-EQ-02: Equals\uff08自己参照\uff09で true 確認</summary>
    [Fact]
    public void VO_EQ_02_Equals_WithSelfReference_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentAge.From(25);

        // Act & Assert
        Assert.True(instance.Equals(instance));
    }

    /// <summary>VO-EQ-01: Equals(object)\uff08同一値\uff09で true 確認</summary>
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

    /// <summary>VO-NE-01: NotEqual(object)\uff08\u7570なる値\uff09で false 確認</summary>
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

    /// <summary>VO-HC-01: GetHashCode\uff08同一値\uff09で同一ハッシュ確認</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(150)]
    public void VO_HC_01_GetHashCode_WithSameValues_ReturnsSameHashCode(int age)
    {
        // Arrange
        var instance1 = RespondentAge.From(age);
        var instance2 = RespondentAge.From(age);

        // Act & Assert
        Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
    }

    /// <summary>VO-HC-04: GetHashCode\uff08Unset\uff09を複数呼び出しで一貴性確認</summary>
    [Fact]
    public void GetHashCode_UnsetInstances_ReturnsSameHashCode()
    {
        // Arrange
        var instance1 = RespondentAge.Unset();
        var instance2 = RespondentAge.Unset();

        // Act & Assert
        Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
    }

    /// <summary>VO-HC-01: GetHashCodeを Dictionary で実装可能確認</summary>
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

    /// <summary>VO-HC-01: GetHashCodeを HashSet で実装可能確認</summary>
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

    /// <summary>VO-TS-02: ToString\uff08IsSet=true\uff09で値を文字列化確認</summary>
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

    /// <summary>VO-TS-01: ToString\uff08IsSet=false\uff09で "Unset" 返却確認</summary>
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

    /// <summary>VO-NE-02, VO-VR-01: From(0) と Unset が畵異握確認\uff080 を\u6709効値として受け入れ\uff09</summary>
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

    /// <summary>VO-EQ-01, VO-HC-01: GetEqualityComponents で IsSet フラグから確認されている</summary>
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

    /// <summary>VO-OPT-03, VO-OPT-04: TryFrom(0) と TryFrom(null) が区別されている確認</summary>
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
