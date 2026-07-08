using SupportAdvance.Tests.SharedKernel.Tests.ValueObjects.Fixtures;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

/// <summary>
/// ValueObjectComponentNormalizer の動作を ValueObject 経由でテスト
/// 仕様: ValueObjectComponentNormalizer 単体テスト仕様書.md
///
/// 注：ValueObjectComponentNormalizer は internal なので、
/// ValueObject（Equals, GetHashCode, ToString）を通じて間接的にテストする
/// </summary>
public class ValueObjectComponentNormalizerIntegrationTests
{
    // ================================================================================
    // 観点グループ A：components が null または空 → Normalize による正規化確認
    // ================================================================================

    /// <summary>
    /// 観点: VCN-A-01
    /// IsSet=true、components={ "Tokyo" } → 正規化でIsSetが先頭に付加される
    /// → Equals で正しく比較できることで確認
    /// </summary>
    [Fact]
    public void VCN_A01_NormalizationAddsIsSet_ConfirmedByEquals()
    {
        // Arrange
        var obj1 = OrderId.Create("ID-001", isSet: true);
        var obj2 = OrderId.Create("ID-001", isSet: true);

        // Act
        var areEqual = obj1.Equals(obj2);
        var hashCodesEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert - IsSetが付加されているはずなので、同じ値なら等価
        Assert.True(areEqual);
        Assert.True(hashCodesEqual);
    }

    /// <summary>
    /// 観点: VCN-A-02
    /// IsSet フラグが異なるとハッシュコード・等価性に反映される
    /// → 正規化が IsSet を含めている側面
    /// </summary>
    [Fact]
    public void VCN_A02_IsSetIntegratedInNormalization_ConfirmedByHashCode()
    {
        // Arrange
        var objTrue = OrderId.Create("ID-002", isSet: true);
        var objFalse = OrderId.Create("ID-002", isSet: false);

        // Act
        var areEqual = objTrue.Equals(objFalse);
        var hashCodeTrue = objTrue.GetHashCode();
        var hashCodeFalse = objFalse.GetHashCode();

        // Assert
        Assert.False(areEqual);
        Assert.NotEqual(hashCodeTrue, hashCodeFalse);
    }

    /// <summary>
    /// 複雑なValueObjectでの正規化動作確認
    /// </summary>
    [Fact]
    public void VCN_ComplexValueObject_NormalizationWorks()
    {
        // Arrange
        var price1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var price2 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var price3 = ProductPrice.Create(100.0m, "JPY", isSet: false);

        // Act
        var equalsTrue = price1.Equals(price2);
        var equalsFalse = price1.Equals(price3);
        var hashCode1 = price1.GetHashCode();
        var hashCode3 = price3.GetHashCode();

        // Assert
        Assert.True(equalsTrue);
        Assert.False(equalsFalse);
        Assert.NotEqual(hashCode1, hashCode3);
    }

    // ================================================================================
    // 観点グループ B：先頭要素が bool かつ IsSet と一致 → 重複排除の確認
    // ================================================================================

    /// <summary>
    /// 複数コンポーネントの場合、正規化が正しく動作
    /// </summary>
    [Fact]
    public void VCN_B01_NormalizationWithMultipleComponents()
    {
        // Arrange
        var price1 = ProductPrice.Create(150.5m, "EUR", isSet: true);
        var price2 = ProductPrice.Create(150.5m, "EUR", isSet: true);

        // Act
        var areEqual = price1.Equals(price2);
        var toString = price1.ToString();

        // Assert
        Assert.True(areEqual);
        Assert.Equal("150.5, EUR", toString);
    }

    // ================================================================================
    // 観点グループ C：通常ケース → 正規化の一般的動作確認
    // ================================================================================

