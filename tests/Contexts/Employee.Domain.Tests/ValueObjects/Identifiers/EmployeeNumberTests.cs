namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;

/// <summary>
/// EmployeeNumber ValueObject の単体テスト
/// </summary>
public class EmployeeNumberTests
{
    // ========== 観点グループ GEN：生成メソッド（From/FromDbValue） ==========

    [Fact]
    public void TestENGEN01_From1001MinValueReturnsValid()
    {
        // Act
        var result = EmployeeNumber.From(1001);

        // Assert
        Assert.Equal(1001, result.Value);
        Assert.True(result.IsSet);
    }

    [Fact]
    public void TestENGEN02_From1234MiddleValueReturnsValid()
    {
        // Act
        var result = EmployeeNumber.From(1234);

        // Assert
        Assert.Equal(1234, result.Value);
    }

    [Fact]
    public void TestENGEN03_From8499MaxValueReturnsValid()
    {
        // Act
        var result = EmployeeNumber.From(8499);

        // Assert
        Assert.Equal(8499, result.Value);
    }

    [Fact]
    public void TestENGEN04_From1000ReservedThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(1000));
    }

    [Fact]
    public void TestENGEN05_From999BelowRangeThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(999));
    }

    [Fact]
    public void TestENGEN07_From0ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(0));
    }

    [Fact]
    public void TestENGEN08_FromNegativeThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(-100));
    }

    [Fact]
    public void TestENGEN09_FromDbValue1234ReturnsValid()
    {
        // Act
        var result = EmployeeNumber.FromDbValue(1234);

        // Assert
        Assert.Equal(1234, result.Value);
    }

    [Fact]
    public void TestENGEN10_FromDbValue1000ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.FromDbValue(1000));
    }

    // ========== 観点グループ TRY：TryFrom/TryFromDbValue ==========

    [Fact]
    public void TestENTRY01_TryFrom1234ReturnsTrueAndSetsResult()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(1234, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(1234, result.Value);
    }

    [Fact]
    public void TestENTRY02_TryFrom1001MinValueReturnsTrue()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(1001, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestENTRY03_TryFrom8499MaxValueReturnsTrue()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(8499, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestENTRY04_TryFrom1000ReservedReturnsFalse()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(1000, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestENTRY05_TryFrom999ReturnsFalse()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(999, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestENTRY06_TryFrom8500ReturnsTrue()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(8500, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(8500, result.Value);
    }

    [Fact]
    public void TestENTRY07_TryFromNullReturnsFalse()
    {
        // Act
        bool success = EmployeeNumber.TryFrom(null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestENTRY08_TryFromDbValue1234ReturnsTrue()
    {
        // Act
        bool success = EmployeeNumber.TryFromDbValue(1234, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(1234, result.Value);
    }

    [Fact]
    public void TestENTRY09_TryFromDbValue1000ReturnsFalse()
    {
        // Act
        bool success = EmployeeNumber.TryFromDbValue(1000, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestENTRY10_TryFromDbValueNullReturnsFalse()
    {
        // Act
        bool success = EmployeeNumber.TryFromDbValue(null, out _);

        // Assert
        Assert.False(success);
    }

    // ========== 観点グループ DISP：表示形式（ToString） ==========

    [Fact]
    public void TestENDISP01_From1001ToStringIs01001()
    {
        // Act
        var result = EmployeeNumber.From(1001).ToString();

        // Assert
        Assert.Equal("01001", result);
    }

    [Fact]
    public void TestENDISP02_From1234ToStringIs01234()
    {
        // Act
        var result = EmployeeNumber.From(1234).ToString();

        // Assert
        Assert.Equal("01234", result);
    }

    [Fact]
    public void TestENDISP03_From8499ToStringIs08499()
    {
        // Act
        var result = EmployeeNumber.From(8499).ToString();

        // Assert
        Assert.Equal("08499", result);
    }


    // ========== 観点グループ EQ：等価性（Equals/GetHashCode） ==========

    [Fact]
    public void TestENEQ01_SameValues1234AreEqual()
    {
        // Act
        var num1 = EmployeeNumber.From(1234);
        var num2 = EmployeeNumber.From(1234);

        // Assert
        Assert.Equal(num1, num2);
    }

    [Fact]
    public void TestENEQ02_SelfReferenceIsEqual()
    {
        // Act
        var num = EmployeeNumber.From(1234);

        // Assert
        Assert.Equal(num, num);
    }

    [Fact]
    public void TestENEQ03_DifferentValues1234And5678AreNotEqual()
    {
        // Act
        var num1 = EmployeeNumber.From(1234);
        var num2 = EmployeeNumber.From(5678);

        // Assert
        Assert.NotEqual(num1, num2);
    }

    [Fact]
    public void TestENEQ04_EqualsNullReturnsFalse()
    {
        // Act
        var num = EmployeeNumber.From(1234);
        bool result = num.Equals((EmployeeNumber?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TestENEQ05_GetHashCodeSameForSameValues()
    {
        // Act
        var num1 = EmployeeNumber.From(1234);
        var num2 = EmployeeNumber.From(1234);

        // Assert
        Assert.Equal(num1.GetHashCode(), num2.GetHashCode());
    }

    [Fact]
    public void TestENEQ06_GetHashCodeDifferentForDifferentValues()
    {
        // Act
        var num1 = EmployeeNumber.From(1234);
        var num2 = EmployeeNumber.From(5678);

        // Assert
        Assert.NotEqual(num1.GetHashCode(), num2.GetHashCode());
    }

    [Fact]
    public void TestENEQ07_UsableInDictionary()
    {
        // Act
        var dict = new Dictionary<EmployeeNumber, string>
        {
            { EmployeeNumber.From(1234), "太郎" },
            { EmployeeNumber.From(5678), "花子" }
        };

        // Assert
        Assert.Equal("太郎", dict[EmployeeNumber.From(1234)]);
        Assert.Equal("花子", dict[EmployeeNumber.From(5678)]);
    }

    // ========== 観点グループ IMM：不変性 ==========

    [Fact]
    public void TestENIMM01_ValuePropertyIsReadOnly()
    {
        // Act
        var num = EmployeeNumber.From(1234);

        // Assert - Value は get のみで set がない
        int value = num.Value;
        Assert.Equal(1234, value);
        // set できないので、以下のコードはコンパイルエラー：
        // num.Value = 5678;  // ← コンパイラが拒否
    }

    [Fact]
    public void TestENIMM02_MultipleReferencesShareSameValue()
    {
        // Act
        var num = EmployeeNumber.From(1234);
        var ref1 = num;
        var ref2 = num;

        // Assert
        Assert.Equal(ref1.Value, ref2.Value);
        Assert.Same(ref1, ref2);
    }

    // ========== 観点グループ EDGE：エッジケース ==========

    [Fact]
    public void TestENEDGE01_MinValid1001VsReserved1000()
    {
        // Act & Assert
        var valid = EmployeeNumber.From(1001);  // OK
        Assert.Equal(1001, valid.Value);

        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(1000));  // NG
    }

    [Fact]
    public void TestENEDGE02_MaxValid8499And8500BothValid()
    {
        // Act & Assert
        var valid1 = EmployeeNumber.From(8499);  // OK
        Assert.Equal(8499, valid1.Value);

        var valid2 = EmployeeNumber.From(8500);  // OK（範囲上限削除）
        Assert.Equal(8500, valid2.Value);
    }

    // ========== データドリブンテスト ==========

    [Theory]
    [InlineData(1001, "01001")]
    [InlineData(1234, "01234")]
    [InlineData(8499, "08499")]
    public void TestToStringFormat(int value, string expected)
    {
        // Act
        var number = EmployeeNumber.From(value);

        // Assert
        Assert.Equal(expected, number.ToString());
    }

    [Theory]
    [InlineData(1001, true)]
    [InlineData(1234, true)]
    [InlineData(8499, true)]
    [InlineData(8500, true)]   // 上限削除で有効
    [InlineData(1000, false)]  // 予約
    [InlineData(999, false)]   // 範囲下
    [InlineData(0, false)]     // 0
    [InlineData(-1, false)]    // 負数
    public void TestFromVariousValues(int input, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var result = EmployeeNumber.From(input);
            Assert.Equal(input, result.Value);
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(input));
        }
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(1234)]
    [InlineData(8499)]
    public void TestTryFromValidReturnsTrue(int input)
    {
        // Act
        bool success = EmployeeNumber.TryFrom(input, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(input, result.Value);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(999)]
    public void TestTryFromInvalidReturnsFalse(int input)
    {
        // Act
        bool success = EmployeeNumber.TryFrom(input, out _);

        // Assert
        Assert.False(success);
    }
}

