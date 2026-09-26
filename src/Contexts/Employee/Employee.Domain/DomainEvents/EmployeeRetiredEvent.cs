using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Domain.DomainEvents;

/// <summary>
/// 従業員が退職したというドメインイベント
/// </summary>
/// <remarks>
/// <para>【発行】Employee.RetireEmployee()</para>
/// <para>【用途】人事システム連携、アーカイブ、監査ログ</para>
/// <para>【識別】AggregateRootId（RowId）で識別、ナチュラルキー（Division+Number）で参照</para>
/// </remarks>
public sealed class EmployeeRetiredEvent : IDomainEvent
{
    /// <summary>
    /// 集約ルート ID（RowId）
    /// </summary>
    public long AggregateRootId { get; }

    /// <summary>
    /// 従業員種別区分（参考情報）
    /// </summary>
    /// <value><c>Employee.TypeDivision</c>（<c>BizDivision</c>）の文字列表現。部署コードではない値</value>
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

    /// <summary>
    /// <see cref="EmployeeRetiredEvent"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="aggregateRootId">退職した従業員の行ID（<c>EmployeeRowId</c> の値）</param>
    /// <param name="division">従業員種別区分の文字列表現</param>
    /// <param name="number">従業員番号の文字列表現</param>
    /// <param name="retiredOn">退職日（JST）</param>
    /// <exception cref="ArgumentNullException"><paramref name="division"/> または <paramref name="number"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【注意】<see cref="OccurredAt"/> にはイベントの生成時刻ではなく <paramref name="retiredOn"/> を設定</para>
    /// </remarks>
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
        OccurredAt = retiredOn; // 退職日がイベント発生時刻
    }
}
