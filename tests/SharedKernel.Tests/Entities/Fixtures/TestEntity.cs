using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用 Entity
/// </summary>
public sealed class TestEntity : Entity<TestId>
{
    public string Name { get; private set; }

    public TestEntity(TestId id, string name = "Test")
    {
        Id = id;
        Name = name;
    }

    public void UpdateName(string newName, IClock clock)
    {
        Name = newName;
        this.RaiseDomainEvent(new TestDomainEvent(DomainEventId.New(), this.Id, newName, clock.JstNow));
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
