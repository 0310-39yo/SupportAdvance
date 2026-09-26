using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Application.Queries;

/// <summary>
/// BizId（ビジネスID）での検索を加えた、<see cref="IQueryService{TAggregate, TId}"/> の拡張
/// </summary>
/// <typeparam name="TAggregate">読み取る集約。各 Context の Application 層で公開するインターフェース（例: <c>IEmployee</c>）</typeparam>
/// <typeparam name="TId">集約ID の型（<c>RowId</c> の派生型）</typeparam>
/// <remarks>
/// <para>【用途】Employee など、BizId（従業員番号）での検索が必要な集約</para>
/// <para>【継承】<see cref="IQueryService{TAggregate, TId}.GetByIdAsync"/> は親インターフェースから継承</para>
/// </remarks>
/// <example>
/// <code>
/// services.AddScoped&lt;IQueryServiceWithBizId&lt;IEmployee, EmployeeRowId&gt;, EmployeeQueryService&gt;();
///
/// public class GetEmployeeByBizIdIntegrationUseCase
/// {
///     private readonly IQueryServiceWithBizId&lt;IEmployee, EmployeeRowId&gt; _employeeQuery;
///
///     public async Task&lt;IEmployee?&gt; ExecuteAsync(int bizId)
///         =&gt; await _employeeQuery.GetByBizIdAsync(bizId);
/// }
/// </code>
/// </example>
public interface IQueryServiceWithBizId<TAggregate, TId> : IQueryService<TAggregate, TId>
    where TAggregate : IAggregateRoot
    where TId : notnull
{
    /// <summary>
    /// BizId（ビジネスID）で Aggregate の検索
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号など、1以上）</param>
    /// <returns>見つかった Aggregate、または null</returns>
    Task<TAggregate?> GetByBizIdAsync(int bizId);
}
