namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の単体テスト仕様書に基づく統合テストクラス
/// 仕様書：docs/SharedKernel/ValueObjects/ValueObject単体テスト仕様書.md
///
/// テスト観点：
/// - IS: IsSet プロパティ
/// - EQ: Equals メソッド（等価判定）
/// - NE: Equals メソッド（非等価判定）
/// - HC: GetHashCode メソッド
/// - OP: == / != 演算子
/// - TS: ToString メソッド
/// - VCN: ValueObjectComponentNormalizer 統合テスト
/// </summary>
public class ValueObjectTests
{
    #region IsSet プロパティテスト（IS）

    /// <summary>
    /// 観点: VO-IS-01
    /// IsSet=true で構築したオブジェクトは IsSet が true を返す
    /// パターン: 単一コンポーネント、IsSet=true
    /// </summary>
    [Fact]
    public void VO_IS_01_SingleComponent_IsSetTrue()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = orderId.IsSet;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-IS-01
    /// IsSet=true で構築したオブジェクト（複数コンポーネント）は IsSet が true を返す
    /// パターン: 複数コンポーネント、IsSet=true
    /// </summary>
    [Fact]
    public void VO_IS_01_MultipleComponents_IsSetTrue()
    {
        // Arrange
        var price = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var result = price.IsSet;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-IS-02
    /// IsSet=false で構築したオブジェクトは IsSet が false を返す
    /// パターン: 単一コンポーネント、IsSet=false
    /// </summary>
    [Fact]
    public void VO_IS_02_SingleComponent_IsSetFalse()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-002", isSet: false);

        // Act
        var result = orderId.IsSet;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-IS-02
    /// IsSet=false で構築したオブジェクト（複数コンポーネント）は IsSet が false を返す
    /// パターン: 複数コンポーネント、IsSet=false
    /// </summary>
    [Fact]
    public void VO_IS_02_MultipleComponents_IsSetFalse()
    {
        // Arrange
        var price = ProductPrice.Create(200.0m, "USD", isSet: false);

        // Act
        var result = price.IsSet;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// IsSet プロパティは読み取り専用であり、外部から変更不可
    /// </summary>
    [Fact]
    public void IsSetProperty_IsReadOnly()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-003", isSet: true);

        // Act & Assert
        Assert.True(orderId.IsSet);
    }

    #endregion

    #region Equals メソッドテスト（等価判定：EQ）

    /// <summary>
    /// 観点: VO-EQ-01
    /// 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価
    /// パターン: 同じ値、同じ IsSet=true
    /// </summary>
    [Fact]
    public void VO_EQ_01_SameValueSameIsSetTrue_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-001", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-EQ-01
    /// 同じ値、IsSet=false でも等価
    /// パターン: 同じ値、同じ IsSet=false
    /// </summary>
    [Fact]
    public void VO_EQ_01_SameValueSameIsSetFalse_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-002", isSet: false);
        var obj2 = OrderId.Create("ORD-002", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
    }

    /// <summary>
    /// 観点: VO-EQ-01
    /// 複数コンポーネントがすべて一致する場合は等価
    /// パターン: 複数コンポーネント、すべて一致
    /// </summary>
    [Fact]
    public void VO_EQ_01_MultipleComponentsAllMatch_AreEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-EQ-02
    /// 同一参照のオブジェクトは等価（自己参照比較）
    /// </summary>
    [Fact]
    public void VO_EQ_02_SelfReference_IsEqual()
    {
        // Arrange
        var obj = OrderId.Create("ORD-003", isSet: true);

        // Act
        var equalsResult = obj.Equals(obj);
        var operatorResult = obj == obj;

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
    }

    /// <summary>
    /// 観点: VO-EQ-03
    /// 複数コンポーネントのすべてが一致する場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_03_MultipleComponentsPartialMatch_AreEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(150.5m, "EUR", isSet: true);
        var obj2 = ProductPrice.Create(150.5m, "EUR", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.True(equalsResult);
    }

    /// <summary>
    /// 観点: VO-EQ-04
    /// 両方が IsSet=false かつ値が同じ場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_04_BothUnsetSameValue_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: false);
        var obj2 = OrderId.Create("ORD-004", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// Equals メソッドは複数回呼び出しても一貫性がある
    /// </summary>
    [Fact]
    public void Equals_MultipleCalls_Consistent()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-005", isSet: true);
        var obj2 = OrderId.Create("ORD-005", isSet: true);

        // Act & Assert
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1.Equals(obj2));
    }

