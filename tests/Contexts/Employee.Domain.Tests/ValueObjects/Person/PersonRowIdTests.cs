using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Person;

/// <summary>
/// PersonRowId ValueObject の単体テスト
/// 【責務】個人基本情報の DB 行 ID を管理
/// </summary>
public class PersonRowIdTests
{
    [Fact]
    public void Constructor_WithValidId_CreatesPersonRowId()
    {
        var personRowId = PersonRowId.From(1);
        Assert.NotNull(personRowId);
        Assert.Equal(1, personRowId.Value);
    }

    [Fact]
    public void Constructor_WithLargeId_CreatesPersonRowId()
    {
        var personRowId = PersonRowId.From(999999);
        Assert.NotNull(personRowId);
        Assert.Equal(999999, personRowId.Value);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var rowId1 = PersonRowId.From(1);
        var rowId2 = PersonRowId.From(1);
        var rowId3 = PersonRowId.From(2);

        Assert.Equal(rowId1, rowId2);
        Assert.NotEqual(rowId1, rowId3);
    }
}
