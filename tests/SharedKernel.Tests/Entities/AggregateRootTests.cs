using SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

namespace SupportAdvance.Tests.SharedKernel.Tests.Entities;

/// <summary>
/// AggregateRoot<TId> 基底クラスのテスト
/// </summary>
public class AggregateRootTests
{
    #region Entity 継承機能テスト

    /// <summary>
    /// TC-6.1: AggregateRoot は Entity と同じ機能を持つ
    /// </summary>
    [Fact]
    public void AggregateRoot_InheritFromEntity_ShouldHaveSameFunctionality()
    {
        // Arrange
        var id = new TestId("root-001");
        var root = new TestAggregateRoot(id);

        // Act & Assert
        Assert.Equal(id, root.Id);
        Assert.Empty(root.DomainEvents);
    }

    /// <summary>
    /// TC-6.2: AggregateRoot も等価性判定が ID ベース
    /// </summary>
    [Fact]
    public void AggregateRoot_Equals_ShouldUseIdBase()
    {
        // Arrange
        var id = new TestId("root-001");
        var root1 = new TestAggregateRoot(id);
        var root2 = new TestAggregateRoot(id);

        // Act & Assert
        Assert.True(root1.Equals(root2));
    }

    #endregion
}
