namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// HierarchyLevel（階層レベル） 単体テスト
///
/// 選択肢型 ValueObject パターン：0-4 の限定値のみ許可
/// DB スキーマ: m_departments.hierarchy_level [int] (0-4)
///
/// テスト観点：
/// - VO-01: From(0-4) は IsSet=true のインスタンスを生成
/// - VO-02: From(範囲外) は ArgumentOutOfRangeException を投げる
/// - VO-03: GetDisplayName で業務名を返す
/// - VO-06: 等価性判定
/// </summary>
public class HierarchyLevelTests
{
    /// <summary>From(0) は Company レベルを生成</summary>
    [Fact]
    public void From_With0_ReturnsCompanyLevel()
    {
        // Act
        var level = HierarchyLevel.From(0);

        // Assert
        Assert.NotNull(level);
        Assert.True(level.IsSet);
        Assert.Equal(0, level.Value);
        Assert.Equal("Company", level.ToString());
    }

    /// <summary>From(1) は Division レベルを生成</summary>
    [Fact]
    public void From_With1_ReturnsDivisionLevel()
    {
        // Act
        var level = HierarchyLevel.From(1);

        // Assert
        Assert.Equal(1, level.Value);
        Assert.Equal("Division", level.ToString());
    }

    /// <summary>From(2) は Department レベルを生成</summary>
    [Fact]
    public void From_With2_ReturnsDepartmentLevel()
    {
        // Act
        var level = HierarchyLevel.From(2);

        // Assert
        Assert.Equal(2, level.Value);
        Assert.Equal("Department", level.ToString());
    }

    /// <summary>From(3) は Group レベルを生成</summary>
    [Fact]
    public void From_With3_ReturnsGroupLevel()
    {
        // Act
        var level = HierarchyLevel.From(3);

        // Assert
        Assert.Equal(3, level.Value);
        Assert.Equal("Group", level.ToString());
    }

    /// <summary>From(4) は Team レベルを生成</summary>
    [Fact]
    public void From_With4_ReturnsTeamLevel()
    {
        // Act
        var level = HierarchyLevel.From(4);

        // Assert
        Assert.Equal(4, level.Value);
        Assert.Equal("Team", level.ToString());
    }

    /// <summary>From(-1) は ArgumentOutOfRangeException を投げる</summary>
    [Fact]
    public void From_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(-1));
    }

    /// <summary>From(5) は ArgumentOutOfRangeException を投げる</summary>
    [Fact]
    public void From_With5_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(5));
    }

    /// <summary>TryFrom(0) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_With0_ReturnsTrue()
    {
        // Act
        var result = HierarchyLevel.TryFrom(0, out var level);

        // Assert
        Assert.True(result);
        Assert.NotNull(level);
        Assert.Equal(0, level.Value);
    }

    /// <summary>TryFrom(4) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_With4_ReturnsTrue()
    {
        // Act
        var result = HierarchyLevel.TryFrom(4, out var level);

        // Assert
        Assert.True(result);
        Assert.Equal(4, level.Value);
    }

    /// <summary>TryFrom(-1) は false を返す</summary>
    [Fact]
    public void TryFrom_WithNegative_ReturnsFalse()
    {
        // Act
        var result = HierarchyLevel.TryFrom(-1, out var level);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(5) は false を返す</summary>
    [Fact]
    public void TryFrom_With5_ReturnsFalse()
    {
        // Act
        var result = HierarchyLevel.TryFrom(5, out var level);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFromDbValue(0) は true を返す</summary>
    [Fact]
    public void TryFromDbValue_With0_ReturnsTrue()
    {
        // Act
        var result = HierarchyLevel.TryFromDbValue(0, out var level);

        // Assert
        Assert.True(result);
        Assert.Equal(0, level.Value);
    }

    /// <summary>TryFromDbValue(-1) は false を返す</summary>
    [Fact]
    public void TryFromDbValue_WithNegative_ReturnsFalse()
    {
        // Act
        var result = HierarchyLevel.TryFromDbValue(-1, out var level);

        // Assert
        Assert.False(result);
    }

    /// <summary>Company() ファクトリメソッドは Company レベルを生成</summary>
    [Fact]
    public void Company_ReturnsLevel0()
    {
        // Act
        var level = HierarchyLevel.Company();

        // Assert
        Assert.Equal(0, level.Value);
        Assert.Equal("Company", level.ToString());
    }

    /// <summary>Division() ファクトリメソッドは Division レベルを生成</summary>
    [Fact]
    public void Division_ReturnsLevel1()
    {
        // Act
        var level = HierarchyLevel.Division();

        // Assert
        Assert.Equal(1, level.Value);
        Assert.Equal("Division", level.ToString());
    }

    /// <summary>Department() ファクトリメソッドは Department レベルを生成</summary>
    [Fact]
    public void Department_ReturnsLevel2()
    {
        // Act
        var level = HierarchyLevel.Department();

        // Assert
        Assert.Equal(2, level.Value);
        Assert.Equal("Department", level.ToString());
    }

    /// <summary>Group() ファクトリメソッドは Group レベルを生成</summary>
    [Fact]
    public void Group_ReturnsLevel3()
    {
        // Act
        var level = HierarchyLevel.Group();

        // Assert
        Assert.Equal(3, level.Value);
        Assert.Equal("Group", level.ToString());
    }

    /// <summary>Team() ファクトリメソッドは Team レベルを生成</summary>
    [Fact]
    public void Team_ReturnsLevel4()
    {
        // Act
        var level = HierarchyLevel.Team();

        // Assert
        Assert.Equal(4, level.Value);
        Assert.Equal("Team", level.ToString());
    }

    /// <summary>同じレベル同士は等価</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var level1 = HierarchyLevel.From(0);
        var level2 = HierarchyLevel.From(0);

        // Act & Assert
        Assert.Equal(level1, level2);
        Assert.True(level1 == level2);
    }

    /// <summary>異なるレベルは非等価</summary>
    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var level1 = HierarchyLevel.From(0);
        var level2 = HierarchyLevel.From(1);

        // Act & Assert
        Assert.NotEqual(level1, level2);
        Assert.False(level1 == level2);
    }

    /// <summary>演算子：同じレベルの == は true</summary>
    [Fact]
    public void OperatorEqual_SameValues_ReturnsTrue()
    {
        // Arrange
        var level1 = HierarchyLevel.Company();
        var level2 = HierarchyLevel.From(0);

        // Act & Assert
        Assert.True(level1 == level2);
    }

    /// <summary>演算子：異なるレベルの != は true</summary>
    [Fact]
    public void OperatorNotEqual_DifferentValues_ReturnsTrue()
    {
        // Arrange
        var level1 = HierarchyLevel.Company();
        var level2 = HierarchyLevel.Division();

        // Act & Assert
        Assert.True(level1 != level2);
    }
}



