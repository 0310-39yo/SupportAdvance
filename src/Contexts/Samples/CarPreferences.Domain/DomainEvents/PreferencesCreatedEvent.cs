using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの初期好み設定が作成されたイベント
///
/// 【発行】Application層（CreateUserPreferencesUseCase）
/// 【用途】ウェルカムメール、初期推奨、ログ
/// 【識別子】EventId（GUID ValueObject）
/// </summary>
public sealed class PreferencesCreatedEvent : IDomainEvent
{
    /// <summary>イベント一意識別子（GUID ValueObject）</summary>
    public DomainEventId EventId { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesCreatedEvent(
        DomainEventId eventId,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        EventId = eventId;
        OccurredAt = occurredAt;
    }
}