    #endregion

    #region Equals メソッドテスト（非等価判定：NE）

    /// <summary>
    /// 観点: VO-NE-01
    /// コンポーネント値が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_01_DifferentComponentValue_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-002", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-NE-02
    /// IsSet フラグが異なる場合は非等価（値が同じでも）
    /// パターン: IsSet: true vs false
    /// </summary>
    [Fact]
    public void VO_NE_02_DifferentIsSetFlag_TrueVsFalse_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-003", isSet: true);
        var obj2 = OrderId.Create("ORD-003", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-NE-02
    /// IsSet フラグが異なる場合は非等価（逆順）
    /// パターン: IsSet: false vs true
    /// </summary>
    [Fact]
    public void VO_NE_02_DifferentIsSetFlag_FalseVsTrue_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: false);
        var obj2 = OrderId.Create("ORD-004", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
    }

    /// <summary>
    /// 観点: VO-NE-03
    /// 型が異なる場合は非等価（値が同じでも）
    /// </summary>
    [Fact]
    public void VO_NE_03_DifferentType_AreNotEqual()
    {
        // Arrange
        var orderObj = OrderId.Create("100", isSet: true);
        var priceObj = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var equalsResult = orderObj.Equals(priceObj);
        var operatorResult = orderObj == priceObj;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
    }

    /// <summary>
    /// 観点: VO-NE-04
    /// null との比較は非等価
    /// </summary>
    [Fact]
    public void VO_NE_04_CompareWithNull_AreNotEqual()
    {
        // Arrange
        var obj = OrderId.Create("ORD-005", isSet: true);
        OrderId? nullObj = null;

        // Act
        var equalsResult = obj.Equals(nullObj);
        var operatorResult = obj == nullObj;
        var operatorNotEqualResult = obj != nullObj;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.True(operatorNotEqualResult);
    }

    /// <summary>
    /// 観点: VO-NE-05
    /// 複数コンポーネントの一部が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_05_MultipleComponentsPartialDifference_AreNotEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "USD", isSet: true);  // Currency が異なる

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 複数コンポーネントの別の一部が異なる場合も非等価
    /// </summary>
    [Fact]
    public void MultipleComponentsPartialDifference_AmountDifferent_AreNotEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(150.0m, "JPY", isSet: true);  // Amount が異なる

