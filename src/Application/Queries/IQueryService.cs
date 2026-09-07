using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Application.Queries;

/// <summary>
/// ジェネリック Query Service インターフェース
///
/// 【責務】Context間でのドメインモデル（Aggregate）の参照
/// 【用途】複数の Bounded Context がリアルタイムに別の Context のドメインモデル情報を読み取る際に使用
/// 【メリット】
///   - 汎用層が肥大化しない（ジェネリック定義のみ）
///   - スケーラブル（Aggregate追加時も構造不変）
///   - Context独立（各Context が自身の Aggregate を管理）
///   - 依存方向が正しい（Context別 Application → 汎用 Application）
///
/// 【実装パターン】
/// 各 Bounded Context の Application層で、このインターフェースを実装します：
///
/// ```csharp
/// public class EmployeeQueryService : IQueryService<Employee, EmployeeRowId>
/// {
///     private readonly IEmployeeRepository _repository;
///
///     public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
/// }
/// ```
///
/// DI登録：
/// ```csharp
/// services.AddScoped<IQueryService<Employee, EmployeeRowId>, EmployeeQueryService>();
/// ```
///
/// 利用例：
/// ```csharp
/// // CarPreferences Context が Employee 情報を取得
/// public class UpdateCarPreferencesUseCase
/// {
///     private readonly IQueryService<Employee, EmployeeRowId> _employeeQuery;
///
///     public async Task Execute(EmployeeRowId employeeId, CarModelRequest request)
///     {
///         var employee = await _employeeQuery.GetByIdAsync(employeeId);
///         if (employee == null)
///             throw new EmployeeNotFoundException();
///         // ...
///     }
/// }
/// ```
/// </summary>
/// <typeparam name="TAggregate">ドメインモデルの型（IAggregateRoot を実装）</typeparam>
/// <typeparam name="TId">集約ID の型（RowId を継承）</typeparam>
public interface IQueryService<TAggregate, TId>
    where TAggregate : IAggregateRoot
    where TId : notnull
{
    /// <summary>
    /// 集約IDで Aggregate を検索する
    /// </summary>
    /// <param name="id">検索する集約ID</param>
    /// <returns>見つかった Aggregate、または null</returns>
    Task<TAggregate?> GetByIdAsync(TId id);
}
