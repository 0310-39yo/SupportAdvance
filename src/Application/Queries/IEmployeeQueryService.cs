namespace SupportAdvance.Application.Queries;

/// <summary>
/// 他の Bounded Context から従業員を読み取るための、Employee 専用の問い合わせサービス（汎用層）
/// </summary>
/// <remarks>
/// <para>【設計】汎用層での定義による、BC 間の直接参照の回避。戻り値は Domain の型ではなく <see cref="IEmployeeQueryResult"/></para>
/// <para>【実装】Employee Context の <c>EmployeeQueryService</c></para>
/// </remarks>
/// <example>
/// <code>
/// public class GetEmployeeByBizIdIntegrationUseCase
/// {
///     private readonly IEmployeeQueryService _employeeQuery;
///
///     public async Task&lt;IEmployeeQueryResult?&gt; ExecuteAsync(int bizId)
///         =&gt; await _employeeQuery.GetByBizIdAsync(bizId);
/// }
/// </code>
/// </example>
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
