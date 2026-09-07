using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Domain.DomainEvents;

/// <summary>
/// 従業員が退職したというドメインイベント
///
/// 【発行】Employee.RetireEmployee()
/// 【用途】人事システム連携、アーカイブ、監査ログ
/// 【識別】AggregateRootId（RowId）で識別、ナチュラルキー（Division+Number）で参照
/// </summary>
public sealed class EmployeeRetiredEvent : IDomainEvent
{
    /// <summary>
    /// 集約ルート ID（RowId）
    /// </summary>
    public long AggregateRootId { get; }

    /// <summary>
    /// 部署コード（参考情報）
    /// </summary>
    public string Division { get; }

    /// <summary>
    /// 従業員番号（参考情報）
    /// </summary>
    public string Number { get; }

    /// <summary>
    /// 退職日（JST）
    /// </summary>
    public LocalDateTime RetiredOn { get; }

    /// <summary>
    /// イベント発生時刻（JST）
    /// </summary>
    public LocalDateTime OccurredAt { get; }

    public EmployeeRetiredEvent(
        long aggregateRootId,
        string division,
        string number,
        LocalDateTime retiredOn)
    {
        ArgumentNullException.ThrowIfNull(division);
        ArgumentNullException.ThrowIfNull(number);

        AggregateRootId = aggregateRootId;
        Division = division;
        Number = number;
        RetiredOn = retiredOn;
        OccurredAt = retiredOn;  // 退職日がイベント発生時刻
    }
}
