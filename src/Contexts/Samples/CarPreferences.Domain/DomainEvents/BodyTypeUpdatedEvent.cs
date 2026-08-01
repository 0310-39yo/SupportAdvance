using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの希望ボディタイプが更新されたイベント
///
/// 【発行】UserPreferences.UpdateBodyType()
/// 【用途】ログ、検索インデックス更新
/// </summary>
public sealed class BodyTypeUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public RespondentPersonId UserId { get; }

    /// <summary>変更前のボディタイプ</summary>
    public BodyType? OldBodyType { get; }

    /// <summary>変更後のボディタイプ</summary>
    public BodyType NewBodyType { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public BodyTypeUpdatedEvent(
        RespondentPersonId userId,
        BodyType? oldBodyType,
        BodyType newBodyType,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newBodyType);

        UserId = userId;
        OldBodyType = oldBodyType;
        NewBodyType = newBodyType;
        OccurredAt = occurredAt;
    }
}
