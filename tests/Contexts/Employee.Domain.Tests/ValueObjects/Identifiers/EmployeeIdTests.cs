namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// EmployeeId ValueObject の単体テスト
/// </summary>
public class EmployeeIdTests
{
    #region グループ 1: 生成メソッド（NewId）

    [Fact]
    public void TestEIDGEN01_NewIdReturnsValidId()
    {
        var result = EmployeeId.NewId();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public void TestEIDGEN02_NewIdGeneratesUniqueIds()
    {
        var id1 = EmployeeId.NewId();
        var id2 = EmployeeId.NewId();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TestEIDGEN03_NewIdGeneratesTenUniqueIds()
    {
        var ids = Enumerable.Range(0, 10)
            .Select(_ => EmployeeId.NewId())
            .ToList();
        var distinctCount = ids.Distinct().Count();
        Assert.Equal(10, distinctCount);
    }

    #endregion

    #region グループ 2: 生成メソッド（From）

    [Fact]
    public void TestEIDFROM01_FromValidGuidReturnsValidId()
    {
        var guid = Guid.NewGuid();
        var result = EmployeeId.From(guid);
        Assert.NotNull(result);
        Assert.Equal(guid, result.Value);
    }

    [Fact]
    public void TestEIDFROM02_FromKnownGuidReturnsValidId()
    {
        var knownGuid = new Guid("12345678-1234-1234-1234-123456789012");
        var result = EmployeeId.From(knownGuid);
        Assert.Equal(knownGuid, result.Value);
    }

    [Fact]
    public void TestEIDFROM03_FromEmptyGuidThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmployeeId.From(Guid.Empty));
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    public void TestEIDFROMDataDriven_FromValidGuidsReturnsValidIds(string guidString)
    {
        var guid = Guid.Parse(guidString);
        var result = EmployeeId.From(guid);
        Assert.Equal(guid, result.Value);
    }

    #endregion

    #region グループ 3: 安全な生成（TryFrom）

    [Fact]
    public void TestEIDTRY01_TryFromValidGuidReturnsTrue()
    {
        var guid = Guid.NewGuid();
        bool success = EmployeeId.TryFrom(guid, out var result);
        Assert.True(success);
        Assert.Equal(guid, result.Value);
    }

    [Fact]
    public void TestEIDTRY02_TryFromEmptyGuidReturnsFalse()
    {
        bool success = EmployeeId.TryFrom(Guid.Empty, out var result);
        Assert.False(success);
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void TestEIDTRYDataDriven_TryFromValidGuidsReturnsTrue(string guidString)
    {
        var guid = Guid.Parse(guidString);
        bool success = EmployeeId.TryFrom(guid, out var result);
        Assert.True(success);
        Assert.Equal(guid, result.Value);
    }

    #endregion

    #region グループ 4: DB値変換（TryFromDbValue）

    [Fact]
    public void TestEIDDBVAL01_TryFromDbValueReturnsTrue()
    {
        var guid = Guid.NewGuid();
        bool success = EmployeeId.TryFromDbValue(guid, out var result);
        Assert.True(success);
        Assert.Equal(guid, result.Value);
    }

    [Fact]
    public void TestEIDDBVAL02_TryFromDbValueEmptyReturnsFalse()
    {
        bool success = EmployeeId.TryFromDbValue(Guid.Empty, out var result);
        Assert.False(success);
    }

    [Fact]
    public void TestEIDDBVAL03_TryFromDbValueKnownGuidReturnsTrue()
    {
        var knownGuid = new Guid("12345678-1234-1234-1234-123456789012");
        bool success = EmployeeId.TryFromDbValue(knownGuid, out var result);
        Assert.True(success);
        Assert.Equal(knownGuid, result.Value);
    }

    #endregion

    #region グループ 5: 等価性（Equality）

    [Fact]
    public void TestEIDEQ01_SameValuesAreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = EmployeeId.From(guid);
        var id2 = EmployeeId.From(guid);
        Assert.True(id1.Equals(id2));
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void TestEIDEQ02_DifferentValuesAreNotEqual()
    {
        var id1 = EmployeeId.NewId();
        var id2 = EmployeeId.NewId();
        Assert.False(id1.Equals(id2));
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TestEIDEQ03_ObjectEqualsReturnsTrue()
    {
        var guid = Guid.NewGuid();
        var id1 = EmployeeId.From(guid);
        object id2 = EmployeeId.From(guid);
        Assert.True(id1.Equals(id2));
    }

    [Fact]
    public void TestEIDEQ04_HashCodesAreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = EmployeeId.From(guid);
        var id2 = EmployeeId.From(guid);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void TestEIDEQ05_CanBeUsedAsDictionaryKey()
    {
        var dict = new Dictionary<EmployeeId, string>();
        var guid = Guid.NewGuid();
        var id1 = EmployeeId.From(guid);
        var id2 = EmployeeId.From(guid);
        dict.Add(id1, "Employee123");
        dict[id2] = "Employee124";
        Assert.Single(dict);
        Assert.Equal("Employee124", dict[id1]);
    }

    [Fact]
    public void TestEIDEQ06_EqualsNullReturnsFalse()
    {
        var id = EmployeeId.NewId();
        Assert.False(id.Equals(null));
    }

    [Fact]
    public void TestEIDEQ07_EqualsDifferentTypeReturnsFalse()
    {
        var id = EmployeeId.NewId();
        Assert.False(id.Equals(id.Value));
    }

    #endregion

    #region グループ 6: 表示形式（Display）

    [Fact]
    public void TestEIDDISP01_ToStringReturnsGuidString()
    {
        var guid = new Guid("12345678-1234-1234-1234-123456789012");
        var id = EmployeeId.From(guid);
        string result = id.ToString();
        Assert.Equal("12345678-1234-1234-1234-123456789012", result);
    }

    [Fact]
    public void TestEIDDISP02_ToStringForNewId()
    {
        var id = EmployeeId.NewId();
        string result = id.ToString();
        // NewId で生成された Guid が文字列化される
        Assert.NotEmpty(result);
        // Guid の標準形式: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", result.ToLower());
    }

    #endregion

    #region グループ 7: 不変性（Immutability）

    [Fact]
    public void TestEIDIMM01_ValueIsReadOnly()
    {
        var guid = Guid.NewGuid();
        var id = EmployeeId.From(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void TestEIDIMM02_ValueNeverChanges()
    {
        var id = EmployeeId.NewId();
        var firstRead = id.Value;
        var secondRead = id.Value;
        Assert.Equal(firstRead, secondRead);
    }

    #endregion
}

