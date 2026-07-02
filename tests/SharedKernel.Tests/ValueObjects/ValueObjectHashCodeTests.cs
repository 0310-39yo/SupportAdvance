namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の GetHashCode メソッドに関するテスト
/// 観点グループ HC: GetHashCode メソッド
/// </summary>
public class ValueObjectHashCodeTests
{
    /// <summary>
    /// 観点: VO-HC-01
    /// Equals=true の 2 つのオブジェクトは同一ハッシュ値を返す（ハッシュ整合性）
    /// パターン: 4.4.2.1 - 同値オブジェクトのハッシュ比較
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
    /// パターン: 4.4.2.2 - 複数のオブジェクトペアで検証
    /// </summary>
    [Fact]
    public void VO_HC_01_MultipleObjectPairs_SameHashCode()
    {
        // Arrange
        var pairs = new[]
        {
            new { obj1 = OrderId.Create("ORD-001", isSet: true), obj2 = OrderId.Create("ORD-001", isSet: true) },
            new { obj1 = OrderId.Create("ORD-002", isSet: true), obj2 = OrderId.Create("ORD-002", isSet: true) },
            new { obj1 = OrderId.Create("ORD-003", isSet: false), obj2 = OrderId.Create("ORD-003", isSet: false) }
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
}
