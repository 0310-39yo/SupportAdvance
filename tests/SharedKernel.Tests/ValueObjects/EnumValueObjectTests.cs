using SupportAdvance.Tests.SharedKernel.Tests.ValueObjects.Fixtures;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using Xunit;

/// <summary>
/// EnumValueObject<TValue> の単体テスト仕様書に基づくテストクラス
/// 仕様書：docs/SharedKernel/ValueObjects/EnumValueObject_単体テスト仕様書.md
/// </summary>
public class EnumValueObjectTests
{
    // ================================================================================
    // 2.1 IsSet プロパティのテスト
    // ================================================================================

    /// <summary>
    /// Test-IsSet-001: IsSet は常に true を返す
    /// </summary>
    [Fact]
    public void IsSet_常に_True_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;

        // Act
        var result = draft.IsSet;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test-IsSet-002: 複数インスタンスで IsSet は常に true
    /// </summary>
    [Fact]
    public void IsSet_複数インスタンス_すべてTrue_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;
        var completed = OrderStatus.Completed;

        // Act
        var result1 = draft.IsSet;
        var result2 = approved.IsSet;
        var result3 = completed.IsSet;

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    // ================================================================================
    // 2.2 TryGetValue メソッドのテスト
    // ================================================================================

    /// <summary>
    /// Test-TryGetValue-001: TryGetValue は常に true を返す
    /// </summary>
    [Fact]
    public void TryGetValue_常に_True_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;

        // Act
        var result = draft.TryGetValue(out int value);

