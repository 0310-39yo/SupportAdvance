using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの希望ボディタイプが更新されたイベント
///
/// 【発行】UserPreferences.UpdateBodyType()
/// 【用途】ログ、検索インデックス更新
/// 【識別子】RowId（システム基本ID、マイナンバー相当）
/// </summary>
public sealed class BodyTypeUpdatedEvent : IDomainEvent
{
    /// <summary>row_id（システム基本ID）</summary>
    public long RowId { get; }

    /// <summary>変更前のボディタイプ</summary>
    public BodyType? OldBodyType { get; }

    /// <summary>変更後のボディタイプ</summary>
    public BodyType NewBodyType { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public BodyTypeUpdatedEvent(
        long rowId,
        BodyType? oldBodyType,
        BodyType newBodyType,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(newBodyType);

        RowId = rowId;
        OldBodyType = oldBodyType;
        NewBodyType = newBodyType;
        OccurredAt = occurredAt;
    }
}
