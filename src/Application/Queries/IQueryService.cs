using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Application.Queries;

/// <summary>
/// ジェネリック Query Service インターフェース
///
/// 【責務】Context間でのドメインモデル（Aggregate）の参照
/// 【用途】複数の Bounded Context がリアルタイムに別の Context のドメインモデル情報を読み取る際に使用
/// 【アーキテクチャ】
///   - TAggregate は各 Context の Application層で定義されたインターフェース（IEmployee など）
///   - Domain Entity は隠蔽され、Application層インターフェース経由でのみ参照
///   - これにより、他 Context が Domain Entity に直接依存することを防止
///
/// 【メリット】
///   - 汎用層が肥大化しない（ジェネリック定義のみ）
///   - スケーラブル（Aggregate追加時も構造不変）
///   - Context独立（各Context が自身の Aggregate を管理）
///   - 依存方向が正しい（Context別 Application → 汎用 Application）
///   - アーキテクチャ準拠（Application層インターフェース経由の参照）
///
/// 【実装パターン】
/// 1. Aggregate の Application層インターフェースを定義：
/// ```csharp
/// public interface IEmployee : IAggregateRoot { }
/// ```
///
/// 2. Domain Entity がインターフェースを実装：
/// ```csharp
/// public sealed class Employee : AggregateRoot<EmployeeRowId>, IEmployee { }
/// ```
///
/// 3. Query Service を実装：
/// ```csharp
/// public class EmployeeQueryService : IQueryService<IEmployee, EmployeeRowId>
/// {
///     private readonly IEmployeeRepository _repository;
///
///     public async Task<IEmployee?> GetByIdAsync(EmployeeRowId id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
/// }
/// ```
///
/// 4. DI登録：
/// ```csharp
/// services.AddScoped<IQueryService<IEmployee, EmployeeRowId>, EmployeeQueryService>();
/// ```
///
/// 5. 利用例（他の Context）：
/// ```csharp
/// // CarPreferences Context が Employee 情報を取得（IEmployee インターフェース経由）
/// public class UpdateCarPreferencesUseCase
/// {
///     private readonly IQueryService<IEmployee, EmployeeRowId> _employeeQuery;
///
///     public async Task Execute(EmployeeRowId employeeId, CarModelRequest request)
///     {
///         var employee = await _employeeQuery.GetByIdAsync(employeeId);
///         if (employee == null)
///             throw new EmployeeNotFoundException();
///         // ... employee は IEmployee インターフェース経由で使用
///     }
/// }
/// ```
/// 重要: 他 Context は Employee Entity を参照せず、IEmployee インターフェース経由でのみアクセス
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
