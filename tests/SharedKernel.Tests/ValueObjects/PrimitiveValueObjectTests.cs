using SupportAdvance.Tests.SharedKernel.Tests.ValueObjects.Fixtures;

namespace SupportAdvance.Tests.SharedKernel.Tests.ValueObjects;

/// <summary>
/// PrimitiveValueObject<TValue> の単体テスト
/// 仕様書：docs/SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject単体テスト仕様書.md
/// </summary>
public class PrimitiveValueObjectTests
{
    #region コンストラクタテスト（CT グループ）

    /// <summary>
    /// PVO-CT-01: isSet=true で値設定用コンストラクタが正常に動作
    /// </summary>
    [Fact]
    public void VO_CT_01_Constructor_WithValueAndIsSetTrue_SetsValueAndIsSet()
    {
        // Arrange & Act
        var value = TestStringValue.Create("TestValue", isSet: true);

        // Assert
        Assert.True(value.IsSet);
        Assert.Equal("TestValue", value.Value);
    }

    /// <summary>
    /// PVO-CT-02: 未設定用コンストラクタが IsSet=false で初期化
    /// </summary>
    [Fact]
    public void VO_CT_02_Constructor_WithIsSetFalse_InitializesUnset()
    {
        // Arrange & Act
        var value = TestStringValue.CreateUnset();

        // Assert
        Assert.False(value.IsSet);
        Assert.Null(value.Value);
    }

    /// <summary>
    /// PVO-CT-03: Normalize が Validate より先に呼び出される
    /// </summary>
    [Fact]
    public void VO_CT_03_NormalizeThenValidate_NormalizeCallsFirst()
    {
        // Arrange & Act
        var value = TestStringValue.Create("  TrimTest  ", isSet: true);

        // Assert - Normalize でトリムされてから Validate で検証される
        Assert.Equal("TrimTest", value.Value);
        Assert.True(value.IsSet);
    }

    /// <summary>
    /// PVO-CT-04: Normalize で入力値が変換される
    /// </summary>
    [Fact]
    public void VO_CT_04_Normalize_TransformsInputValue()
    {
        // Arrange & Act
        var value = TestStringValue.Create("  PaddedValue  ", isSet: true);

        // Assert
        Assert.Equal("PaddedValue", value.Value);
    }

    /// <summary>
    /// PVO-CT-05: isSet=false の場合、Normalize/Validate は呼び出されない
    /// </summary>
    [Fact]
    public void VO_CT_05_WhenIsSetFalse_NormalizeAndValidateNotCalled()
    {
        // Arrange & Act
        var value = TestStringValue.CreateUnset();

        // Assert
        Assert.False(value.IsSet);
        Assert.Null(value.Value);
    }

