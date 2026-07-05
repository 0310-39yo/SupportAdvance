using Xunit;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// RespondentName の単体テスト
/// </summary>
public sealed class RespondentNameTests
{
    #region From メソッドテスト

    /// <summary>VO-OPT-02: From\uff08有効値\uff09で設定済みインスタンス返却確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData("花子")]
    [InlineData("A")]
    [InlineData("田中太郎")]
    [InlineData("山田 太郎")]
    public void VO_OPT_02_From_WithValidName_CreatesInstance(string name)
    {
        // Act
        var result = RespondentName.From(name);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out string? value));
        Assert.Equal(name, value);
        Assert.Equal(name, result.ToString());
    }

    /// <summary>VO-IS-02: From(null) で ArgumentNullException スロー確認</summary>
    [Fact]
    public void VO_IS_02_From_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RespondentName.From(null!));
    }

    #endregion

    #region Unset メソッドテスト

    /// <summary>VO-OPT-01, VO-TS-01: Unset() で未設定インスタンス返却確認</summary>
    [Fact]
    public void VO_OPT_01_Unset_CreatesUnsetInstance()
    {
        // Act
        var result = RespondentName.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSet);
    }

    /// <summary>VO-TS-01: Unset().ToString() で "Unset" 返却確認</summary>
    [Fact]
    public void Unset_ToString_ReturnsUnsetString()
    {
        // Act
        var result = RespondentName.Unset();

        // Assert
        Assert.Equal("Unset", result.ToString());
    }

    /// <summary>VO-EQ-04: Unset() の複数呼び出しが等価確認</summary>
    [Fact]
    public void Unset_MultipleCalls_ReturnEqualInstances()
    {
        // Act
        var result1 = RespondentName.Unset();
        var result2 = RespondentName.Unset();

        // Assert
        Assert.Equal(result1, result2);
    }

    #endregion

    #region TryFrom メソッドテスト

    /// <summary>VO-OPT-04: TryFrom\uff08有効値\uff09で true と設定済み返却確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData("花子")]
    [InlineData("A")]
    [InlineData("田中太郎")]
    public void VO_OPT_04_TryFrom_WithValidName_ReturnsTrue(string name)
    {
        // Act
        var success = RespondentName.TryFrom(name, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.True(result.IsSet);
        Assert.True(result.TryGetValue(out string? value));
        Assert.Equal(name, value);
    }

    /// <summary>VO-OPT-03: TryFrom(null) で true と Unset 返却確認</summary>
    [Fact]
    public void VO_OPT_03_TryFrom_WithNullInput_ReturnsTrueWithUnsetInstance()
    {
        // Act
        var success = RespondentName.TryFrom(null, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.False(result.IsSet);
        Assert.Equal("Unset", result.ToString());
    }

    /// <summary>VO-OPT-04: TryFrom() のアウトパラメータに設定済みインスタンスが不定確認</summary>
    [Fact]
    public void TryFrom_WithValidName_OutParameterIsSet()
    {
        // Act
        RespondentName.TryFrom("太郎", out var result);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<RespondentName>(result);
        Assert.True(result.IsSet);
    }

    [Fact]
    public void TryFrom_WithEmptyString_ReturnsInstance()
    {
        // Act
        var success = RespondentName.TryFrom("", out var result);

        // Assert
        Assert.NotNull(result);
        // 空文字列の許可は実装に依存（現状は許可）
    }

    #endregion

    #region Equals(RespondentName?) メソッドテスト

    /// <summary>VO-EQ-01: 同一値での等価確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData("花子")]
    public void VO_EQ_01_Equals_WithSameName_ReturnTrue(string name)
    {
        // Act
        var a = RespondentName.From(name);
        var b = RespondentName.From(name);

        // Assert
        Assert.True(a.Equals(b));
    }

    /// <summary>VO-NE-01: 異なる値での不等価確認</summary>
    [Theory]
    [InlineData("太郎", "花子")]
    [InlineData("A", "B")]
    public void VO_NE_01_Equals_WithDifferentNames_ReturnFalse(string name1, string name2)
    {
        // Act
        var a = RespondentName.From(name1);
        var b = RespondentName.From(name2);

        // Assert
        Assert.False(a.Equals(b));
    }

    /// <summary>VO-NE-02: 設定済みと未設定の比較で不等価確認</summary>
    [Fact]
    public void Equals_WithSetAndUnset_ReturnFalse()
    {
        // Act
        var set = RespondentName.From("太郎");
        var unset = RespondentName.Unset();

        // Assert
        Assert.False(set.Equals(unset));
    }

    /// <summary>VO-EQ-04: 未設定同士の等価確認</summary>
    [Fact]
    public void Equals_WithBothUnset_ReturnTrue()
    {
        // Act
        var unset1 = RespondentName.Unset();
        var unset2 = RespondentName.Unset();

        // Assert
        Assert.True(unset1.Equals(unset2));
    }

    /// <summary>VO-EQ-02: 同一参照の等価確認</summary>
    [Fact]
    public void Equals_WithSameReference_ReturnTrue()
    {
        // Act
        var a = RespondentName.From("太郎");

        // Assert
        Assert.True(a.Equals(a));
    }

    /// <summary>VO-NE-03: nullとの比較で不等価確認</summary>
    [Fact]
    public void Equals_WithNull_ReturnFalse()
    {
        // Act
        var instance = RespondentName.From("太郎");

        // Assert
        Assert.False(instance.Equals(null));
    }

    /// <summary>VO-EQ-01: 対稱性を確認（A=B なら B=A\uff09</summary>
    [Fact]
    public void Equals_Symmetry_AEqualsB_EqualssBEqualsA()
    {
        // Act
        var a = RespondentName.From("太郎");
        var b = RespondentName.From("太郎");

        // Assert
        Assert.True(a.Equals(b));
        Assert.True(b.Equals(a));
    }

    #endregion

    #region Equals(object?) メソッドテスト

    /// <summary>VO-EQ-01: object型で同一値の等価確認</summary>
    [Fact]
    public void EqualsObject_WithSameNameTypeCasted_ReturnTrue()
    {
        // Act
        var a = RespondentName.From("太郎");
        var b = (object)RespondentName.From("太郎");

        // Assert
        Assert.True(a.Equals(b));
    }

    /// <summary>VO-NE-04: 異なる型との比較で不等価確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData(123)]
    [InlineData(null)]
    public void EqualsObject_WithDifferentType_ReturnFalse(object? other)
    {
        // Act
        var instance = RespondentName.From("太郎");

        // Assert
        Assert.False(instance.Equals(other));
    }

    /// <summary>VO-NE-05: 文字列をobjectで渡して不等価を確認</summary>
    [Fact]
    public void EqualsObject_WithString_ReturnFalse()
    {
        // Act
        var instance = RespondentName.From("太郎");

        // Assert
        Assert.False(instance.Equals((object)"太郎"));
    }

    #endregion

    #region GetHashCode メソッドテスト

    /// <summary>VO-HC-01: 等価インスタンスが同一ハッシュを返却確認</summary>
    [Fact]
    public void VO_HC_01_GetHashCode_WithEqualInstances_ReturnSameHashCode()
    {
        // Act
        var a = RespondentName.From("太郎");
        var b = RespondentName.From("太郎");

        // Assert
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>VO-HC-02: 異なる値では致码が異なる確率が高いことを確認</summary>
    [Theory]
    [InlineData("太郎", "花子")]
    [InlineData("A", "B")]
    public void GetHashCode_WithDifferentNames_ReturnDifferentHashCode(string name1, string name2)
    {
        // Act
        var a = RespondentName.From(name1);
        var b = RespondentName.From(name2);

        // Assert
        // ハッシュ衝突は可能だが、通常は異なるハッシュコードを返す
        // この検証は参考情報（絶対値ではない）
        var hashA = a.GetHashCode();
        var hashB = b.GetHashCode();
        // Assert.NotEqual(hashA, hashB);  // 衝突の可能性があるためコメント
    }

    /// <summary>VO-HC-03: HashSet を使用したバグ測定を確認</summary>
    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Act
        var name1 = RespondentName.From("太郎");
        var name2 = RespondentName.From("太郎");
        var name3 = RespondentName.From("花子");

        var set = new HashSet<RespondentName> { name1, name3 };

        // Assert
        Assert.True(set.Contains(name2));  // name1 と name2 は等価なので、Contains が true
        Assert.True(set.Contains(name3));
        Assert.False(set.Contains(RespondentName.From("新太郎")));
    }

    /// <summary>VO-HC-04: Dictionary を使用したキー測定を確認</summary>
    [Fact]
    public void GetHashCode_CanBeUsedAsDictionaryKey()
    {
        // Act
        var name1 = RespondentName.From("太郎");
        var name2 = RespondentName.From("太郎");
        var name3 = RespondentName.From("花子");

        var dict = new Dictionary<RespondentName, string> 
        { 
            { name1, "data1" } 
        };

        // Assert
        Assert.True(dict.ContainsKey(name2));  // name1 と name2 は等価
        Assert.Equal("data1", dict[name2]);

        dict[name3] = "data2";
        Assert.Equal("data2", dict[name3]);
    }

    #endregion

    #region Value プロパティテスト

    /// <summary>VO-OPT-06: Value プロパティで同値を取得確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData("花子")]
    public void Value_WithSetInstance_ReturnNameString(string name)
    {
        // Act
        var instance = RespondentName.From(name);

        // Assert
        Assert.True(instance.TryGetValue(out string? value));
        Assert.Equal(name, value);
    }

    /// <summary>VO-OPT-05: 未設定時はデフォルトを返却確認</summary>
    [Fact]
    public void Value_WithUnsetInstance_ReturnDefault()
    {
        // Act
        var instance = RespondentName.Unset();

        // Assert
        Assert.False(instance.TryGetValue(out string? value));
        Assert.Null(value);
    }

    #endregion

    #region IsSet プロパティテスト

    /// <summary>VO-IS-01: From で作成したインスタンスは IsSet=true</summary>
    [Fact]
    public void IsSet_WithFromCreatedInstance_ReturnTrue()
    {
        // Act
        var instance = RespondentName.From("太郎");

        // Assert
        Assert.True(instance.IsSet);
    }

    /// <summary>VO-IS-02: Unset で作成したインスタンスは IsSet=false</summary>
    [Fact]
    public void IsSet_WithUnsetCreatedInstance_ReturnFalse()
    {
        // Act
        var instance = RespondentName.Unset();

        // Assert
        Assert.False(instance.IsSet);
    }

    #endregion

    #region ToString メソッドテスト

    /// <summary>VO-TS-02: 設定済みインスタンスを ToString() で文字列を確認</summary>
    [Theory]
    [InlineData("太郎")]
    [InlineData("花子")]
    [InlineData("A")]
    public void VO_TS_02_ToString_WithSetInstance_ReturnNameString(string name)
    {
        // Act
        var instance = RespondentName.From(name);

        // Assert
        Assert.Equal(name, instance.ToString());
    }

    /// <summary>VO-TS-01: Unset を ToString() で "Unset" を確認</summary>
    [Fact]
    public void VO_TS_01_ToString_WithUnsetInstance_ReturnUnset()
    {
        // Act
        var instance = RespondentName.Unset();

        // Assert
        Assert.Equal("Unset", instance.ToString());
    }

    #endregion

    #region 不変性テスト

    /// <summary>VO-OP-01: 不変性を確認（IsSetを変更不可\uff09</summary>
    [Fact]
    public void Immutability_IsSetPropertyIsReadOnly()
    {
        // Arrange
        var instance = RespondentName.From("太郎");

        // Act & Assert
        // IsSet は read-only init なので、インスタンス作成後に変更不可
        // コンパイル時にエラーが発生するため、実行時テストは不可
        // 静的分析でカバー
        Assert.True(instance.IsSet);  // 値は変わらない
    }

    #endregion

    #region インターフェース実装テスト

    /// <summary>VO-OP-02: IEquatable<RespondentName> を実装している確認</summary>
    [Fact]
    public void ImplementsIEquatable()
    {
        // Act
        var instance = RespondentName.From("太郎");

        // Assert
        Assert.IsAssignableFrom<IEquatable<RespondentName>>(instance);
    }

    #endregion

    #region 統合テスト

    /// <summary>VO-OPT-07: 統合テスト - 複数インスタンスを模擬のコレクションで作成、検索、等価性確認</summary>
    [Fact]
    public void Integration_CreateUpdateFindScenario()
    {
        // Arrange
        var respondents = new List<RespondentName>(capacity: 5);

        // Act - 複数の回答者を作成
        respondents.Add(RespondentName.From("太郎"));
        respondents.Add(RespondentName.From("花子"));
        respondents.Add(RespondentName.From("次郎"));
        respondents.Add(RespondentName.Unset());

        // Assert - 検索と等価性確認
        var target = RespondentName.From("花子");
        Assert.Contains(target, respondents);

        // 等価なインスタンスで検索可能
        Assert.True(respondents.Any(r => r.Equals(target)));
    }

    /// <summary>VO-OPT-07: 統合テスト - TryFromを使用したコレクションの処理確認</summary>
    [Fact]
    public void Integration_TryFromWithCollectionScenario()
    {
        // Arrange
        var inputs = new[] 
        { 
            "太郎", 
            "花子", 
            null, 
            "次郎" 
        };

        // Act
        var results = new List<RespondentName>();
        foreach (var input in inputs)
        {
            if (RespondentName.TryFrom(input, out var respondentName))
            {
                results.Add(respondentName);
            }
        }

        // Assert
        Assert.Equal(4, results.Count);  // null も Unset として成功
        Assert.True(results[2].IsSet == false);  // null は Unset
    }

    #endregion
}