        // Assert
        Assert.True(result);
        Assert.Equal(1, value);
    }

    /// <summary>
    /// Test-TryGetValue-002: out パラメータに正しい内部値が格納される
    /// </summary>
    [Fact]
    public void TryGetValue_out_パラメータ_正しい内部値_を格納_する()
    {
        // Arrange
        var approved = OrderStatus.Approved;

        // Act
        var result = approved.TryGetValue(out int value);

        // Assert
        Assert.True(result);
        Assert.Equal(2, value);
    }

    /// <summary>
    /// Test-TryGetValue-003: 複数の選択肢で異なる内部値が取得できる
    /// </summary>
    [Fact]
    public void TryGetValue_複数選択肢_各値に対応_した内部値_を返_す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;
        var completed = OrderStatus.Completed;

        // Act
        draft.TryGetValue(out int value1);
        approved.TryGetValue(out int value2);
        completed.TryGetValue(out int value3);

        // Assert
        Assert.Equal(1, value1);
        Assert.Equal(2, value2);
        Assert.Equal(3, value3);
    }

    // ================================================================================
    // 2.3 Validate メソッドのテスト（派生クラス経由）
    // ================================================================================

    /// <summary>
    /// Test-Validate-001: 無効な値で ArgumentOutOfRangeException がスローされる
    /// </summary>
    [Fact]
    public void Validate_無効な値_ArgumentOutOfRangeException_を投げる()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderStatus.From(99));
    }

    /// <summary>
    /// Test-Validate-002: 有効な値では例外がスローされない
    /// </summary>
    [Fact]
    public void Validate_有効な値_インスタンス生成_に成功_する()
    {
        // Arrange & Act
        var result = OrderStatus.From(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Draft, result);
    }

    /// <summary>
    /// Test-Validate-003: 境界値で Validate が正確に動作する
    /// </summary>
    [Fact]
    public void Validate_境界値_上限超過時_に例外_を投げる()
    {
        // Arrange & Act & Assert
        var validResult = OrderStatus.From(3);  // 上限値
        Assert.Equal(OrderStatus.Completed, validResult);

        Assert.Throws<ArgumentOutOfRangeException>(() => OrderStatus.From(4));  // 上限超過
    }

    // ================================================================================
    // 2.4 GetDisplayName メソッドのテスト（ToString 経由）
    // ================================================================================

    /// <summary>
    /// Test-GetDisplayName-001: ToString() が GetDisplayName() の結果を返す
    /// </summary>
    [Fact]
    public void ToString_GetDisplayName_経由_表示名_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;

        // Act
        var result = draft.ToString();

        // Assert
        Assert.Equal("下書き", result);
    }

    /// <summary>
    /// Test-GetDisplayName-002: 各選択肢で異なる表示名が返される
    /// </summary>
    [Fact]
    public void ToString_複数選択肢_各値に対応_した表示名_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;
        var completed = OrderStatus.Completed;

        // Act
        var result1 = draft.ToString();
        var result2 = approved.ToString();
        var result3 = completed.ToString();

        // Assert
        Assert.Equal("下書き", result1);
        Assert.Equal("承認済", result2);
        Assert.Equal("完了", result3);
    }

    /// <summary>
    /// Test-GetDisplayName-003: 表示名は空文字列でない
    /// </summary>
    [Fact]
    public void ToString_表示名_空文字列_でない()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;
        var completed = OrderStatus.Completed;

        // Act
        var result1 = draft.ToString();
        var result2 = approved.ToString();
        var result3 = completed.ToString();

        // Assert
        Assert.False(string.IsNullOrEmpty(result1));
        Assert.False(string.IsNullOrEmpty(result2));
        Assert.False(string.IsNullOrEmpty(result3));
    }

    // ================================================================================
    // 2.5 GetEqualityComponents メソッドのテスト（Equals/GetHashCode 経由）
    // ================================================================================

    /// <summary>
    /// Test-EqualityComp-001: 同じ値の 2 つのインスタンスは等価
    /// </summary>
    [Fact]
    public void Equals_同じ値_True_を返す()
    {
        // Arrange
        var draft1 = OrderStatus.From(1);
        var draft2 = OrderStatus.From(1);

        // Act
        var result = draft1.Equals(draft2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test-EqualityComp-002: 異なる値の 2 つのインスタンスは非等価
    /// </summary>
    [Fact]
    public void Equals_異なる値_False_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;

        // Act
        var result = draft.Equals(approved);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test-EqualityComp-003: 等価なインスタンスは同じハッシュコードを持つ
    /// </summary>
    [Fact]
    public void GetHashCode_等価_同じハッシュコード_を返す()
    {
        // Arrange
        var draft1 = OrderStatus.From(1);
        var draft2 = OrderStatus.From(1);

        // Act
        var hashCode1 = draft1.GetHashCode();
        var hashCode2 = draft2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    /// <summary>
    /// Test-EqualityComp-004: 非等価なインスタンスは異なるハッシュコードを持つ（通常）
    /// </summary>
    [Fact]
    public void GetHashCode_非等価_異なるハッシュコード_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;

        // Act
        var hashCode1 = draft.GetHashCode();
        var hashCode2 = approved.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    // ================================================================================
    // 2.6 ToString メソッドのテスト
    // ================================================================================

    /// <summary>
    /// Test-ToString-001: ToString() は GetDisplayName() を呼び出す
    /// </summary>
    [Fact]
    public void ToString_GetDisplayName_経由_表示名_を返す_詳細確認()
    {
        // Arrange
        var approved = OrderStatus.Approved;

        // Act
        var result = approved.ToString();

        // Assert
        Assert.Equal("承認済", result);
    }

    /// <summary>
    /// Test-ToString-002: ログ出力時に ToString() が使用できる
    /// </summary>
    [Fact]
    public void ToString_ログ出力_表示名_が出力_される()
    {
        // Arrange
        var status = OrderStatus.Approved;

        // Act
        var logOutput = $"Order status: {status.ToString()}";

        // Assert
        Assert.Contains("承認済", logOutput);
    }

    /// <summary>
    /// Test-ToString-003: MessageBox に ToString() が直接渡せる
    /// </summary>
    [Fact]
    public void ToString_UI表示_日本語_が表示_される()
    {
        // Arrange
        var statuses = new[] { OrderStatus.Draft, OrderStatus.Approved, OrderStatus.Completed };

        // Act & Assert
        foreach (var status in statuses)
        {
            var displayText = status.ToString();
            Assert.False(string.IsNullOrEmpty(displayText));
            // 実際の MessageBox 表示は手動確認が必要だが、ここでは文字列が存在することを検証
        }
    }

    // ================================================================================
    // 2.7 静的メンバーのテスト（派生クラス経由）
    // ================================================================================

    /// <summary>
    /// Test-Static-001: 静的フィールドが正しく初期化されている
    /// </summary>
    [Fact]
    public void StaticField_定義済み_正しくInitialize_されている()
    {
        // Arrange & Act
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;
        var completed = OrderStatus.Completed;

        // Assert
        Assert.NotNull(draft);
        Assert.NotNull(approved);
        Assert.NotNull(completed);
    }

    /// <summary>
    /// Test-Static-002: From メソッドが値から逆引きできる
    /// </summary>
    [Fact]
    public void From_有効な値_対応_する選択肢_を返す()
    {
        // Arrange & Act
        var draft = OrderStatus.From(1);
        var approved = OrderStatus.From(2);
        var completed = OrderStatus.From(3);

        // Assert
        Assert.Equal(OrderStatus.Draft, draft);
        Assert.Equal(OrderStatus.Approved, approved);
        Assert.Equal(OrderStatus.Completed, completed);
    }

    /// <summary>
    /// Test-Static-003: From メソッドが無効な値で例外を投げる
    /// </summary>
    [Fact]
    public void From_無効な値_例外_を投げる()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderStatus.From(99));
    }

    /// <summary>
    /// Test-Static-004: From メソッドが複数の値に対応した選択肢を返す
    /// </summary>
    [Fact]
    public void From_複数値_各値に対応_した選択肢_を返す()
    {
        // Arrange
        int[] values = { 1, 2, 3 };

        // Act
        var results = values.Select(v => OrderStatus.From(v)).ToArray();

        // Assert
        Assert.Equal(3, results.Length);
        Assert.Equal(OrderStatus.Draft, results[0]);
        Assert.Equal(OrderStatus.Approved, results[1]);
        Assert.Equal(OrderStatus.Completed, results[2]);
    }

    // ================================================================================
    // 2.8 派生クラスの制約チェック
    // ================================================================================

    /// <summary>
    /// Test-Constraint-001: sealed クラスは派生禁止（コンパイルエラー）
    /// </summary>
    [Fact]
    public void Sealed_派生禁止_コンパイルエラー_になる()
    {
        // Note: このテストは実際にはコンパイル時に検証される
        // 実行時には OrderStatus が sealed であることを確認する
        var type = typeof(OrderStatus);
        Assert.True(type.IsSealed, "OrderStatus should be sealed");
    }

    /// <summary>
    /// Test-Constraint-002: コンストラクタは private（外部からインスタンス化不可）
    /// </summary>
    [Fact]
    public void Constructor_Private_外部からインスタンス化_不可()
    {
        // Arrange
        var type = typeof(OrderStatus);
        var constructors = type.GetConstructors();

        // Assert
        Assert.Empty(constructors);  // public なコンストラクタが存在しない
    }

    /// <summary>
    /// Test-Constraint-003: TryFrom は実装されない（IOptionalValueObject 非実装）
    /// </summary>
    [Fact]
    public void TryFrom_未実装_存在_しない()
    {
        // Arrange
        var type = typeof(OrderStatus);
        var method = type.GetMethod("TryFrom");

        // Assert
        Assert.Null(method);  // TryFrom メソッドが存在しない
    }

    // ================================================================================
    // 追加テスト：等価性のハッシュセット動作
    // ================================================================================

    /// <summary>
    /// HashSet での動作確認（等価性が正しく機能）
    /// </summary>
    [Fact]
    public void Equals_HashSet_等価インスタンス_は重複排除_される()
    {
        // Arrange
        var draft1 = OrderStatus.From(1);
        var draft2 = OrderStatus.From(1);
        var approved = OrderStatus.From(2);

        var hashSet = new HashSet<OrderStatus> { draft1, draft2, approved };

        // Act & Assert
        Assert.Equal(2, hashSet.Count);  // draft1 と draft2 が同じと見なされて重複排除
    }

    /// <summary>
    /// Dictionary での動作確認（キー機能）
    /// </summary>
    [Fact]
    public void Equals_Dictionary_等価インスタンス_を同じキー_と認識_する()
    {
        // Arrange
        var key1 = OrderStatus.From(1);
        var key1Same = OrderStatus.From(1);

        var dict = new Dictionary<OrderStatus, string> { { key1, "Draft Status" } };

        // Act
        var found = dict.TryGetValue(key1Same, out var value);

        // Assert
        Assert.True(found);
        Assert.Equal("Draft Status", value);
    }

    /// <summary>
    /// 演算子オーバーロード == の確認
    /// </summary>
    [Fact]
    public void OperatorEqual_等価インスタンス_True_を返す()
    {
        // Arrange
        var draft1 = OrderStatus.From(1);
        var draft2 = OrderStatus.From(1);

        // Act
        var result = draft1 == draft2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 演算子オーバーロード != の確認
    /// </summary>
    [Fact]
    public void OperatorNotEqual_異なるインスタンス_True_を返す()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        var approved = OrderStatus.Approved;

        // Act
        var result = draft != approved;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 静的フィールド参照の等価性確認
    /// </summary>
    [Fact]
    public void StaticFieldReference_同じフィールド参照_等価()
    {
        // Arrange & Act
        var draft1 = OrderStatus.Draft;
        var draft2 = OrderStatus.Draft;

        // Assert
        Assert.Equal(draft1, draft2);
        Assert.True(ReferenceEquals(draft1, draft2));  // 実は同じインスタンス参照
    }

    // ================================================================================
    // 2.8+ デフォルト値(0) vs Unset(null相当) の識別テスト（0 vs null セマンティクス）
    // ================================================================================

    /// <summary>
    /// Test-DefaultValue-001: ValueField=0 でも IsSet=true なら値確定
    /// EnumValueObject 自体は常に IsSet=true（明示的なテスト）
    /// </summary>
    [Fact]
    public void EnumValueObject_AlwaysHasIsSetTrue_EvenWithDefaultValue()
    {
        // Arrange: OrderStatus は常に IsSet=true（派生クラスレベル）
        var draft = OrderStatus.Draft;

        // Act
        var isSet = draft.IsSet;
        var hasTryGetValue = draft.TryGetValue(out var value);

        // Assert
        Assert.True(isSet);
        Assert.True(hasTryGetValue);
        Assert.Equal(1, value);
    }

    /// <summary>
    /// Test-DefaultValue-002: From() で復元後も IsSet=true が保持される
    /// </summary>
    [Fact]
    public void From_CreatedInstance_AlwaysHasIsSetTrue()
    {
        // Arrange & Act
        var created = OrderStatus.From(2);

        // Assert
        Assert.True(created.IsSet);
        Assert.True(created.TryGetValue(out var value));
        Assert.Equal(2, value);
    }

    /// <summary>
    /// Test-DefaultValue-003: Equals/GetHashCode は IsSet に基づく
    /// （同じ選択肢なら等価、IsSet 状態も含めて比較）
    /// </summary>
    [Fact]
    public void Equality_IsBasedOnIsSet_AndValueField()
    {
        // Arrange
        var draft1 = OrderStatus.Draft;
        var draft2 = OrderStatus.From(1);  // Draft と同じ値

        // Act
        var areEqual = draft1.Equals(draft2);
        var hashEqual = draft1.GetHashCode() == draft2.GetHashCode();

        // Assert
        Assert.True(areEqual);
        Assert.True(hashEqual);
    }

    /// <summary>
    /// Test-DefaultValue-004: ToString は IsSet に基づいて表示
    /// EnumValueObject は常に IsSet=true なので GetDisplayName() を呼び出す
    /// </summary>
    [Fact]
    public void ToString_Returns_DisplayName_WhenIsSetTrue()
    {
        // Arrange
        var completed = OrderStatus.Completed;

        // Act
        var stringRepresentation = completed.ToString();

        // Assert
        Assert.NotEmpty(stringRepresentation);
        Assert.NotEqual("Unset", stringRepresentation);  // "Unset" にはならない（IsSet是常に true）
        Assert.Equal("完了", stringRepresentation);
    }

    /// <summary>
    /// Test-DefaultValue-005: All static instances have IsSet=true
    /// EnumValueObject は派生クラスでも常に IsSet=true
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AllStaticInstances_HaveIsSetTrue(int value)
    {
        // Arrange & Act
        var instance = OrderStatus.From(value);

        // Assert
        Assert.True(instance.IsSet);
        Assert.True(instance.TryGetValue(out _));
    }

    // ================================================================================
    // 追加: null 比較テスト（VO-OP-03, VO-OP-04）
    // ================================================================================

    /// <summary>
    /// VO-OP-03: 両辺が null のとき == は true
    /// </summary>
    [Fact]
    public void OperatorEqual_BothNull_ReturnsTrue()
    {
        // Arrange
        OrderStatus? nullLeft = null;
        OrderStatus? nullRight = null;

        // Act
        var result = nullLeft == nullRight;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// VO-OP-04: 片方のみ null のとき == は false
    /// </summary>
    [Fact]
    public void OperatorEqual_OneNull_ReturnsFalse()
    {
        // Arrange
        OrderStatus? nullValue = null;
        OrderStatus nonNullValue = OrderStatus.Draft;

        // Act
        var resultNullLeft = nullValue == nonNullValue;
        var resultNullRight = nonNullValue == nullValue;

        // Assert
        Assert.False(resultNullLeft);
        Assert.False(resultNullRight);
    }

    /// <summary>
    /// VO-NE-03: 型が異なる場合は非等価
    /// </summary>
    [Fact]
    public void Equals_DifferentType_NotEqual()
    {
        // Arrange
        var orderStatus = OrderStatus.Draft;
        object differentType = 1;  // int 型

        // Act
        var result = orderStatus.Equals(differentType);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// null との比較（ValueObject.Equals での null チェック）
    /// </summary>
    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        OrderStatus? nullValue = null;

        // Act
        var result = draft.Equals(nullValue);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// != 演算子での null 比較
    /// </summary>
    [Fact]
    public void OperatorNotEqual_WithNull_ReturnsTrue()
    {
        // Arrange
        var draft = OrderStatus.Draft;
        OrderStatus? nullValue = null;

        // Act
        var result = draft != nullValue;

        // Assert
        Assert.True(result);
    }
}
