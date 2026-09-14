namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;

public class HierarchyLevelTests
{
    [Fact]
    public void From_ValidLevel_ReturnsInstance()
    {
        var level = HierarchyLevel.From(2);
        Assert.Equal(2, level.Value);
    }

    [Fact]
    public void From_InvalidLevel_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void From_ValidLevels_ReturnsInstance(int level)
    {
        var hierarchyLevel = HierarchyLevel.From(level);
        Assert.Equal(level, hierarchyLevel.Value);
    }

    [Fact]
    public void Company_ReturnsLevel0()
    {
        var level = HierarchyLevel.Company();
        Assert.Equal(0, level.Value);
    }

    [Fact]
    public void Division_ReturnsLevel1()
    {
        var level = HierarchyLevel.Division();
        Assert.Equal(1, level.Value);
    }

    [Fact]
    public void Department_ReturnsLevel2()
    {
        var level = HierarchyLevel.Department();
        Assert.Equal(2, level.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var level1 = HierarchyLevel.From(2);
        var level2 = HierarchyLevel.From(2);
        Assert.Equal(level1, level2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var level1 = HierarchyLevel.From(2);
        var level2 = HierarchyLevel.From(3);
        Assert.NotEqual(level1, level2);
    }
}