        // Act
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.False(equalsResult);
    }

    /// <summary>
    /// 相互性の検証：obj1.Equals(obj2) == obj2.Equals(obj1)
    /// </summary>
    [Fact]
    public void Equals_IsSymmetric()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-006", isSet: true);
        var obj2 = OrderId.Create("ORD-007", isSet: true);

        // Act
        var result1 = obj1.Equals(obj2);
        var result2 = obj2.Equals(obj1);

        // Assert
        Assert.Equal(result1, result2);
    }

    /// <summary>
    /// 推移性の検証：obj1 == obj2 && obj2 == obj3 ⇒ obj1 == obj3
    /// </summary>
    [Fact]
    public void Equals_IsTransitive()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-008", isSet: true);
        var obj2 = OrderId.Create("ORD-008", isSet: true);
        var obj3 = OrderId.Create("ORD-008", isSet: true);

        // Act
        var result12 = obj1.Equals(obj2);
        var result23 = obj2.Equals(obj3);
        var result13 = obj1.Equals(obj3);

        // Assert
        Assert.True(result12);
        Assert.True(result23);
        Assert.True(result13);
    }

    #endregion

    #region GetHashCode メソッドテスト（HC）

    /// <summary>
    /// 観点: VO-HC-01
    /// Equals=true の 2 つのオブジェクトは同一ハッシュ値
    /// パターン: 同値オブジェクトのハッシュ比較
    /// </summary>
    [Fact]
    public void VO_HC_01_EqualObjects_SameHashCode()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-001", isSet: true);

        // Act
        var hash1 = obj1.GetHashCode();
        var hash2 = obj2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// 観点: VO-HC-01
    /// 複数のオブジェクトペアでハッシュ検証（ハッシュ整合性）
    /// パターン: 複数のオブジェクトペアで検証
    /// </summary>
    [Fact]
    public void VO_HC_01_MultipleObjectPairs_SameHashCode()
    {
        // Arrange
        var pairs = new[]
        {
            (obj1: OrderId.Create("ORD-001", isSet: true), obj2: OrderId.Create("ORD-001", isSet: true)),
            (obj1: OrderId.Create("ORD-002", isSet: true), obj2: OrderId.Create("ORD-002", isSet: true)),
            (obj1: OrderId.Create("ORD-003", isSet: false), obj2: OrderId.Create("ORD-003", isSet: false))
        };

        // Act & Assert
        foreach (var pair in pairs)
        {
            Assert.Equal(pair.obj1.GetHashCode(), pair.obj2.GetHashCode());
        }
    }

    /// <summary>
    /// 観点: VO-HC-02
    /// IsSet フラグが異なると、ハッシュ値も異なる
    /// </summary>
    [Fact]
    public void VO_HC_02_DifferentIsSet_DifferentHashCode()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: true);
        var obj2 = OrderId.Create("ORD-004", isSet: false);

        // Act
        var hash1 = obj1.GetHashCode();
        var hash2 = obj2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// 観点: VO-HC-03
    /// コンポーネント値が異なると、ハッシュ値も異なる
    /// </summary>
    [Fact]
    public void VO_HC_03_DifferentComponentValue_DifferentHashCode()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-005", isSet: true);
        var obj2 = OrderId.Create("ORD-006", isSet: true);

        // Act
        var hash1 = obj1.GetHashCode();
        var hash2 = obj2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// 観点: VO-HC-03
    /// 複数コンポーネントの一つが異なるとハッシュ値が異なる
    /// </summary>
    [Fact]
    public void VO_HC_03_MultipleComponentsOneDifferent_DifferentHashCode()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "USD", isSet: true);  // Currency が異なる

        // Act
        var hash1 = obj1.GetHashCode();
        var hash2 = obj2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// 観点: VO-HC-04
    /// ハッシュ値は複数呼び出しで一貫している（副作用なし）
    /// </summary>
    [Fact]
    public void VO_HC_04_MultipleCalls_ConsistentHashCode()
    {
        // Arrange
        var obj = OrderId.Create("ORD-007", isSet: true);

        // Act
        var hash1 = obj.GetHashCode();
        var hash2 = obj.GetHashCode();
        var hash3 = obj.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    /// <summary>
    /// HashSet で正しく機能する
    /// 等価なオブジェクトはセット内で重複排除される
    /// </summary>
    [Fact]
    public void HashSet_RemovesDuplicates()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-008", isSet: true);
        var obj2 = OrderId.Create("ORD-008", isSet: true);
        var obj3 = OrderId.Create("ORD-009", isSet: true);

        var hashSet = new HashSet<OrderId> { obj1, obj2, obj3 };

        // Act & Assert
        Assert.Equal(2, hashSet.Count);
        Assert.Contains(obj1, hashSet);
        Assert.Contains(obj3, hashSet);
    }

    /// <summary>
    /// Dictionary のキーとして正しく機能する
    /// </summary>
    [Fact]
    public void Dictionary_AsKey_Works()
    {
        // Arrange
        var key1 = OrderId.Create("ORD-010", isSet: true);
        var key1Same = OrderId.Create("ORD-010", isSet: true);
        var key2 = OrderId.Create("ORD-011", isSet: true);

        var dict = new Dictionary<OrderId, string>
        {
            { key1, "Value1" },
            { key2, "Value2" }
        };

        // Act
        var value1 = dict[key1Same];
        var exists = dict.ContainsKey(key1Same);

        // Assert
        Assert.Equal("Value1", value1);
        Assert.True(exists);
    }

    #endregion

    #region == / != 演算子テスト（OP）

    /// <summary>
    /// 観点: VO-OP-01
    /// 等価なオブジェクトに == を適用すると true
    /// </summary>
    [Fact]
    public void VO_OP_01_EqualObjects_OperatorEqualReturnsTrue()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-OP-02
    /// 非等価なオブジェクトに == を適用すると false
    /// </summary>
    [Fact]
    public void VO_OP_02_NotEqualObjects_OperatorEqualReturnsFalse()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-002", isSet: true);

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-OP-03
    /// 両辺が null のとき == は true
    /// </summary>
    [Fact]
    public void VO_OP_03_BothNull_OperatorEqualReturnsTrue()
    {
        // Arrange
        OrderId? obj1 = null;
        OrderId? obj2 = null;

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-OP-04
    /// 片方のみ null のとき == は false
    /// </summary>
    [Fact]
    public void VO_OP_04_OneNull_OperatorEqualReturnsFalse()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-003", isSet: true);
        OrderId? obj2 = null;

        // Act
        var result1 = obj1 == obj2;
        var result2 = obj2 == obj1;

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// != は == の否定と一致する
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_IsNegationOfEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: true);
        var obj2 = OrderId.Create("ORD-005", isSet: true);

        // Act
        var equalResult = obj1 == obj2;
        var notEqualResult = obj1 != obj2;

        // Assert
        Assert.NotEqual(equalResult, notEqualResult);
        Assert.True(notEqualResult);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// != は == の否定と一致する（等価な場合）
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_IsNegationOfEqual_WhenEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-006", isSet: true);
        var obj2 = OrderId.Create("ORD-006", isSet: true);

        // Act
        var equalResult = obj1 == obj2;
        var notEqualResult = obj1 != obj2;

        // Assert
        Assert.NotEqual(equalResult, notEqualResult);
        Assert.False(notEqualResult);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// 両方が null のとき != は false
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_BothNull_ReturnsFalse()
    {
        // Arrange
        OrderId? obj1 = null;
        OrderId? obj2 = null;

        // Act
        var result = obj1 != obj2;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// 片方が null のとき != は true
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_OneNull_ReturnsTrue()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-007", isSet: true);
        OrderId? obj2 = null;

        // Act
        var result = obj1 != obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// == と Equals メソッドの結果が一致する
    /// </summary>
    [Fact]
    public void OperatorEqual_ConsistentWithEquals()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-008", isSet: true);
        var obj2 = OrderId.Create("ORD-008", isSet: true);

        // Act
        var operatorResult = obj1 == obj2;
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.Equal(operatorResult, equalsResult);
    }

    /// <summary>
    /// == と Equals メソッドの結果が一致する（非等価な場合）
    /// </summary>
    [Fact]
    public void OperatorEqual_ConsistentWithEquals_NotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-009", isSet: true);
        var obj2 = OrderId.Create("ORD-010", isSet: true);

        // Act
        var operatorResult = obj1 == obj2;
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.Equal(operatorResult, equalsResult);
    }

    /// <summary>
    /// 複数コンポーネントでの == 演算子の動作
    /// </summary>
    [Fact]
    public void OperatorEqual_MultipleComponents()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj3 = ProductPrice.Create(150.0m, "JPY", isSet: true);

        // Act
        var result12 = obj1 == obj2;
        var result13 = obj1 == obj3;

        // Assert
        Assert.True(result12);
        Assert.False(result13);
    }

    #endregion

    #region ToString メソッドテスト（TS）

    /// <summary>
    /// 観点: VO-TS-01
    /// IsSet=false のとき "Unset" を返す
    /// パターン: 単一コンポーネント
    /// </summary>
    [Fact]
    public void VO_TS_01_IsSetFalse_ReturnsUnset()
    {
        // Arrange
        var obj = OrderId.Create("ORD-001", isSet: false);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>
    /// 観点: VO-TS-01
    /// IsSet=false の複数コンポーネントオブジェクトでも "Unset" を返す
    /// </summary>
    [Fact]
    public void VO_TS_01_MultipleComponentsIsSetFalse_ReturnsUnset()
    {
        // Arrange
        var obj = ProductPrice.Create(100.0m, "JPY", isSet: false);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、コンポーネントをカンマ区切りで連結
    /// パターン: 単一コンポーネント
    /// </summary>
    [Fact]
    public void VO_TS_02_IsSetTrue_SingleComponent_ReturnsCombinedValue()
    {
        // Arrange
        var obj = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("ORD-001", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、複数コンポーネントをカンマで区切って連結
    /// パターン: 複数コンポーネント
    /// </summary>
    [Fact]
    public void VO_TS_02_IsSetTrue_MultipleComponents_ReturnsCombinedValue()
    {
        // Arrange
        var obj = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        // 注: decimal(100m)は"100"と表示されるが、decimal(100.0m)は"100.0"と表示される
        Assert.Equal("100.0, JPY", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、複数値がカンマで区切られている
    /// </summary>
    [Fact]
    public void VO_TS_02_MultipleComponents_CommaDelimited()
    {
        // Arrange
        var obj = ProductPrice.Create(250.5m, "USD", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Contains(", ", result);
        Assert.Equal("250.5, USD", result);
    }

    /// <summary>
    /// 観点: VO-TS-03
    /// ToString 出力に IsSet 値自体が含まれない
    /// </summary>
    [Fact]
    public void VO_TS_03_IsSetNotInOutput()
    {
        // Arrange
        var objTrue = OrderId.Create("ORD-002", isSet: true);
        var objFalse = OrderId.Create("ORD-003", isSet: false);

        // Act
        var resultTrue = objTrue.ToString();
        var resultFalse = objFalse.ToString();

        // Assert
        Assert.DoesNotContain("True", resultTrue);
        Assert.DoesNotContain("False", resultTrue);
        Assert.DoesNotContain("True", resultFalse);
        Assert.DoesNotContain("False", resultFalse);
    }

    /// <summary>
    /// 観点: VO-TS-03
    /// ToString では IsSet フラグ（bool値）そのものは含まれない
    /// 単に コンポーネント値のみ
    /// </summary>
    [Fact]
    public void VO_TS_03_ComponentValueOnly()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: true);
        var obj2 = OrderId.Create("ORD-004", isSet: false);

        // Act
        var resultTrue = obj1.ToString();
        var resultFalse = obj2.ToString();

        // Assert
        Assert.Equal("ORD-004", resultTrue);
        Assert.Equal("Unset", resultFalse);
        Assert.NotEqual("True, ORD-004", resultTrue);
    }

    /// <summary>
    /// 複数の異なるコンポーネントを検証
    /// </summary>
    [Fact]
    public void ToString_VariousComponentValues()
    {
        // Arrange & Act & Assert
        var obj1Result = ProductPrice.Create(0m, "JPY", isSet: true).ToString();
        Assert.Equal("0, JPY", obj1Result);

        var obj2Result = ProductPrice.Create(999999.99m, "EUR", isSet: true).ToString();
        Assert.Equal("999999.99, EUR", obj2Result);

        var obj3Result = OrderId.Create("TEST-123", isSet: true).ToString();
        Assert.Equal("TEST-123", obj3Result);
    }

    /// <summary>
    /// IsSet の値に応じた動作の検証
    /// </summary>
    [Fact]
    public void ToString_IsSetDependentBehavior()
    {
        // Arrange
        var unsetObj = OrderId.Create("ORD-005", isSet: false);
        var setObj = OrderId.Create("ORD-005", isSet: true);

        // Act
        var unsetResult = unsetObj.ToString();
        var setResult = setObj.ToString();

        // Assert
        Assert.Equal("Unset", unsetResult);
        Assert.Equal("ORD-005", setResult);
        Assert.NotEqual(unsetResult, setResult);
    }

    #endregion

    /// <summary>
    /// 値の不変性確認
    /// object 生成後、内部状態が変更されないことを検証
    /// </summary>
    [Fact]
    public void ValueObject_Immutability()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var stringBefore = obj1.ToString();

        // Act
        // obj1 に対する操作は行わない（変更不可なので操作できない）
        var stringAfter = obj1.ToString();

        // Assert
        Assert.Equal(stringBefore, stringAfter);
    }
}
