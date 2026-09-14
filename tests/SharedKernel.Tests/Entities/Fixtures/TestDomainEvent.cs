using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用ドメインイベント
/// </summary>
public sealed class TestDomainEvent : IDomainEvent
{
    public long AggregateRootId { get; }
    public TestId EntityId { get; }
    public string Message { get; }
    public LocalDateTime OccurredAt { get; }

    public TestDomainEvent(long aggregateRootId, TestId entityId, string message, LocalDateTime occurredAt)
    {
        AggregateRootId = aggregateRootId;
        EntityId = entityId;
        Message = message;
        OccurredAt = occurredAt;
    }
}

