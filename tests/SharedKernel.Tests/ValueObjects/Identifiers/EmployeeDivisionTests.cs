namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// EmployeeDivision ValueObject の単体テスト
/// </summary>
public class EmployeeDivisionTests
{
    // ========== 観点グループ FM：ファクトリメソッド ==========

    [Fact]
    public void TestEMFM01_RegularEmployeeReturnsM()
    {
        // Act
        var result = EmployeeDivision.RegularEmployee();

        // Assert
        Assert.Equal('M', result.Value);
        Assert.True(result.IsSet);
        Assert.True(result.IsRegularEmployee);
    }

    [Fact]
    public void TestEMFM02_DispatchedReturnsT()
    {
        // Act
        var result = EmployeeDivision.Dispatched();

        // Assert
        Assert.Equal('T', result.Value);
        Assert.True(result.IsSet);
        Assert.True(result.IsDispatched);
    }

    [Fact]
    public void TestEMFM03_ContractorReturnsC()
    {
        // Act
        var result = EmployeeDivision.Contractor();

        // Assert
        Assert.Equal('C', result.Value);
        Assert.True(result.IsSet);
        Assert.True(result.IsContractor);
    }

    [Fact]
    public void TestEMFM04_MultipleCallsCreateNewInstances()
    {
        // Act
        var result1 = EmployeeDivision.RegularEmployee();
        var result2 = EmployeeDivision.RegularEmployee();

        // Assert
        Assert.NotSame(result1, result2);
        Assert.Equal(result1, result2);  // ただし等価
    }

    // ========== 観点グループ GEN：生成メソッド（From/FromDbValue） ==========

    [Fact]
    public void TestEMGEN01_FromMReturnsValidObject()
    {
        // Act
        var result = EmployeeDivision.From('M');

        // Assert
        Assert.Equal('M', result.Value);
        Assert.True(result.IsSet);
    }

    [Fact]
    public void TestEMGEN02_FromTAndCReturnValidObjects()
    {
        // Act
        var resultT = EmployeeDivision.From('T');
        var resultC = EmployeeDivision.From('C');

        // Assert
        Assert.Equal('T', resultT.Value);
        Assert.Equal('C', resultC.Value);
    }

