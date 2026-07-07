using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.Tests.ValueObjects;

/// <summary>
/// RespondentAt の単体テスト
/// </summary>
public sealed class RespondentAtTests
{
    #region IsSet プロパティテスト

    /// <summary>VO-IS-01: IsSet=true で構築したオブジェクトは IsSet が true を返す</summary>
    [Fact]
    public void VO_IS_01_From_WithValidDate_IsSetIsTrue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);

        // Act
        var result = RespondentAt.From(pastDate, clock);

        // Assert
        Assert.True(result.IsSet);
    }

    /// <summary>VO-IS-02: IsSet=false で構築したオブジェクトは IsSet が false を返す</summary>
    [Fact]
    public void VO_IS_02_Unset_CreatesUnsetInstance()
    {
        // Act
        var result = RespondentAt.Unset();

        // Assert
        Assert.False(result.IsSet);
    }

    #endregion

    #region Clock 依存検証テスト

    /// <summary>VO-CLK-01: 過去日は検証成功、IsSet=true で返す</summary>
    [Fact]
    public void VO_CLK_01_From_WithPastDate_ReturnsInstance()
    {
        // Arrange
        var now = new DateTime(2026, 7, 8, 12, 0, 0);
        var clock = new MockClock(now);
        var pastDate = now.AddHours(-1);

        // Act
        var result = RespondentAt.From(pastDate, clock);

        // Assert
        Assert.True(result.IsSet);
        Assert.Equal(pastDate, result.Value);
    }

    /// <summary>VO-CLK-02: 現在日時は検証成功、IsSet=true で返す</summary>
    [Fact]
    public void VO_CLK_02_From_WithCurrentDateTime_ReturnsInstance()
    {
        // Arrange
        var now = new DateTime(2026, 7, 8, 12, 0, 0);
        var clock = new MockClock(now);

        // Act
        var result = RespondentAt.From(now, clock);

        // Assert
        Assert.True(result.IsSet);
        Assert.Equal(now, result.Value);
    }

    /// <summary>VO-CLK-03: 未来日は ArgumentException をスロー</summary>
    [Fact]
    public void VO_CLK_03_From_WithFutureDate_ThrowsArgumentException()
    {
        // Arrange
        var now = new DateTime(2026, 7, 8, 12, 0, 0);
        var clock = new MockClock(now);
        var futureDate = now.AddHours(1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => RespondentAt.From(futureDate, clock));
        Assert.Contains("cannot be in the future", ex.Message);
    }

    /// <summary>VO-CLK-04: DateTime.MinValue は ArgumentException をスロー</summary>
    [Fact]
    public void VO_CLK_04_From_WithMinValue_ThrowsArgumentException()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => RespondentAt.From(DateTime.MinValue, clock));
        Assert.Contains("must be a valid system timestamp", ex.Message);
    }

    /// <summary>VO-CLK-05: DateTime.MaxValue は ArgumentException をスロー</summary>
    [Fact]
    public void VO_CLK_05_From_WithMaxValue_ThrowsArgumentException()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => RespondentAt.From(DateTime.MaxValue, clock));
        Assert.Contains("must be a valid system timestamp", ex.Message);
    }

    #endregion

    #region Optional メソッドテスト（IOptionalValidateWithClock）

    /// <summary>VO-OPT-01: Unset() は IsSet=false のインスタンスを返す</summary>
    [Fact]
    public void VO_OPT_01_Unset_CreatesUnsetInstance()
    {
        // Act
        var result = RespondentAt.Unset();

        // Assert
        Assert.False(result.IsSet);
        Assert.Equal("Unset", result.ToString());
    }

    /// <summary>VO-OPT-02: From(value, clock) は IsSet=true のインスタンスを返す</summary>
    [Fact]
    public void VO_OPT_02_From_WithValidDate_CreatesInstance()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);

        // Act
        var result = RespondentAt.From(pastDate, clock);

        // Assert
        Assert.True(result.IsSet);
        Assert.Equal(pastDate, result.Value);
    }

    /// <summary>VO-OPT-03: TryFrom(null, clock) は true を返し、Unset インスタンスを返す</summary>
    [Fact]
    public void VO_OPT_03_TryFrom_WithNullInput_ReturnsTrueAndUnset()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

        // Act
        var success = RespondentAt.TryFrom((DateTime?)null, clock, out var result);

        // Assert
        Assert.True(success);
        Assert.False(result.IsSet);
    }

    /// <summary>VO-OPT-04: TryFrom(valid, clock) は true を返し、設定済みインスタンスを返す</summary>
    [Fact]
    public void VO_OPT_04_TryFrom_WithValidDate_ReturnsTrueAndInstance()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);

        // Act
        var success = RespondentAt.TryFrom(pastDate, clock, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsSet);
        Assert.Equal(pastDate, result.Value);
    }

    /// <summary>VO-OPT-05: TryFrom(invalid, clock) は false を返し、Unset インスタンスを返す</summary>
    [Theory]
    [MemberData(nameof(GetInvalidDateTimesForTryFrom))]
    public void VO_OPT_05_TryFrom_WithInvalidDate_ReturnsFalseAndUnset(DateTime invalidDate)
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

        // Act
        var success = RespondentAt.TryFrom(invalidDate, clock, out var result);

        // Assert
        Assert.False(success);
        Assert.False(result.IsSet);
    }

    /// <summary>VO-OPT-06: TryGetValue(out value) は IsSet=true で true を返す</summary>
    [Fact]
    public void VO_OPT_06_TryGetValue_WithSetInstance_ReturnsTrueAndValue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);
        var respondentAt = RespondentAt.From(pastDate, clock);

        // Act
        var success = respondentAt.TryGetValue(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(pastDate, value);
    }

    /// <summary>VO-OPT-07: TryGetValue(out value) は IsSet=false で false を返す</summary>
    [Fact]
    public void VO_OPT_07_TryGetValue_WithUnsetInstance_ReturnsFalse()
    {
        // Arrange
        var unset = RespondentAt.Unset();

        // Act
        var success = unset.TryGetValue(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default(DateTime), value);
    }

    #endregion

    #region Equals テスト

    /// <summary>VO-EQ-01: 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価</summary>
    [Fact]
    public void VO_EQ_01_Equals_WithSameDateAndIsSet_ReturnsTrue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date = new DateTime(2026, 7, 8, 10, 0, 0);
        var respondentAt1 = RespondentAt.From(date, clock);
        var respondentAt2 = RespondentAt.From(date, clock);

        // Act & Assert
        Assert.True(respondentAt1.Equals(respondentAt2));
        Assert.Equal(respondentAt1, respondentAt2);
    }

    /// <summary>VO-EQ-02: 同一参照のオブジェクトは等価</summary>
    [Fact]
    public void VO_EQ_02_Equals_WithSameReference_ReturnsTrue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), clock);

        // Act & Assert
        Assert.True(respondentAt.Equals(respondentAt));
    }

    /// <summary>VO-EQ-04: 両方が IsSet=false かつ値が同じ場合は等価</summary>
    [Fact]
    public void VO_EQ_04_Equals_WithBothUnset_ReturnsTrue()
    {
        // Arrange
        var unset1 = RespondentAt.Unset();
        var unset2 = RespondentAt.Unset();

        // Act & Assert
        Assert.True(unset1.Equals(unset2));
        Assert.Equal(unset1, unset2);
    }

    #endregion

    #region 非等価テスト

    /// <summary>VO-NE-01: コンポーネント値が異なる場合は非等価</summary>
    [Fact]
    public void VO_NE_01_Equals_WithDifferentDates_ReturnsFalse()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date1 = new DateTime(2026, 7, 8, 10, 0, 0);
        var date2 = new DateTime(2026, 7, 8, 9, 0, 0);
        var respondentAt1 = RespondentAt.From(date1, clock);
        var respondentAt2 = RespondentAt.From(date2, clock);

        // Act & Assert
        Assert.False(respondentAt1.Equals(respondentAt2));
        Assert.NotEqual(respondentAt1, respondentAt2);
    }

    /// <summary>VO-NE-02: IsSet が異なる場合は非等価</summary>
    [Fact]
    public void VO_NE_02_Equals_WithDifferentIsSet_ReturnsFalse()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), clock);
        var unset = RespondentAt.Unset();

        // Act & Assert
        Assert.False(respondentAt.Equals(unset));
        Assert.NotEqual(respondentAt, unset);
    }

    /// <summary>VO-NE-04: null との比較は非等価</summary>
    [Fact]
    public void VO_NE_04_Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), clock);

        // Act & Assert
        Assert.False(respondentAt.Equals(null));
    }

    #endregion

    #region GetHashCode テスト

    /// <summary>VO-HC-01: Equals=true の 2 つのオブジェクトは同一ハッシュ値</summary>
    [Fact]
    public void VO_HC_01_GetHashCode_WithEqualInstances_ReturnsSameHashCode()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date = new DateTime(2026, 7, 8, 10, 0, 0);
        var respondentAt1 = RespondentAt.From(date, clock);
        var respondentAt2 = RespondentAt.From(date, clock);

        // Act & Assert
        Assert.Equal(respondentAt1.GetHashCode(), respondentAt2.GetHashCode());
    }

    /// <summary>VO-HC-02: IsSet が異なるとハッシュ値が異なる</summary>
    [Fact]
    public void VO_HC_02_GetHashCode_WithDifferentIsSet_ReturnsDifferentHashCode()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), clock);
        var unset = RespondentAt.Unset();

        // Act & Assert
        Assert.NotEqual(respondentAt.GetHashCode(), unset.GetHashCode());
    }

    /// <summary>VO-HC-04: ハッシュ値は複数呼び出しで一貫している</summary>
    [Fact]
    public void VO_HC_04_GetHashCode_MultipleInvocations_ReturnsSameValue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), clock);

        // Act
        var hashCode1 = respondentAt.GetHashCode();
        var hashCode2 = respondentAt.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    #endregion

    #region 演算子テスト

    /// <summary>VO-OP-01: 等価なオブジェクトに == を適用すると true</summary>
    [Fact]
    public void VO_OP_01_Equality_WithEqualInstances_ReturnsTrue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date = new DateTime(2026, 7, 8, 10, 0, 0);
        var respondentAt1 = RespondentAt.From(date, clock);
        var respondentAt2 = RespondentAt.From(date, clock);

        // Act & Assert
        Assert.True(respondentAt1 == respondentAt2);
    }

    /// <summary>VO-OP-02: 非等価なオブジェクトに == を適用すると false</summary>
    [Fact]
    public void VO_OP_02_Equality_WithDifferentInstances_ReturnsFalse()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date1 = new DateTime(2026, 7, 8, 10, 0, 0);
        var date2 = new DateTime(2026, 7, 8, 9, 0, 0);
        var respondentAt1 = RespondentAt.From(date1, clock);
        var respondentAt2 = RespondentAt.From(date2, clock);

        // Act & Assert
        Assert.False(respondentAt1 == respondentAt2);
    }

    /// <summary>VO-OP-05: != は == の否定と一致</summary>
    [Fact]
    public void VO_OP_05_Inequality_WithDifferentInstances_ReturnsTrue()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date1 = new DateTime(2026, 7, 8, 10, 0, 0);
        var date2 = new DateTime(2026, 7, 8, 9, 0, 0);
        var respondentAt1 = RespondentAt.From(date1, clock);
        var respondentAt2 = RespondentAt.From(date2, clock);

        // Act & Assert
        Assert.True(respondentAt1 != respondentAt2);
        Assert.False(respondentAt1 == respondentAt2);
    }

    #endregion

    #region ToString テスト

    /// <summary>VO-TS-01: IsSet=false のとき "Unset" を返す</summary>
    [Fact]
    public void VO_TS_01_ToString_WithUnsetInstance_ReturnsUnsetString()
    {
        // Arrange
        var unset = RespondentAt.Unset();

        // Act
        var result = unset.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>VO-TS-02: IsSet=true のとき、DateTime を文字列で返す</summary>
    [Fact]
    public void VO_TS_02_ToString_WithSetInstance_ReturnsDateTimeString()
    {
        // Arrange
        var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
        var date = new DateTime(2026, 7, 8, 10, 30, 0);
        var respondentAt = RespondentAt.From(date, clock);

        // Act
        var result = respondentAt.ToString();

        // Assert
        Assert.NotEmpty(result);
        Assert.NotEqual("Unset", result);
        Assert.Contains("2026", result);  // 年号を含む
    }

    #endregion

    #region ヘルパーメソッド

    public static TheoryData<DateTime> GetInvalidDateTimesForTryFrom()
    {
        var now = new DateTime(2026, 7, 8, 12, 0, 0);
        return new TheoryData<DateTime>
        {
            DateTime.MinValue,                  // MinValue
            DateTime.MaxValue,                  // MaxValue
            now.AddHours(1),                    // 未来日
            now.AddDays(1),                     // 明日
        };
    }

    #endregion
}
