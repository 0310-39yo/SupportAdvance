using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Extensions;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;

/// <summary>
/// BizId で Employee を取得する統合 Use Case（プロトタイプ）
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ジェネリック Query Service（IQueryServiceWithBizId）経由で Employee Entity を取得</description></item>
/// <item><description>Entity を IEmployeeQueryResult（DTO）に変換して返却</description></item>
/// <item><description>Context間連携（BC間直接参照なし）のプロトタイプを示す</description></item>
/// </list>
/// <para>【依存関係】</para>
/// <list type="bullet">
/// <item><description>IQueryServiceWithBizId&lt;IEmployee, EmployeeRowId&gt;（汎用層）— Entity を返す標準パターン</description></item>
/// <item><description>IEmployee, EmployeeRowId（SharedKernel）— BC間で共有可能な型</description></item>
/// <item><description>EmployeeExtensions.ToDto（Employee Context の拡張メソッド）— Entity → DTO 変換</description></item>
/// </list>
/// <para>【アーキテクチャ】</para>
/// <list type="bullet">
/// <item><description>IntegrationPrototype → ジェネリック Query Service（IQueryServiceWithBizId、汎用層）</description></item>
/// <item><description>Entity → DTO 変換は IntegrationPrototype が責務を持つ</description></item>
/// </list>
/// </remarks>
public class GetEmployeeByBizIdIntegrationUseCase
{
    private readonly IQueryServiceWithBizId<IEmployee, EmployeeRowId> _employeeQuery;
    private readonly IAppLogging<GetEmployeeByBizIdIntegrationUseCase> _logger;

    /// <summary>
    /// <see cref="GetEmployeeByBizIdIntegrationUseCase"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="employeeQuery">ジェネリック Query Service（汎用層）</param>
    /// <param name="logger">ログの出力先</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public GetEmployeeByBizIdIntegrationUseCase(
        IQueryServiceWithBizId<IEmployee, EmployeeRowId> employeeQuery,
        IAppLogging<GetEmployeeByBizIdIntegrationUseCase> logger)
    {
        _employeeQuery = employeeQuery ?? throw new ArgumentNullException(nameof(employeeQuery));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// BizId で Employee を検索（ジェネリック Query Service 経由）
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号、1以上）</param>
    /// <returns>見つかった従業員のクエリ結果（IEmployeeQueryResult DTO）、または null</returns>
    /// <exception cref="ArgumentException">bizId が無効な場合</exception>
    /// <remarks>
    /// <para>【処理フロー】</para>
    /// <list type="number">
    /// <item><description>ジェネリック Query Service で Entity を取得</description></item>
    /// <item><description>Entity を IEmployeeQueryResult（DTO）に変換</description></item>
    /// <item><description>DTO を返却（BC間での型隠蔽）</description></item>
    /// </list>
    /// </remarks>
    public async Task<IEmployeeQueryResult?> ExecuteAsync(int bizId)
    {
        if (bizId <= 0)
        {
            throw new ArgumentException("BizId must be greater than 0", nameof(bizId));
        }

        _logger.LogInformation($"[IntegrationPrototype] Querying Employee by BizId: {bizId}");

        try
        {
            // ジェネリック Query Service 経由で Employee Entity を取得
            var employee = await _employeeQuery.GetByBizIdAsync(bizId);

            if (employee == null)
            {
                _logger.LogInformation($"[IntegrationPrototype] No employee found for BizId: {bizId}");
                return null;
            }

            // Entity を DTO に変換して返却（BC間での型隠蔽）
            // IEmployee インターフェース経由で受け取った Entity を具体型にキャストして拡張メソッドを呼び出す
            var employeeEntity = (SupportAdvance.Contexts.Employee.Domain.Entities.Employee)employee;
            var employeeResult = employeeEntity.ToDto();

            _logger.LogInformation(
                $"[IntegrationPrototype] Employee found: {employeeResult.PersonLastName} {employeeResult.PersonFirstName}, BizId: {bizId}");
            return employeeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[IntegrationPrototype] Failed to query Employee by BizId: {bizId}", ex);
            throw;
        }
    }
}
