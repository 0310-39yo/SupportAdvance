namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// EmployeeCode ValueObject の単体テスト
/// </summary>
public class EmployeeCodeTests
{
    // ========== 観点グループ GEN：生成メソッド（From/TryFrom） ==========

    [Fact]
    public void TestECGEN01_FromM1234ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));

        // Assert
        Assert.Equal("M1234", result.ToString());
    }

    [Fact]
    public void TestECGEN02_FromT7500ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(7500));

        // Assert
        Assert.Equal("T7500", result.ToString());
    }

    [Fact]
    public void TestECGEN03_FromC8000ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(8000));

        // Assert
        Assert.Equal("C8000", result.ToString());
    }

    [Fact]
    public void TestECGEN04_FromM10000ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(10000));

        // Assert
        Assert.Equal("M10000", result.ToString());
    }

    [Fact]
    public void TestECGEN05_FromT70000ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(70000));

        // Assert
        Assert.Equal("T70000", result.ToString());
    }

    [Fact]
    public void TestECGEN06_FromC80000ReturnsValidCode()
    {
        // Act
        var result = EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(80000));

        // Assert
        Assert.Equal("C80000", result.ToString());
    }

    [Fact]
    public void TestECGEN07_FromM7500InvalidThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(7500)));
    }

    [Fact]
    public void TestECGEN08_FromT1234InvalidThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(1234)));
    }

    [Fact]
    public void TestECGEN09_FromC7500InvalidThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(7500)));
    }


    // ========== 観点グループ TRY：TryFrom ==========

    [Fact]
    public void TestECTRY01_TryFromM1234ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234),
            out var result);

        // Assert
        Assert.True(success);
        Assert.Equal("M1234", result.ToString());
    }

    [Fact]
    public void TestECTRY02_TryFromT7500ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(7500),
            out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECTRY03_TryFromC8000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Contractor(),
            EmployeeNumber.From(8000),
            out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECTRY04_TryFromM7500InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(7500),
            out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECTRY05_TryFromT1234InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(1234),
            out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECTRY06_TryFromC7500InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Contractor(),
            EmployeeNumber.From(7500),
            out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECTRY08_TryFromM8000InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(8000),
            out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECTRY09_TryFromT70000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(70000),
            out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECTRY10_TryFromC80000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.Contractor(),
            EmployeeNumber.From(80000),
            out _);

        // Assert
        Assert.True(success);
    }

    // ========== 観点グループ PARSE：文字列パース（TryParse） ==========

    [Fact]
    public void TestECPARSE01_TryParseM1234ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryParse("M1234", out var result);

        // Assert
        Assert.True(success);
        Assert.Equal("M1234", result.ToString());
    }

    [Fact]
    public void TestECPARSE02_TryParseT7500ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryParse("T7500", out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECPARSE03_TryParseC8000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryParse("C8000", out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECPARSE04_TryParseM10000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryParse("M10000", out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECPARSE05_TryParseX1234InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse("X1234", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECPARSE06_TryParseM999InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse("M999", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECPARSE07_TryParseT1234InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse("T1234", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECPARSE08_TryParseNullReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse(null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECPARSE09_TryParseEmptyReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse("", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECPARSE10_TryParseNoDistributionReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryParse("1234", out _);

        // Assert
        Assert.False(success);
    }

    // ========== 観点グループ DBVAL：DB値変換（TryFromDbValues） ==========

    [Fact]
    public void TestECDBVAL01_TryFromDbValuesM1234ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('M', 1234, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal("M1234", result.ToString());
    }

    [Fact]
    public void TestECDBVAL02_TryFromDbValuesT7500ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('T', 7500, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECDBVAL03_TryFromDbValuesC8000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('C', 8000, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECDBVAL04_TryFromDbValuesM7500InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('M', 7500, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECDBVAL05_TryFromDbValuesT1234InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('T', 1234, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECDBVAL06_TryFromDbValuesX1234InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('X', 1234, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECDBVAL07_TryFromDbValuesM999InvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('M', 999, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestECDBVAL08_TryFromDbValuesM10000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('M', 10000, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECDBVAL09_TryFromDbValuesT70000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('T', 70000, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TestECDBVAL10_TryFromDbValuesC80000ReturnsTrue()
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues('C', 80000, out _);

        // Assert
        Assert.True(success);
    }

    // ========== 観点グループ DISP：表示形式（ToString） ==========

    [Fact]
    public void TestECDISP01_ToStringM1234ReturnsM1234()
    {
        // Act
        var result = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234)).ToString();

        // Assert
        Assert.Equal("M1234", result);
    }

    [Fact]
    public void TestECDISP02_ToStringT7500ReturnsT7500()
    {
        // Act
        var result = EmployeeCode.From(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(7500)).ToString();

        // Assert
        Assert.Equal("T7500", result);
    }

    [Fact]
    public void TestECDISP03_ToStringC8000ReturnsC8000()
    {
        // Act
        var result = EmployeeCode.From(
            EmployeeDivision.Contractor(),
            EmployeeNumber.From(8000)).ToString();

        // Assert
        Assert.Equal("C8000", result);
    }

    [Fact]
    public void TestECDISP04_ToStringM10000ReturnsM10000()
    {
        // Act
        var result = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(10000)).ToString();

        // Assert
        Assert.Equal("M10000", result);
    }

    [Fact]
    public void TestECDISP05_ToStringM1001ReturnsM1001()
    {
        // Act
        var result = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1001)).ToString();

        // Assert
        Assert.Equal("M1001", result);
    }

    // ========== 観点グループ EQ：等価性（Equals/GetHashCode） ==========

    [Fact]
    public void TestECEQ01_SameValuesAreEqual()
    {
        // Act
        var code1 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));
        var code2 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));

        // Assert
        Assert.Equal(code1, code2);
    }

    [Fact]
    public void TestECEQ02_SelfReferenceIsEqual()
    {
        // Act
        var code = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));

        // Assert
        Assert.Equal(code, code);
    }

    [Fact]
    public void TestECEQ03_DifferentValuesAreNotEqual()
    {
        // Act
        var code1 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));
        var code2 = EmployeeCode.From(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(7500));

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void TestECEQ04_EqualsNullReturnsFalse()
    {
        // Act
        var code = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));
        bool result = code.Equals((EmployeeCode?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TestECEQ05_GetHashCodeSameForSameValues()
    {
        // Act
        var code1 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));
        var code2 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));

        // Assert
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }

    [Fact]
    public void TestECEQ06_GetHashCodeDifferentForDifferentValues()
    {
        // Act
        var code1 = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));
        var code2 = EmployeeCode.From(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(7500));

        // Assert
        Assert.NotEqual(code1.GetHashCode(), code2.GetHashCode());
    }

    [Fact]
    public void TestECEQ07_UsableInDictionary()
    {
        // Act
        var dict = new Dictionary<EmployeeCode, string>
        {
            { EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)), "太郎" },
            { EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(7500)), "花子" }
        };

        // Assert
        Assert.Equal("太郎", dict[EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234))]);
        Assert.Equal("花子", dict[EmployeeCode.From(
            EmployeeDivision.Dispatched(),
            EmployeeNumber.From(7500))]);
    }

    // ========== 観点グループ PROP：プロパティ ==========

    [Fact]
    public void TestECPROP01_DivisionReturnsCorrectValue()
    {
        // Act
        var code = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));

        // Assert
        Assert.True(code.Division.IsRegularEmployee);
    }

    [Fact]
    public void TestECPROP02_NumberReturnsCorrectValue()
    {
        // Act
        var code = EmployeeCode.From(
            EmployeeDivision.RegularEmployee(),
            EmployeeNumber.From(1234));

        // Assert
        Assert.Equal(1234, code.Number.Value);
    }

    // ========== 観点グループ RANGE：範囲検証（詳細） ==========

    [Theory]
    [InlineData('M', 1001, true)]      // M下限
    [InlineData('M', 1000, false)]     // M下限-1
    [InlineData('M', 6999, true)]      // M初期上限
    [InlineData('M', 7000, false)]     // M初期上限+1
    [InlineData('M', 9999, false)]     // Mギャップ
    [InlineData('M', 10000, true)]     // M使い切り後
    [InlineData('T', 7500, true)]      // T下限
    [InlineData('T', 7499, false)]     // T下限-1
    [InlineData('T', 7999, true)]      // T初期上限
    [InlineData('T', 8000, false)]     // T初期上限+1（請負範囲）
    [InlineData('T', 69999, false)]    // Tギャップ
    [InlineData('T', 70000, true)]     // T使い切り後
    [InlineData('C', 8000, true)]      // C下限
    [InlineData('C', 7999, false)]     // C下限-1
    [InlineData('C', 8499, true)]      // C初期上限
    [InlineData('C', 8500, false)]     // C初期上限+1
    [InlineData('C', 79999, false)]    // Cギャップ
    [InlineData('C', 80000, true)]     // C使い切り後
    public void TestRangeValidation(char divChar, int number, bool shouldSucceed)
    {
        // Act
        bool success = EmployeeCode.TryFromDbValues(divChar, number, out _);

        // Assert
        Assert.Equal(shouldSucceed, success);
    }
}
