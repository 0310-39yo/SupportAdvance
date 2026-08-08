using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// ユーザーのトランスミッション希望が更新されたイベント
///
/// 【発行】UserPreferences.UpdateTransmissionPreference()
/// 【用途】ログ、ユーザー通知
/// 【識別子】EventId（GUID ValueObject）
/// </summary>
public sealed class TransmissionPreferenceUpdatedEvent : IDomainEvent
{
    /// <summary>イベント一意識別子（GUID ValueObject）</summary>
    public DomainEventId EventId { get; }

    /// <summary>変更前の値（true: オートマ, false: マニュアル）</summary>
    public bool OldPreference { get; }

    /// <summary>変更後の値</summary>
    public bool NewPreference { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public TransmissionPreferenceUpdatedEvent(
        DomainEventId eventId,
        bool oldPreference,
        bool newPreference,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        EventId = eventId;
        OldPreference = oldPreference;
        NewPreference = newPreference;
        OccurredAt = occurredAt;
    }
}