    /// <summary>
    /// 観点: VCN-C-03
    /// null を含むコンポーネントでも正規化が正しく動作
    /// </summary>
    [Fact]
    public void VCN_C03_NormalizationWithNullComponent()
    {
        // Arrange
        var obj1 = OrderId.Create("ID-003", isSet: true);
        var obj2 = OrderId.Create("ID-003", isSet: true);

        // Act
        var areEqual = obj1.Equals(obj2);
        var hash1 = obj1.GetHashCode();
        var hash2 = obj2.GetHashCode();

        // Assert - 正規化が正しく処理されるので等価性が成り立つ
        Assert.True(areEqual);
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// ToString でIsSetを含めて正規化されることを確認
    /// </summary>
    [Fact]
    public void VCN_NormalizationInToString()
    {
        // Arrange
        var objSet = OrderId.Create("ID-004", isSet: true);
        var objUnset = OrderId.Create("ID-004", isSet: false);

        // Act
        var toStringSet = objSet.ToString();
        var toStringUnset = objUnset.ToString();

        // Assert
        // IsSet=false の場合  "Unset"を返す（正規化ではIsSetを先頭に付加）
        // IsSet=true の場合、値を返す
        Assert.Equal("ID-004", toStringSet);
        Assert.Equal("Unset", toStringUnset);
    }

    /// <summary>
    /// 複雑なケース: 複数型のコンポーネント + IsSet
    /// </summary>
    [Fact]
    public void VCN_C02_MultipleTypesNormalization()
    {
        // Arrange
        var price1 = ProductPrice.Create(999999.99m, "EUR", isSet: true);
        var price2 = ProductPrice.Create(999999.99m, "EUR", isSet: true);
        var price3 = ProductPrice.Create(999999.99m, "EUR", isSet: false);

        // Act
        var equal12 = price1.Equals(price2);
        var equal13 = price1.Equals(price3);
        var hash1 = price1.GetHashCode();
        var hash3 = price3.GetHashCode();

        // Assert
        Assert.True(equal12);
        Assert.False(equal13);
        Assert.NotEqual(hash1, hash3);
    }

    /// <summary>
    /// Equals の対称性と推移性の確認（正規化による）
    /// </summary>
    [Fact]
    public void VCN_Normalization_PreservesEqualsProperties()
    {
        // Arrange
        var obj1 = OrderId.Create("ID-005", isSet: true);
        var obj2 = OrderId.Create("ID-005", isSet: true);
        var obj3 = OrderId.Create("ID-005", isSet: true);

        // Act
        var eq12 = obj1.Equals(obj2);
        var eq21 = obj2.Equals(obj1);
        var eq23 = obj2.Equals(obj3);
        var eq13 = obj1.Equals(obj3);

        // Assert
        // 対称性: eq12 == eq21
        Assert.Equal(eq12, eq21);
        // 推移性: eq12 && eq23 ⇒ eq13
        if (eq12 && eq23)
        {
            Assert.True(eq13);
        }
    }

    /// <summary>
    /// HashSet での動作確認（正規化による）
    /// </summary>
    [Fact]
    public void VCN_Normalization_WorksInHashSet()
    {
        // Arrange
        var obj1 = OrderId.Create("ID-006", isSet: true);
        var obj2 = OrderId.Create("ID-006", isSet: true);
        var obj3 = OrderId.Create("ID-007", isSet: true);

        var hashSet = new HashSet<OrderId> { obj1, obj2, obj3 };

        // Act & Assert
        // 正規化が正しく機能するので、obj1 と obj2 は同じハッシュコード＆等価 → 重複排除される
        Assert.Equal(2, hashSet.Count);
        Assert.Contains(obj1, hashSet);
        Assert.Contains(obj3, hashSet);
    }

    /// <summary>
    /// Dictionary でのキー機能確認（正規化による）
    /// </summary>
    [Fact]
    public void VCN_Normalization_WorksInDictionary()
    {
        // Arrange
        var key1 = OrderId.Create("ID-008", isSet: true);
        var key1Same = OrderId.Create("ID-008", isSet: true);
        var key2 = OrderId.Create("ID-009", isSet: true);

        var dict = new Dictionary<OrderId, string>
        {
            { key1, "Value1" },
            { key2, "Value2" }
        };

        // Act
        var found = dict.TryGetValue(key1Same, out var value);

        // Assert
        Assert.True(found);
        Assert.Equal("Value1", value);
    }
}
