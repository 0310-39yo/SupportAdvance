using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの初期好み設定が作成されたイベント
///
/// 【発行】Application層（CreateUserPreferencesUseCase）
/// 【用途】ウェルカムメール、初期推奨、ログ
/// </summary>
public sealed class PreferencesCreatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public RespondentPersonId UserId { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesCreatedEvent(
        RespondentPersonId userId,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(userId);

        UserId = userId;
        OccurredAt = occurredAt;
    }
}
