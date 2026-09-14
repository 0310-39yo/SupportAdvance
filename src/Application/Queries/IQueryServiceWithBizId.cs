using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Application.Queries;

/// <summary>
/// BizId（ビジネスID）検索対応の拡張 Query Service インターフェース
///
/// 【責務】IQueryService<> を拡張し、BizId でのクエリもサポート
/// 【用途】Employee など、BizId（従業員番号）での検索が必要な Aggregate
/// 【継承】IQueryService<TAggregate, TId> を継承（GetByIdAsync は親インターフェースから）
///
/// 【実装パターン】
/// ```csharp
/// public class EmployeeQueryService : IQueryServiceWithBizId<IEmployee, EmployeeRowId>
/// {
///     // GetByIdAsync（IQueryService から）
///     public async Task<IEmployee?> GetByIdAsync(EmployeeRowId id) { ... }
///
///     // GetByBizIdAsync（このインターフェースで追加）
///     public async Task<IEmployee?> GetByBizIdAsync(int bizId) { ... }
/// }
/// ```
///
/// 【DI登録】
/// ```csharp
/// services.AddScoped<IQueryServiceWithBizId<IEmployee, EmployeeRowId>, EmployeeQueryService>();
/// ```
///
/// 【利用例】
/// ```csharp
/// public class GetEmployeeByBizIdIntegrationUseCase
/// {
///     private readonly IQueryServiceWithBizId<IEmployee, EmployeeRowId> _employeeQuery;
///
///     public async Task<IEmployee?> ExecuteAsync(int bizId)
///     {
///         return await _employeeQuery.GetByBizIdAsync(bizId);
///     }
/// }
/// ```
/// </summary>
/// <typeparam name="TAggregate">ドメインモデルの型（IAggregateRoot を実装）</typeparam>
/// <typeparam name="TId">集約ID の型（RowId を継承）</typeparam>
public interface IQueryServiceWithBizId<TAggregate, TId> : IQueryService<TAggregate, TId>
    where TAggregate : IAggregateRoot
    where TId : notnull
{
    /// <summary>
    /// BizId（ビジネスID）で Aggregate を検索する
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号など、1以上）</param>
    /// <returns>見つかった Aggregate、または null</returns>
    Task<TAggregate?> GetByBizIdAsync(int bizId);
}
