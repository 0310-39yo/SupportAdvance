using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Application.Queries;

/// <summary>
/// 集約を ID で読み取る、Context 間共通の問い合わせサービスの抽象
/// </summary>
/// <typeparam name="TAggregate">読み取る集約。Domain の Entity ではなく、各 Context の Application 層で公開するインターフェース（例: <c>IEmployee</c>）</typeparam>
/// <typeparam name="TId">集約ID の型（<c>RowId</c> の派生型）</typeparam>
/// <remarks>
/// <para>【用途】他の Bounded Context の集約の最新状態の同期的な読み取り。変更の通知にはドメインイベントを使用</para>
/// <para>【設計】Domain の Entity は隠蔽し、Application 層のインターフェース経由でのみ公開。他 Context が Domain の Entity に直接依存することの防止</para>
/// <para>【メリット】汎用層の肥大化なし（ジェネリック定義のみ）。集約の追加時も構造不変。依存方向は Context別 Application → 汎用 Application</para>
/// <para>【重要】他 Context からは Entity を参照せず、公開インターフェース（<c>IEmployee</c> など）経由でのみアクセス</para>
/// <para>【参照】CLAUDE.md「Context間のデータ共有パターン」</para>
/// </remarks>
/// <example>
/// 実装と DI 登録
/// <code>
/// public class EmployeeQueryService : IQueryService&lt;IEmployee, EmployeeRowId&gt;
/// {
///     private readonly IEmployeeRepository _repository;
///
///     public async Task&lt;IEmployee?&gt; GetByIdAsync(EmployeeRowId id)
///         =&gt; await _repository.GetByIdAsync(id);
/// }
///
/// services.AddScoped&lt;IQueryService&lt;IEmployee, EmployeeRowId&gt;, EmployeeQueryService&gt;();
/// </code>
/// 他の Context からの利用
/// <code>
/// public class UpdateCarPreferencesUseCase
/// {
///     private readonly IQueryService&lt;IEmployee, EmployeeRowId&gt; _employeeQuery;
///
///     public async Task Execute(EmployeeRowId employeeId, CarModelRequest request)
///     {
///         var employee = await _employeeQuery.GetByIdAsync(employeeId)
///             ?? throw new EmployeeNotFoundException();
///     }
/// }
/// </code>
/// </example>
public interface IQueryService<TAggregate, TId>
    where TAggregate : IAggregateRoot
    where TId : notnull
{
    /// <summary>
    /// 集約IDで Aggregate の検索
    /// </summary>
    /// <param name="id">検索する集約ID</param>
    /// <returns>見つかった Aggregate、または null</returns>
    Task<TAggregate?> GetByIdAsync(TId id);
}
