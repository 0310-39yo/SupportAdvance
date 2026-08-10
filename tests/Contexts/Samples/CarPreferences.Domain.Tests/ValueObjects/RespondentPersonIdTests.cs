using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.Tests.ValueObjects;

/// <summary>
/// RespondentPersonId の単体テスト
/// テスト仕様書: RespondentPersonId_単体テスト仕様書 v1.0
/// </summary>
public sealed class RespondentPersonIdTests
{
    #region IsSet プロパティテスト

    /// <summary>
    /// VO-IS-01: IsSet=true で構築したオブジェクトは IsSet が true を返す
    /// </summary>
    [Fact]
    public void VO_IS_01_IsSet_CreatedWithFrom_ReturnsTrue()
    {
        // Act
        var result = RespondentPersonId.From(1500);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSet);
    }

    /// <summary>
    /// VO-IS-02: IsSet=false で構築したオブジェクトは IsSet が false を返す
    /// </summary>
    [Fact]
    public void VO_IS_02_IsSet_CreatedWithUnset_ReturnsFalse()
    {
        // Act
        var result = RespondentPersonId.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSet);
    }

    #endregion

    #region Optional値操作テスト

    /// <summary>
    /// VO-OPT-01: Unset() は IsSet=false のインスタンスを返す
    /// </summary>
    [Fact]
    public void VO_OPT_01_Unset_ReturnsUnsetInstance()
    {
        // Act
        var result = RespondentPersonId.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal("Unset", result.ToString());
    }

    /// <summary>
    /// VO-OPT-02: From(value) は IsSet=true のインスタンスを返す
    /// </summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(1500)]
    [InlineData(5000)]
    [InlineData(9999)]
    public void VO_OPT_02_From_WithValidValue_ReturnsSetInstance(int value)
    {
        // Act
        var result = RespondentPersonId.From(value);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out int outValue));
        Assert.Equal(value, outValue);
    }

    /// <summary>
    /// VO-OPT-03: TryFrom(null) は true を返し、Unset インスタンスを返す
    /// </summary>
    [Fact]
    public void VO_OPT_03_TryFromNullable_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        bool success = RespondentPersonId.TryFrom((int?)null, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
    }

    /// <summary>
    /// VO-OPT-04: TryFrom(valid) は true を返し、設定済みインスタンスを返す
    /// </summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(9999)]
    public void VO_OPT_04_TryFromNullable_WithValidValue_ReturnsTrueAndSetInstance(int value)
    {
        // Act
        bool success = RespondentPersonId.TryFrom((int?)value, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out int outValue));
        Assert.Equal(value, outValue);
    }

    /// <summary>
    /// VO-OPT-05: TryFrom(invalid) は false を返し、Unset インスタンスを返す
    /// </summary>
    [Theory]
    [InlineData(999)]
    [InlineData(500)]
    [InlineData(0)]
    [InlineData(10000)]
    [InlineData(999999)]
    public void VO_OPT_05_TryFromNullable_WithInvalidValue_ReturnsFalseAndUnset(int value)
    {
        // Act
        bool success = RespondentPersonId.TryFrom((int?)value, out var result);

        // Assert
        Assert.False(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
    }

    /// <summary>
    /// VO-OPT-06: TryGetValue(out value) は IsSet=true で true を返す
    /// </summary>
    [Fact]
    public void VO_OPT_06_TryGetValue_WhenIsSetTrue_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);

        // Act
        bool success = instance.TryGetValue(out int value);

        // Assert
        Assert.True(success);
        Assert.Equal(1500, value);
    }

    /// <summary>
    /// VO-OPT-07: TryGetValue(out value) は IsSet=false で false を返す
    /// </summary>
    [Fact]
    public void VO_OPT_07_TryGetValue_WhenIsSetFalse_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentPersonId.Unset();

        // Act
        bool success = instance.TryGetValue(out int value);

        // Assert
        Assert.False(success);
        Assert.Equal(0, value);
    }

    #endregion

    #region GetValueComponents テスト

    /// <summary>VO-GVC-01: IsSet=true の場合、GetEqualityComponents に IsSet と ValueField が含まれる</summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(1500)]
    [InlineData(9999)]
    public void VO_GVC_01_GetEqualityComponents_WithIsSetTrue_ContainsValueField(int value)
    {
        // Arrange
        var instance = RespondentPersonId.From(value);

        // Act
        var components = instance.GetEqualityComponents().ToList();

        // Assert
        Assert.NotEmpty(components);
        Assert.True(components[0] is bool && (bool)components[0] == true); // IsSet が先頭
        Assert.Contains(value, components); // ValueField が含まれる
    }

    /// <summary>VO-GVC-02: IsSet=false の場合、GetEqualityComponents に IsSet=false のみが含まれる</summary>
    [Fact]
    public void VO_GVC_02_GetEqualityComponents_WithIsSetFalse_ContainsOnlyIsSet()
    {
        // Arrange
        var unset = RespondentPersonId.Unset();

        // Act
        var components = unset.GetEqualityComponents().ToList();

        // Assert
        Assert.Single(components);
        Assert.True(components[0] is bool && (bool)components[0] == false); // IsSet=false のみ
    }

    /// <summary>VO-GVC-03: GetEqualityComponents で IsSet フラグが先頭に付加されることを確認</summary>
    [Fact]
    public void VO_GVC_03_GetEqualityComponents_StartsWithIsSet()
    {
        // Arrange
        var id1 = RespondentPersonId.From(1500);
        var id2 = RespondentPersonId.From(1500);

        // Act & Assert - GetEqualityComponents は protected なので、Equals で確認
        Assert.Equal(id1, id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());

        // IsSet が異なると非等価を確認
        var unset = RespondentPersonId.Unset();
        Assert.NotEqual(id1, unset);
    }

    #endregion

    #region 等価性テスト（Equals = true）

    /// <summary>
    /// VO-EQ-01: 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価
    /// </summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(1500)]
    [InlineData(9999)]
    public void VO_EQ_01_Equals_WithSameValue_ReturnsTrue(int value)
    {
        // Arrange
        var instance1 = RespondentPersonId.From(value);
        var instance2 = RespondentPersonId.From(value);

        // Act & Assert
        Assert.Equal(instance1, instance2);
        Assert.True(instance1.Equals(instance2));
        Assert.True(instance1 == instance2);
    }

    /// <summary>
    /// VO-EQ-02: 同一参照のオブジェクトは等価
    /// </summary>
    [Fact]
    public void VO_EQ_02_Equals_WithSelfReference_ReturnsTrue()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);

        // Act & Assert
        Assert.Equal(instance, instance);
        Assert.True(instance.Equals(instance));
        Assert.True(instance == instance);
    }

    /// <summary>
    /// VO-EQ-03: 複数コンポーネント（IsSet + ValueField）がすべて一致する場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_03_Equals_WithAllComponentsIdentical_ReturnsTrue()
    {
        // Arrange - 複数コンポーネント（IsSet=true, 同じ値）
        var instance1 = RespondentPersonId.From(2500);
        var instance2 = RespondentPersonId.From(2500);

        // Act & Assert
        Assert.True(instance1.IsSet);
        Assert.True(instance2.IsSet);
        Assert.Equal(instance1, instance2);
    }

    /// <summary>
    /// VO-EQ-04: 両方が IsSet=false かつ値が同じ場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_04_Equals_BothUnset_ReturnsTrue()
    {
        // Arrange
        var instance1 = RespondentPersonId.Unset();
        var instance2 = RespondentPersonId.Unset();

        // Act & Assert
        Assert.False(instance1.IsSet);
        Assert.False(instance2.IsSet);
        Assert.Equal(instance1, instance2);
        Assert.True(instance1 == instance2);
    }

    #endregion

    #region 非等価性テスト（Equals = false）

    /// <summary>
    /// VO-NE-01: コンポーネント値が異なる場合は非等価
    /// </summary>
    [Theory]
    [InlineData(1000, 2000)]
    [InlineData(1500, 9999)]
    [InlineData(5000, 4000)]
    public void VO_NE_01_Equals_WithDifferentValue_ReturnsFalse(int value1, int value2)
    {
        // Arrange
        var instance1 = RespondentPersonId.From(value1);
        var instance2 = RespondentPersonId.From(value2);

        // Act & Assert
        Assert.NotEqual(instance1, instance2);
        Assert.False(instance1.Equals(instance2));
        Assert.False(instance1 == instance2);
        Assert.True(instance1 != instance2);
    }

    /// <summary>
    /// VO-NE-02: IsSet が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_02_Equals_WithDifferentIsSet_ReturnsFalse()
    {
        // Arrange
        var setInstance = RespondentPersonId.From(1500);
        var unsetInstance = RespondentPersonId.Unset();

        // Act & Assert
        Assert.NotEqual(setInstance, unsetInstance);
        Assert.False(setInstance.Equals(unsetInstance));
        Assert.False(setInstance == unsetInstance);
    }

    /// <summary>
    /// VO-NE-03: 型が異なる場合は非等価（値が同じでも）
    /// </summary>
    [Fact]
    public void VO_NE_03_Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);
        object differentType = 1500;  // int 型

        // Act & Assert
        Assert.False(instance.Equals(differentType));
        Assert.NotEqual(instance, (object?)differentType);
    }

    /// <summary>
    /// VO-NE-04: null との比較は非等価
    /// </summary>
    [Fact]
    public void VO_NE_04_Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);

        // Act & Assert
        Assert.False(instance.Equals(null));
        Assert.False(instance == null);
        Assert.True(instance != null);
    }

    /// <summary>
    /// VO-NE-05: 複数コンポーネントの一部が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_05_Equals_WithPartialComponentDifference_ReturnsFalse()
    {
        // Arrange - IsSet は同じだが値が異なる
        var instance1 = RespondentPersonId.From(1500);
        var instance2 = RespondentPersonId.From(2500);

        // Act & Assert
        Assert.True(instance1.IsSet);
        Assert.True(instance2.IsSet);
        Assert.NotEqual(instance1, instance2);
    }

    #endregion

    #region ハッシュコードテスト

    /// <summary>
    /// VO-HC-01: Equals=true の 2 つのオブジェクトは同一ハッシュ値
    /// </summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(1500)]
    [InlineData(9999)]
    public void VO_HC_01_GetHashCode_WhenEquals_ReturnsSameHash(int value)
    {
        // Arrange
        var instance1 = RespondentPersonId.From(value);
        var instance2 = RespondentPersonId.From(value);

        // Act & Assert
        Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
    }

    /// <summary>
    /// VO-HC-02: IsSet が異なるとハッシュ値が異なる
    /// </summary>
    [Fact]
    public void VO_HC_02_GetHashCode_WithDifferentIsSet_ReturnsDifferentHash()
    {
        // Arrange
        var setInstance = RespondentPersonId.From(1500);
        var unsetInstance = RespondentPersonId.Unset();

        // Act & Assert
        // IsSet が異なるためハッシュ値が異なることを期待
        Assert.NotEqual(setInstance.GetHashCode(), unsetInstance.GetHashCode());
    }

    /// <summary>
    /// VO-HC-03: コンポーネント値が異なるとハッシュ値が異なる
    /// </summary>
    [Fact]
    public void VO_HC_03_GetHashCode_WithDifferentValue_ReturnsDifferentHash()
    {
        // Arrange
        var instance1 = RespondentPersonId.From(1000);
        var instance2 = RespondentPersonId.From(2000);

        // Act & Assert
        // 通常、異なる値はハッシュ値が異なることが多い
        Assert.NotEqual(instance1.GetHashCode(), instance2.GetHashCode());
    }

    /// <summary>
    /// VO-HC-04: ハッシュ値は複数呼び出しで一貫している
    /// </summary>
    [Fact]
    public void VO_HC_04_GetHashCode_MultipleInvocations_ReturnConsistentHash()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);

        // Act
        int hash1 = instance.GetHashCode();
        int hash2 = instance.GetHashCode();
        int hash3 = instance.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    #endregion

    #region 演算子テスト

    /// <summary>
    /// VO-OP-01: 等価なオブジェクトに == を適用すると true
    /// </summary>
    [Fact]
    public void VO_OP_01_EqualityOperator_WithEqualObjects_ReturnsTrue()
    {
        // Arrange
        var instance1 = RespondentPersonId.From(1500);
        var instance2 = RespondentPersonId.From(1500);

        // Act & Assert
        Assert.True(instance1 == instance2);
        Assert.False(instance1 != instance2);
    }

    /// <summary>
    /// VO-OP-02: 非等価なオブジェクトに == を適用すると false
    /// </summary>
    [Fact]
    public void VO_OP_02_EqualityOperator_WithNonEqualObjects_ReturnsFalse()
    {
        // Arrange
        var instance1 = RespondentPersonId.From(1500);
        var instance2 = RespondentPersonId.From(2500);

        // Act & Assert
        Assert.False(instance1 == instance2);
        Assert.True(instance1 != instance2);
    }

    /// <summary>
    /// VO-OP-03: 両辺が null のとき == は true
    /// </summary>
    [Fact]
    public void VO_OP_03_EqualityOperator_BothNull_ReturnsTrue()
    {
        // Arrange
        RespondentPersonId? instance1 = null;
        RespondentPersonId? instance2 = null;

        // Act & Assert
        Assert.True(instance1 == instance2);
        Assert.False(instance1 != instance2);
    }

    /// <summary>
    /// VO-OP-04: 片方のみ null のとき == は false
    /// </summary>
    [Fact]
    public void VO_OP_04_EqualityOperator_OneNull_ReturnsFalse()
    {
        // Arrange
        var instance = RespondentPersonId.From(1500);
        RespondentPersonId? nullInstance = null;

        // Act & Assert
        Assert.False(instance == nullInstance);
        Assert.True(instance != nullInstance);
    }

    /// <summary>
    /// VO-OP-05: != は == の否定と一致
    /// </summary>
    [Theory]
    [InlineData(1000, 1000)]  // 等価
    [InlineData(1000, 2000)]  // 非等価
    public void VO_OP_05_InequalityOperator_IsNegationOfEquality(int value1, int value2)
    {
        // Arrange
        var instance1 = RespondentPersonId.From(value1);
        var instance2 = RespondentPersonId.From(value2);

        // Act
        bool isEqual = instance1 == instance2;
        bool isNotEqual = instance1 != instance2;

        // Assert - != は == の否定
        Assert.Equal(!isEqual, isNotEqual);
    }

    #endregion

    #region ToString テスト

    /// <summary>
    /// VO-TS-01: IsSet=false のとき "Unset" を返す
    /// </summary>
    [Fact]
    public void VO_TS_01_ToString_WhenUnset_ReturnsUnsetString()
    {
        // Arrange
        var instance = RespondentPersonId.Unset();

        // Act
        string result = instance.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>
    /// VO-TS-02: IsSet=true のとき、値を文字列で返す
    /// </summary>
    [Theory]
    [InlineData(1000)]
    [InlineData(1500)]
    [InlineData(9999)]
    public void VO_TS_02_ToString_WhenSet_ReturnsValueAsString(int value)
    {
        // Arrange
        var instance = RespondentPersonId.From(value);

        // Act
        string result = instance.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }

    /// <summary>
    /// VO-TS-03: ToString 出力に IsSet の値そのものが含まれない
    /// </summary>
    [Fact]
    public void VO_TS_03_ToString_ContainsNoIsSetFlag()
    {
        // Arrange
        var setInstance = RespondentPersonId.From(1500);
        var unsetInstance = RespondentPersonId.Unset();

        // Act
        string setResult = setInstance.ToString();
        string unsetResult = unsetInstance.ToString();

        // Assert - "IsSet" という文字列が含まれない
        Assert.DoesNotContain("IsSet", setResult);
        Assert.DoesNotContain("IsSet", unsetResult);
    }

    #endregion

    #region バリデーション例外テスト

    /// <summary>
    /// From() で無効な値（範囲外）に対して ArgumentOutOfRangeException を投げる
    /// </summary>
    [Theory]
    [InlineData(999)]      // 下限未満
    [InlineData(0)]        // 下限未満
    [InlineData(-100)]     // 下限未満
    [InlineData(10000)]    // 上限超過
    [InlineData(99999)]    // 上限超過
    public void VO_From_WithInvalidValue_ThrowsArgumentOutOfRangeException(int invalidValue)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => RespondentPersonId.From(invalidValue));
    }

    #endregion

    #region 境界値テスト

    /// <summary>
    /// 有効な値の下限値（1000）でテスト
    /// </summary>
    [Fact]
    public void VO_BoundaryValue_MinValidValue_1000_Works()
    {
        // Act
        var instance = RespondentPersonId.From(1000);

        // Assert
        Assert.True(instance.IsSet);
        Assert.True(instance.TryGetValue(out int value));
        Assert.Equal(1000, value);
    }

    /// <summary>
    /// 有効な値の上限値（9999）でテスト
    /// </summary>
    [Fact]
    public void VO_BoundaryValue_MaxValidValue_9999_Works()
    {
        // Act
        var instance = RespondentPersonId.From(9999);

        // Assert
        Assert.True(instance.IsSet);
        Assert.True(instance.TryGetValue(out int value));
        Assert.Equal(9999, value);
    }

    #endregion
}
