using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.DepartmentMembership;

/// <summary>
/// IsPrimary ValueObject の単体テスト
/// 【責務】従業員の主部署フラグを管理
/// </summary>
public class IsPrimaryTests
{
    [Fact]
    public void Constructor_WithTrue_CreatesPrimaryInstance()
    {
        var isPrimary = IsPrimary.From(true);
        Assert.NotNull(isPrimary);
        Assert.True(isPrimary.Value);
    }

    [Fact]
    public void Constructor_WithFalse_CreatesNonPrimaryInstance()
    {
        var isPrimary = IsPrimary.From(false);
        Assert.NotNull(isPrimary);
        Assert.False(isPrimary.Value);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var isPrimary1 = IsPrimary.From(true);
        var isPrimary2 = IsPrimary.From(true);
        var isPrimary3 = IsPrimary.From(false);

        Assert.Equal(isPrimary1, isPrimary2);
        Assert.NotEqual(isPrimary1, isPrimary3);
    }
}
