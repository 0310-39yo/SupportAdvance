using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの予算範囲が更新されたイベント
///
/// 【発行】UserPreferences.UpdateBudget()
/// 【用途】ログ、キャッシュ無効化、検索インデックス更新
/// 【識別子】RowId（システム基本ID、マイナンバー相当）
/// </summary>
public sealed class BudgetUpdatedEvent : IDomainEvent
{
    /// <summary>row_id（システム基本ID）</summary>
    public long RowId { get; }

    /// <summary>変更前の予算下限</summary>
    public Money? OldBudgetFrom { get; }

    /// <summary>変更前の予算上限</summary>
    public Money? OldBudgetTo { get; }

    /// <summary>変更後の予算下限</summary>
    public Money? NewBudgetFrom { get; }

    /// <summary>変更後の予算上限</summary>
    public Money? NewBudgetTo { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public BudgetUpdatedEvent(
        long rowId,
        Money? oldFrom,
        Money? oldTo,
        Money? newFrom,
        Money? newTo,
        LocalDateTime occurredAt)
    {
        RowId = rowId;
        OldBudgetFrom = oldFrom;
        OldBudgetTo = oldTo;
        NewBudgetFrom = newFrom;
        NewBudgetTo = newTo;
        OccurredAt = occurredAt;
    }
}
