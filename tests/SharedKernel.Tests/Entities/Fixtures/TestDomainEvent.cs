using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用ドメインイベント
/// </summary>
public sealed class TestDomainEvent : IDomainEvent
{
    public TestId EntityId { get; }
    public string Message { get; }
    public LocalDateTime OccurredAt { get; }

    public TestDomainEvent(TestId entityId, string message, LocalDateTime occurredAt)
    {
        EntityId = entityId;
        Message = message;
        OccurredAt = occurredAt;
    }
}
