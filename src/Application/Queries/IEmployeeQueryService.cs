namespace SupportAdvance.Application.Queries;

/// <summary>
/// Employee 専用 Query Service インターフェース（汎用層）
///
/// 【責務】Employee Context の Query Service 契約を定義
/// 【用途】他の Bounded Context が Employee を取得する際に使用
/// 【特徴】汎用層に定義により、BC間の直接参照を回避
///
/// 【実装】Employee Context の EmployeeQueryService が実装
///
/// 【利用例】
/// ```csharp
/// public class GetEmployeeByBizIdIntegrationUseCase
/// {
///     private readonly IEmployeeQueryService _employeeQuery;
///
///     public async Task<IEmployeeQueryResult?> ExecuteAsync(int bizId)
///     {
///         return await _employeeQuery.GetByBizIdAsync(bizId);
///     }
/// }
/// ```
/// </summary>
public interface IEmployeeQueryService
{
    /// <summary>
    /// BizId（ビジネスID）で Employee を検索する
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号、1以上）</param>
    /// <returns>見つかった Employee のクエリ結果、または null</returns>
    Task<IEmployeeQueryResult?> GetByBizIdAsync(int bizId);

    /// <summary>
    /// EmployeeRowId で Employee を検索する
    /// </summary>
    /// <param name="rowId">Employee RowId（DB行ID）</param>
    /// <returns>見つかった Employee のクエリ結果、または null</returns>
    Task<IEmployeeQueryResult?> GetByRowIdAsync(long rowId);
}
