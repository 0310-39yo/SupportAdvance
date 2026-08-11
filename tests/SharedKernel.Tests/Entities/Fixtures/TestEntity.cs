using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用 Entity
/// </summary>
public sealed class TestEntity : Entity<TestId>
{
    public string Name { get; private set; }

    public TestEntity(TestId id, string name = "Test")
    {
        RowId = id;
        Name = name;
    }

    public void UpdateName(string newName, IClock clock)
    {
        Name = newName;
        // AggregateRootId（RowId）は 1 にハードコード（テスト用）
        this.RaiseDomainEvent(new TestDomainEvent(1L, this.RowId, newName, clock.JstNow));
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => this.DomainEvents;

    public void ClearDomainEventsForTesting()
    {
        // リフレクションで internal ClearDomainEvents() を呼び出す
        this.GetType()
            .BaseType?
            .GetMethod("ClearDomainEvents",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(this, Array.Empty<object>());
    }
}

