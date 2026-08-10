namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// ManagerEmployeeRowId（管理者従業員行ID） 単体テスト
///
/// オプション ValueObject パターン：null は Unset に変換して成功を返す
/// DB スキーマ: m_departments.manager_employee_row_id [bigint] NULL
/// </summary>
public class ManagerEmployeeRowIdTests
{
    [Theory]
    [InlineData(1L)]
    [InlineData(9999L)]
    public void From_WithValidValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        var managerRowId = ManagerEmployeeRowId.From(validValue);
        Assert.True(managerRowId.IsSet);
        Assert.Equal(validValue, managerRowId.Value);
    }

    [Fact]
    public void From_WithZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ManagerEmployeeRowId.From(0L));
    }

    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ManagerEmployeeRowId.From(-1L));
    }

    [Fact]
    public void Unset_ReturnsInstanceWithIsSetFalse()
    {
        var unset = ManagerEmployeeRowId.Unset();
        Assert.False(unset.IsSet);
        Assert.Null(unset.Value);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance()
    {
        var result = ManagerEmployeeRowId.TryFrom(1L, out var managerRowId);
        Assert.True(result);
        Assert.True(managerRowId.IsSet);
        Assert.Equal(1L, managerRowId.Value);
    }

    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        var result = ManagerEmployeeRowId.TryFrom(null, out var managerRowId);
        Assert.True(result);
        Assert.False(managerRowId.IsSet);
        Assert.Equal(ManagerEmployeeRowId.Unset(), managerRowId);
    }

    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        var result = ManagerEmployeeRowId.TryFrom(0L, out var managerRowId);
        Assert.False(result);
    }

    [Fact]
    public void TryFrom_WithNegativeValue_ReturnsFalse()
    {
        var result = ManagerEmployeeRowId.TryFrom(-1L, out var managerRowId);
        Assert.False(result);
    }

    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        var result = ManagerEmployeeRowId.TryFromDbValue(1L, out var managerRowId);
        Assert.True(result);
        Assert.True(managerRowId.IsSet);
    }

    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrueAndUnset()
    {
        var result = ManagerEmployeeRowId.TryFromDbValue(null, out var managerRowId);
        Assert.True(result);
        Assert.False(managerRowId.IsSet);
    }

    [Fact]
    public void HasManager_WithValue_ReturnsTrue()
    {
        var managerRowId = ManagerEmployeeRowId.From(1L);
        Assert.True(managerRowId.HasManager);
    }

    [Fact]
    public void HasManager_WithUnset_ReturnsFalse()
    {
        var unset = ManagerEmployeeRowId.Unset();
        Assert.False(unset.HasManager);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var managerRowId1 = ManagerEmployeeRowId.From(1L);
        var managerRowId2 = ManagerEmployeeRowId.From(1L);
        Assert.Equal(managerRowId1, managerRowId2);
        Assert.True(managerRowId1 == managerRowId2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var managerRowId1 = ManagerEmployeeRowId.From(1L);
        var managerRowId2 = ManagerEmployeeRowId.From(2L);
        Assert.NotEqual(managerRowId1, managerRowId2);
    }

    [Fact]
    public void Equals_BothUnset_ReturnsTrue()
    {
        var unset1 = ManagerEmployeeRowId.Unset();
        var unset2 = ManagerEmployeeRowId.Unset();
        Assert.Equal(unset1, unset2);
    }

    [Fact]
    public void Equals_DifferentIsSet_ReturnsFalse()
    {
        var managerRowId = ManagerEmployeeRowId.From(1L);
        var unset = ManagerEmployeeRowId.Unset();
        Assert.NotEqual(managerRowId, unset);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var managerRowId1 = ManagerEmployeeRowId.From(1L);
        var managerRowId2 = ManagerEmployeeRowId.From(1L);
        Assert.Equal(managerRowId1.GetHashCode(), managerRowId2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentIsSet_DifferentHash()
    {
        var managerRowId = ManagerEmployeeRowId.From(1L);
        var unset = ManagerEmployeeRowId.Unset();
        Assert.NotEqual(managerRowId.GetHashCode(), unset.GetHashCode());
    }

    [Fact]
    public void ToString_WithValue_ReturnsStringRepresentation()
    {
        var managerRowId = ManagerEmployeeRowId.From(1L);
        Assert.Equal("1", managerRowId.ToString());
    }

    [Fact]
    public void ToString_WithUnset_ReturnsUnset()
    {
        var unset = ManagerEmployeeRowId.Unset();
        Assert.Equal("Unset", unset.ToString());
    }
}


