using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーのトランスミッション希望が更新されたイベント
///
/// 【発行】UserPreferences.UpdateTransmissionPreference()
/// 【用途】ログ、ユーザー通知
/// </summary>
public sealed class TransmissionPreferenceUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public RespondentPersonId UserId { get; }

    /// <summary>変更前の値（true: オートマ, false: マニュアル）</summary>
    public bool OldPreference { get; }

    /// <summary>変更後の値</summary>
    public bool NewPreference { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public TransmissionPreferenceUpdatedEvent(
        RespondentPersonId userId,
        bool oldPreference,
        bool newPreference,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(userId);

        UserId = userId;
        OldPreference = oldPreference;
        NewPreference = newPreference;
        OccurredAt = occurredAt;
    }
}
