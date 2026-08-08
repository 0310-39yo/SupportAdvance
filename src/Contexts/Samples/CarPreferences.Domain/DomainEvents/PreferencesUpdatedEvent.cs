using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの好み設定が更新されたイベント
///
/// 【発行】UserPreferences.UpdatePreferredModel()
/// 【用途】ログ、監査ログ、ユーザー通知
/// 【識別子】EventId（GUID ValueObject）
/// </summary>
public sealed class PreferencesUpdatedEvent : IDomainEvent
{
    /// <summary>イベント一意識別子（GUID ValueObject）</summary>
    public DomainEventId EventId { get; }

    /// <summary>変更タイプ</summary>
    public PreferenceChangeType ChangeType { get; }

    /// <summary>変更前の値</summary>
    public string OldValue { get; }

    /// <summary>変更後の値</summary>
    public string NewValue { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesUpdatedEvent(
        DomainEventId eventId,
        PreferenceChangeType changeType,
        string oldValue,
        string newValue,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(changeType);
        ArgumentNullException.ThrowIfNull(oldValue);
        ArgumentNullException.ThrowIfNull(newValue);

        EventId = eventId;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// ユーザーが選択した好みの変更タイプ
/// </summary>
public enum PreferenceChangeType
{
    /// <summary>希望車種が変更</summary>
    ModelUpdated = 1,

    /// <summary>予算が変更</summary>
    BudgetUpdated = 2,

    /// <summary>ボディタイプが変更</summary>
    BodyTypeUpdated = 3,

    /// <summary>トランスミッション希望が変更</summary>
    TransmissionUpdated = 4
}