    [Fact]
    public void TestEMGEN03_FromXThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.From('X'));
    }

    [Fact]
    public void TestEMGEN04_FromDbValueMReturnsValidObject()
    {
        // Act
        var result = EmployeeDivision.FromDbValue("M");

        // Assert
        Assert.Equal('M', result.Value);
        Assert.True(result.IsSet);
    }

    [Fact]
    public void TestEMGEN05_FromDbValueEmptyThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EmployeeDivision.FromDbValue(""));
    }

    [Fact]
    public void TestEMGEN06_FromDbValueNullThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EmployeeDivision.FromDbValue(null!));
    }

    [Fact]
    public void TestEMGEN07_From0ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.From('0'));
    }

    [Fact]
    public void TestEMGEN08_FromNumberCharThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        // char('0'～'9') は無効な区分値
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.From('1'));
    }

    [Fact]
    public void TestEMGEN09_FromDbValueTReturnsValid()
    {
        // Act
        var result = EmployeeDivision.FromDbValue("T");

        // Assert
        Assert.Equal('T', result.Value);
    }

    [Fact]
    public void TestEMGEN10_FromDbValueInvalidThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.FromDbValue("X"));
    }

    // ========== 観点グループ TRY：TryFrom/TryFromDbValue ==========

    [Fact]
    public void TestEMTRY01_TryFromValidValueReturnsTrueAndSetsResult()
    {
        // Act
        bool success = EmployeeDivision.TryFrom('M', out var result);

        // Assert
        Assert.True(success);
        Assert.Equal('M', result.Value);
    }

    [Fact]
    public void TestEMTRY02_TryFromNullReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFrom(null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestEMTRY03_TryFromInvalidValueReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFrom('X', out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestEMTRY04_TryFromDbValueValidReturnsTrueAndSetsResult()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue("M", out var result);

        // Assert
        Assert.True(success);
        Assert.Equal('M', result.Value);
    }

    [Fact]
    public void TestEMTRY05_TryFromDbValueInvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue("X", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestEMTRY06_TryFromDbValueNullReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue(null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestEMTRY07_TryFromMultipleValuesReturnsTrueForValid()
    {
        // Act & Assert
        Assert.True(EmployeeDivision.TryFrom('M', out _));
        Assert.True(EmployeeDivision.TryFrom('T', out _));
        Assert.True(EmployeeDivision.TryFrom('C', out _));
    }

    [Fact]
    public void TestEMTRY08_TryFromDbValueTReturnsTrue()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue("T", out var result);

        // Assert
        Assert.True(success);
        Assert.Equal('T', result.Value);
    }

    [Fact]
    public void TestEMTRY09_TryFromDbValueInvalidReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue("X", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestEMTRY10_TryFromDbValueEmptyReturnsFalse()
    {
        // Act
        bool success = EmployeeDivision.TryFromDbValue("", out _);

        // Assert
        Assert.False(success);
    }

    // ========== 観点グループ JUDGE：判定プロパティ ==========

    [Fact]
    public void TestEMJUDGE01_IsRegularEmployeeTrueForM()
    {
        // Act
        var division = EmployeeDivision.RegularEmployee();

        // Assert
        Assert.True(division.IsRegularEmployee);
        Assert.False(division.IsDispatched);
        Assert.False(division.IsContractor);
    }

    [Fact]
    public void TestEMJUDGE03_IsDispatchedTrueForT()
    {
        // Act
        var division = EmployeeDivision.Dispatched();

        // Assert
        Assert.False(division.IsRegularEmployee);
        Assert.True(division.IsDispatched);
        Assert.False(division.IsContractor);
    }

    [Fact]
    public void TestEMJUDGE05_IsContractorTrueForC()
    {
        // Act
        var division = EmployeeDivision.Contractor();

        // Assert
        Assert.False(division.IsRegularEmployee);
        Assert.False(division.IsDispatched);
        Assert.True(division.IsContractor);
    }

    [Fact]
    public void TestEMJUDGE02_IsRegularEmployeeFalseForT()
    {
        // Act
        var division = EmployeeDivision.Dispatched();

        // Assert
        Assert.False(division.IsRegularEmployee);
    }

    [Fact]
    public void TestEMJUDGE04_IsDispatchedFalseForM()
    {
        // Act
        var division = EmployeeDivision.RegularEmployee();

        // Assert
        Assert.False(division.IsDispatched);
    }

    [Fact]
    public void TestEMJUDGE06_IsContractorFalseForM()
    {
        // Act
        var division = EmployeeDivision.RegularEmployee();

        // Assert
        Assert.False(division.IsContractor);
    }

    // ========== 観点グループ DISP：表示名（ToString） ==========

    [Fact]
    public void TestEMDISP01_RegularEmployeeDisplayNameIsJapanese()
    {
        // Act
        var result = EmployeeDivision.RegularEmployee().ToString();

        // Assert
        Assert.Equal("従業員", result);
    }

    [Fact]
    public void TestEMDISP02_DispatchedDisplayNameIsJapanese()
    {
        // Act
        var result = EmployeeDivision.Dispatched().ToString();

        // Assert
        Assert.Equal("派遣社員", result);
    }

    [Fact]
    public void TestEMDISP03_ContractorDisplayNameIsJapanese()
    {
        // Act
        var result = EmployeeDivision.Contractor().ToString();

        // Assert
        Assert.Equal("請負者", result);
    }

    // ========== 観点グループ EQ：等価性（Equals/GetHashCode） ==========

    [Fact]
    public void TestEMEQ01_SameValuesAreEqual()
    {
        // Act
        var div1 = EmployeeDivision.From('M');
        var div2 = EmployeeDivision.From('M');

        // Assert
        Assert.Equal(div1, div2);
    }

    [Fact]
    public void TestEMEQ02_SelfReferenceIsEqual()
    {
        // Act
        var div = EmployeeDivision.From('M');

        // Assert
        Assert.Equal(div, div);
    }

    [Fact]
    public void TestEMEQ03_DifferentValuesAreNotEqual()
    {
        // Act
        var divM = EmployeeDivision.From('M');
        var divT = EmployeeDivision.From('T');

        // Assert
        Assert.NotEqual(divM, divT);
    }

    [Fact]
    public void TestEMEQ04_EqualsNullReturnsFalse()
    {
        // Act
        var div = EmployeeDivision.From('M');
        bool result = div.Equals((EmployeeDivision?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TestEMEQ05_GetHashCodeSameForSameValues()
    {
        // Act
        var div1 = EmployeeDivision.From('M');
        var div2 = EmployeeDivision.From('M');

        // Assert
        Assert.Equal(div1.GetHashCode(), div2.GetHashCode());
    }

    [Fact]
    public void TestEMEQ06_GetHashCodeDifferentForDifferentValues()
    {
        // Act
        var divM = EmployeeDivision.From('M');
        var divT = EmployeeDivision.From('T');

        // Assert
        Assert.NotEqual(divM.GetHashCode(), divT.GetHashCode());
    }

    [Fact]
    public void TestEMEQ07_UsableInDictionary()
    {
        // Act
        var dict = new Dictionary<EmployeeDivision, string>
        {
            { EmployeeDivision.RegularEmployee(), "従業員" },
            { EmployeeDivision.Dispatched(), "派遣社員" },
            { EmployeeDivision.Contractor(), "請負者" }
        };

        // Assert
        Assert.Equal("従業員", dict[EmployeeDivision.From('M')]);
        Assert.Equal("派遣社員", dict[EmployeeDivision.From('T')]);
        Assert.Equal("請負者", dict[EmployeeDivision.From('C')]);
    }

    // ========== 観点グループ IMM：不変性 ==========

    [Fact]
    public void TestEMIMM01_ValuePropertyIsReadOnly()
    {
        // Act
        var div = EmployeeDivision.From('M');

        // Assert - Value は get のみで set がないことを確認
        var value = div.Value;
        Assert.Equal('M', value);
        // set できないので、以下のコードはコンパイルエラー：
        // div.Value = 'T';  // ← コンパイラが拒否
    }

    [Fact]
    public void TestEMIMM02_MultipleReferencesShareSameValue()
    {
        // Act
        var div = EmployeeDivision.From('M');
        var ref1 = div;
        var ref2 = div;

        // Assert
        Assert.Equal(ref1.Value, ref2.Value);
        Assert.Same(ref1, ref2);  // 同じインスタンスを参照
    }

    // ========== 観点グループ CONST：定数 ==========

    [Fact]
    public void TestEMCONST01_RegularEmployeeValueIsM()
    {
        // Assert
        Assert.Equal('M', EmployeeDivision.RegularEmployeeValue);
    }

    [Fact]
    public void TestEMCONST02_DispatchedValueIsT()
    {
        // Assert
        Assert.Equal('T', EmployeeDivision.DispatchedValue);
    }

    [Fact]
    public void TestEMCONST03_ContractorValueIsC()
    {
        // Assert
        Assert.Equal('C', EmployeeDivision.ContractorValue);
    }

    // ========== データドリブンテスト ==========

    [Theory]
    [InlineData('M', "従業員", true)]
    [InlineData('T', "派遣社員", true)]
    [InlineData('C', "請負者", true)]
    public void TestFromVariousValidValues(char input, string expectedDisplay, bool shouldSucceed)
    {
        // Act & Assert
        if (shouldSucceed)
        {
            var result = EmployeeDivision.From(input);
            Assert.Equal(expectedDisplay, result.ToString());
        }
    }

    [Theory]
    [InlineData('X')]
    [InlineData('0')]
    [InlineData('Z')]
    [InlineData('m')]  // 小文字は不可
    public void TestFromInvalidValuesThrowException(char input)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.From(input));
    }
}
