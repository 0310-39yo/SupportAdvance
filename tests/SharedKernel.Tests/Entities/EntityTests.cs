using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

namespace SupportAdvance.Tests.SharedKernel.Tests.Entities;

/// <summary>
/// Entity<TId> 基底クラスのテスト
/// </summary>
public class EntityTests
{
    private readonly IClock _clock;

    public EntityTests()
    {
        _clock = new SystemClock();
    }

    #region RaiseDomainEvent テスト

    /// <summary>
    /// TC-1.1: 正常系 - イベント発行
    /// </summary>
    [Fact]
    public void RaiseDomainEvent_WithValidEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var entityId = TestId.From(1L);
        var entity = new TestEntity(entityId);

        // Act
        entity.UpdateName("NewName", _clock);  // RaiseDomainEvent() が呼ばれる

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.IsType<TestDomainEvent>(entity.DomainEvents[0]);
        var actualEvent = (TestDomainEvent)entity.DomainEvents[0];
        Assert.Equal("NewName", actualEvent.Message);
    }

    /// <summary>
    /// TC-1.2: 異常系 - null イベント発行
    /// </summary>
    [Fact]
    public void RaiseDomainEvent_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));

        // Act & Assert
        // リフレクションでの呼び出しはTargetInvocationExceptionでラップされる
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        {
            entity.GetType()
                .GetMethod("RaiseDomainEvent",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(entity, new object?[] { null });
        });

        // InnerException が ArgumentNullException であることを確認
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    #endregion

    #region DomainEvents プロパティテスト

    /// <summary>
    /// TC-2.1: 複数イベント発行
    /// </summary>
    [Fact]
    public void DomainEvents_WithMultipleRaisedEvents_ShouldContainAllEvents()
    {
        // Arrange
        var entityId = TestId.From(1L);
        var entity = new TestEntity(entityId);

        // Act - 複数回イベント発行
        entity.UpdateName("Name1", _clock);
        entity.UpdateName("Name2", _clock);
        entity.UpdateName("Name3", _clock);

        // Assert
        Assert.Equal(3, entity.DomainEvents.Count);
        var messages = entity.DomainEvents
            .Cast<TestDomainEvent>()
            .Select(e => e.Message)
            .ToList();
        Assert.Equal(new[] { "Name1", "Name2", "Name3" }, messages);
    }

    /// <summary>
    /// TC-2.2: DomainEvents は読み取り専用
    /// </summary>
    [Fact]
    public void DomainEvents_IsReadOnlyList_CannotModify()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));
        entity.UpdateName("Name", _clock);
        var domainEvents = entity.DomainEvents;

        // Act & Assert
        // IReadOnlyList<T> には Add メソッドがないため、インターフェースのメソッドも Add がない
        var hasAddMethod = domainEvents.GetType()
            .GetMethod("Add", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null;
        Assert.True(hasAddMethod);
    }

    /// <summary>
    /// TC-2.3: イベント発行なし時は空リスト
    /// </summary>
    [Fact]
    public void DomainEvents_WithoutRaisingEvents_ShouldReturnEmptyList()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));

        // Act & Assert
        Assert.Empty(entity.DomainEvents);
        Assert.Equal(0, entity.DomainEvents.Count);
    }

    #endregion

    #region ClearDomainEvents テスト

    /// <summary>
    /// TC-3.1: イベントリストクリア
    /// </summary>
    [Fact]
    public void ClearDomainEvents_AfterEvents_ShouldClearAll()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));
        entity.UpdateName("Name1", _clock);
        entity.UpdateName("Name2", _clock);
        Assert.Equal(2, entity.DomainEvents.Count);

        // Act
        entity.ClearDomainEventsForTesting();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    #endregion

    #region Equals テスト

    /// <summary>
    /// TC-4.1: 同じ ID を持つ Entity は等価
    /// </summary>
    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var testId = TestId.From(1L);
        var entity1 = new TestEntity(testId, "Name1");
        var entity2 = new TestEntity(testId, "Name2");  // 同じ ID、異なる Name

        // Act & Assert
        Assert.True(entity1.Equals(entity2));
        Assert.True(entity2.Equals(entity1));
    }

    /// <summary>
    /// TC-4.2: 異なる ID を持つ Entity は非等価
    /// </summary>
    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(TestId.From(1L));
        var entity2 = new TestEntity(TestId.From(2L));

        // Act & Assert
        Assert.False(entity1.Equals(entity2));
        Assert.False(entity2.Equals(entity1));
    }

    /// <summary>
    /// TC-4.3: null との比較
    /// </summary>
    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));

        // Act & Assert
        Assert.False(entity.Equals(null));
        Assert.False(entity.Equals((Entity<TestId>?)null));
    }

    /// <summary>
    /// TC-4.4: 参照が同じ場合
    /// </summary>
    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(TestId.From(1L));

        // Act & Assert
        Assert.True(entity.Equals(entity));
    }

    #endregion

    #region GetHashCode テスト

    /// <summary>
    /// TC-5.1: 同じ ID なら同じハッシュコード
    /// </summary>
    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var testId = TestId.From(1L);
        var entity1 = new TestEntity(testId);
        var entity2 = new TestEntity(testId);

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// TC-5.2: HashSet で同じ ID を持つ Entity は重複排除
    /// </summary>
    [Fact]
    public void GetHashCode_InHashSet_ShouldDeduplicate()
    {
        // Arrange
        var testId = TestId.From(1L);
        var entity1 = new TestEntity(testId);
        var entity2 = new TestEntity(testId);
        var hashSet = new HashSet<Entity<TestId>>();

        // Act
        hashSet.Add(entity1);
        hashSet.Add(entity2);

        // Assert
        Assert.Single(hashSet);  // 同じ ID なので1件に重複排除
    }

    #endregion
}