    /// <summary>
    /// PVO-CT-06: Validate が例外をスロー場合、インスタンス構築が失敗
    /// </summary>
    [Fact]
    public void VO_CT_06_Validate_ThrowsException_ConstructionFails()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => TestStringValue.Create("", isSet: true));
        Assert.Throws<ArgumentException>(() => TestStringValue.Create(new string('a', 51), isSet: true));
    }

    #endregion

    #region TryGetValue テスト（TG グループ）

    /// <summary>
    /// PVO-TG-01: isSet=true の場合、TryGetValue は true
    /// </summary>
    [Fact]
    public void VO_TG_01_TryGetValue_WhenIsSetTrue_ReturnsTrue()
    {
        // Arrange
        var value = TestStringValue.Create("TestValue", isSet: true);

        // Act
        var result = value.TryGetValue(out var retrievedValue);

        // Assert
        Assert.True(result);
        Assert.Equal("TestValue", retrievedValue);
    }

    /// <summary>
    /// PVO-TG-02: isSet=false の場合、TryGetValue は false
    /// </summary>
    [Fact]
    public void VO_TG_02_TryGetValue_WhenIsSetFalse_ReturnsFalse()
    {
        // Arrange
        var value = TestStringValue.CreateUnset();

        // Act
        var result = value.TryGetValue(out var retrievedValue);

        // Assert
        Assert.False(result);
        Assert.Null(retrievedValue);
    }

    #endregion

    #region Normalize テスト（NM グループ）

    /// <summary>
    /// PVO-NM-01: オーバーライドしない場合、入力値そのまま
    /// </summary>
    [Fact]
    public void VO_NM_01_Normalize_NotOverridden_ReturnsInputAsIs()
    {
        // Arrange & Act
        var value = TestIntValue.Create(42, isSet: true);

        // Assert
        Assert.Equal(42, value.Value);
    }

    /// <summary>
    /// PVO-NM-02: カスタム処理実装した場合、変換結果が保持される
    /// </summary>
    [Fact]
    public void VO_NM_02_Normalize_CustomImplemented_TransformationApplied()
    {
        // Arrange & Act
        var value = TestDecimalValue.Create(3.14159m, isSet: true);

        // Assert - Math.Round(value, 2) が適用されている
        Assert.Equal(3.14m, value.Value);
    }

    #endregion

    #region Validate テスト（VL グループ）

    /// <summary>
    /// PVO-VL-01: オーバーライドしない場合、常に成功
    /// </summary>
    [Fact]
    public void VO_VL_01_Validate_NotOverridden_AlwaysSucceeds()
    {
        // Arrange & Act & Assert
        var value1 = TestIntValue.Create(0, isSet: true);
        var value2 = TestIntValue.Create(50, isSet: true);
        var value3 = TestIntValue.Create(100, isSet: true);

        Assert.True(value1.IsSet);
        Assert.True(value2.IsSet);
        Assert.True(value3.IsSet);
    }

    /// <summary>
    /// PVO-VL-02: 例外をスロー場合、インスタンス構築が失敗
    /// </summary>
    [Fact]
    public void VO_VL_02_Validate_ThrowsException_ConstructionFails()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => TestIntValue.Create(-1, isSet: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => TestIntValue.Create(101, isSet: true));
        Assert.Throws<ArgumentException>(() => TestDecimalValue.Create(-1.0m, isSet: true));
    }

    #endregion

    #region Format テスト（FM グループ）

    /// <summary>
    /// PVO-FM-01: オーバーライドしない場合、value.ToString()
    /// </summary>
    [Fact]
    public void VO_FM_01_Format_NotOverridden_UsesToString()
    {
        // Arrange & Act
        var value = TestIntValue.Create(42, isSet: true);
        var toString = value.ToString();

        // Assert
        Assert.Equal("42", toString);
    }

    /// <summary>
    /// PVO-FM-02: カスタムフォーマット実装した場合、フォーマット結果を返す
    /// </summary>
    [Fact]
    public void VO_FM_02_Format_CustomImplemented_FormattingApplied()
    {
        // Arrange & Act
        var stringValue = TestStringValue.Create("Test", isSet: true);
        var decimalValue = TestDecimalValue.Create(100.50m, isSet: true);

        // Assert
        Assert.Equal("[Test]", stringValue.ToString());
        Assert.Equal("¥100.50", decimalValue.ToString());
    }

    #endregion

    #region GetEqualityComponents テスト（EC グループ）

    /// <summary>
    /// PVO-EC-01: isSet=true の場合、IsSet と値を返す
    /// </summary>
    [Fact]
    public void VO_EC_01_GetEqualityComponents_WhenIsSetTrue_IncludesValue()
    {
        // Arrange
        var value1 = TestStringValue.Create("Value", isSet: true);
        var value2 = TestStringValue.Create("Value", isSet: true);

        // Act & Assert
        Assert.Equal(value1, value2);
    }

    /// <summary>
    /// PVO-EC-02: isSet=false の場合、IsSet のみを返す
    /// </summary>
    [Fact]
    public void VO_EC_02_GetEqualityComponents_WhenIsSetFalse_ExcludesValue()
    {
        // Arrange
        var value1 = TestStringValue.CreateUnset();
        var value2 = TestStringValue.CreateUnset();

        // Act & Assert
        Assert.Equal(value1, value2);
    }

    /// <summary>
    /// PVO-EC-03: GetEqualityComponents の結果が正規化される
    /// </summary>
    [Fact]
    public void VO_EC_03_GetEqualityComponents_Normalized()
    {
        // Arrange
        var value1 = TestStringValue.Create("  Test  ", isSet: true);
        var value2 = TestStringValue.Create("Test", isSet: true);

        // Act & Assert - Normalize されたので等価
        Assert.Equal(value1, value2);
    }

    #endregion

    #region 等価性テスト（EQ グループ）

    /// <summary>
    /// PVO-EQ-01: 同じ型・同じ値・IsSet=true
    /// </summary>
    [Fact]
    public void VO_EQ_01_SameTypeValueIsSetTrue_AreEqual()
    {
        // Arrange
        var value1 = TestStringValue.Create("Same", isSet: true);
        var value2 = TestStringValue.Create("Same", isSet: true);

        // Act & Assert
        Assert.Equal(value1, value2);
        Assert.True(value1 == value2);
    }

    /// <summary>
    /// PVO-EQ-02: 同じ型・IsSet=false
    /// </summary>
    [Fact]
    public void VO_EQ_02_SameTypeIsSetFalse_AreEqual()
    {
        // Arrange
        var value1 = TestStringValue.CreateUnset();
        var value2 = TestStringValue.CreateUnset();

        // Act & Assert
        Assert.Equal(value1, value2);
    }

    /// <summary>
    /// PVO-EQ-03: 同じ型・IsSet=true・値が異なる
    /// </summary>
    [Fact]
    public void VO_EQ_03_SameTypeIsSetTrueDifferentValue_AreNotEqual()
    {
        // Arrange
        var value1 = TestStringValue.Create("Value1", isSet: true);
        var value2 = TestStringValue.Create("Value2", isSet: true);

        // Act & Assert
        Assert.NotEqual(value1, value2);
        Assert.False(value1 == value2);
    }

    /// <summary>
    /// PVO-EQ-04: IsSet が異なる
    /// </summary>
    [Fact]
    public void VO_EQ_04_DifferentIsSet_AreNotEqual()
    {
        // Arrange
        var valueSet = TestStringValue.Create("Value", isSet: true);
        var valueUnset = TestStringValue.CreateUnset();

        // Act & Assert
        Assert.NotEqual(valueSet, valueUnset);
    }

    /// <summary>
    /// PVO-EQ-05: Equals=true のオブジェクトは同一ハッシュ値
    /// </summary>
    [Fact]
    public void VO_EQ_05_EqualObjects_SameHashCode()
    {
        // Arrange
        var value1 = TestStringValue.Create("HashTest", isSet: true);
        var value2 = TestStringValue.Create("HashTest", isSet: true);

        // Act & Assert
        Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
    }

    /// <summary>
    /// PVO-EQ-06: Normalize で結果が同じになる場合、等価
    /// </summary>
    [Fact]
    public void VO_EQ_06_NormalizeProduceSameResult_AreEqual()
    {
        // Arrange
        var value1 = TestStringValue.Create("  Normalized  ", isSet: true);
        var value2 = TestStringValue.Create("Normalized", isSet: true);

        // Act & Assert
        Assert.Equal(value1, value2);
        Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
    }

    #endregion

    #region ToString テスト（TS グループ）

    /// <summary>
    /// PVO-TS-01: isSet=false の場合、"Unset" を返す
    /// </summary>
    [Fact]
    public void VO_TS_01_IsSetFalse_ReturnsUnset()
    {
        // Arrange
        var value = TestStringValue.CreateUnset();

        // Act & Assert
        Assert.Equal("Unset", value.ToString());
    }

    /// <summary>
    /// PVO-TS-02: Format をオーバーライドしない場合、value.ToString()
    /// </summary>
    [Fact]
    public void VO_TS_02_Format_NotOverridden_UsesToString()
    {
        // Arrange
        var value = TestIntValue.Create(99, isSet: true);

        // Act & Assert
        Assert.Equal("99", value.ToString());
    }

    /// <summary>
    /// PVO-TS-03: Format でカスタムフォーマット実装した場合
    /// </summary>
    [Fact]
    public void VO_TS_03_Format_CustomImplemented_AppliesFormatting()
    {
        // Arrange
        var stringValue = TestStringValue.Create("Formatted", isSet: true);
        var decimalValue = TestDecimalValue.Create(250.75m, isSet: true);

        // Act & Assert
        Assert.Equal("[Formatted]", stringValue.ToString());
        Assert.Equal("¥250.75", decimalValue.ToString());
    }

    #endregion

    #region 追加テスト：複数型での一貫性確認

    /// <summary>
    /// 複数のジェネリック型での一貫した動作を確認
    /// </summary>
    [Fact]
    public void MultipleTypes_ConsistentBehavior()
    {
        // Arrange
        var strVal = TestStringValue.Create("MultiType", isSet: true);
        var intVal = TestIntValue.Create(42, isSet: true);
        var decVal = TestDecimalValue.Create(99.99m, isSet: true);

        // Act & Assert - すべて IsSet=true で初期化されている
        Assert.True(strVal.IsSet);
        Assert.True(intVal.IsSet);
        Assert.True(decVal.IsSet);

        // 同じ値での等価性確認
        var strVal2 = TestStringValue.Create("MultiType", isSet: true);
        Assert.Equal(strVal, strVal2);
    }

    /// <summary>
    /// IsSet=false での一貫した動作
    /// </summary>
    [Fact]
    public void MultipleTypes_UnsetBehaviorConsistent()
    {
        // Arrange
        var strUnset = TestStringValue.CreateUnset();
        var intUnset = TestIntValue.CreateUnset();
        var decUnset = TestDecimalValue.CreateUnset();

        // Act & Assert
        Assert.False(strUnset.IsSet);
        Assert.False(intUnset.IsSet);
        Assert.False(decUnset.IsSet);

        Assert.Equal("Unset", strUnset.ToString());
        Assert.Equal("Unset", intUnset.ToString());
        Assert.Equal("Unset", decUnset.ToString());
    }

    #endregion
}
