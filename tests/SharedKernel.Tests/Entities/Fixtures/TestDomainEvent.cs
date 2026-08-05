using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用ドメインイベント
/// </summary>
public sealed class TestDomainEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public TestId EntityId { get; }
    public string Message { get; }
    public LocalDateTime OccurredAt { get; }

    public TestDomainEvent(DomainEventId eventId, TestId entityId, string message, LocalDateTime occurredAt)
    {
        EventId = eventId;
        EntityId = entityId;
        Message = message;
        OccurredAt = occurredAt;
    }
}
