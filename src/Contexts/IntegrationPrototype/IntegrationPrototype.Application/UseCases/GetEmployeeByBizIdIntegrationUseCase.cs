using SupportAdvance.Application.Queries;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;

/// <summary>
/// BizId で Employee を取得する統合 Use Case（プロトタイプ）
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>汎用 Application層の IEmployeeQueryService 経由で Employee を取得</description></item>
/// <item><description>Context間連携（BC間直接参照なし）のプロトタイプを示す</description></item>
/// </list>
/// <para>【依存関係】</para>
/// <list type="bullet">
/// <item><description>IEmployeeQueryService（汎用層）</description></item>
/// <item><description>Employee Context Application への直接参照なし ✓ アーキテクチャ準拠</description></item>
/// <item><description>Employee Context の型（EmployeeDto）への参照なし ✓ BC間参照なし</description></item>
/// </list>
/// <para>【アーキテクチャ】</para>
/// <list type="bullet">
/// <item><description>IntegrationPrototype → 汎用 Application層（IEmployeeQueryService）</description></item>
/// <item><description>Employee Context が汎用層のインターフェースを実装</description></item>
/// <item><description>BC間の直接参照を完全に回避（型参照も含む）</description></item>
/// </list>
/// </remarks>
public class GetEmployeeByBizIdIntegrationUseCase
{
    private readonly IEmployeeQueryService _employeeQuery;
    private readonly IAppLogging<GetEmployeeByBizIdIntegrationUseCase> _logger;

    /// <summary>
    /// <see cref="GetEmployeeByBizIdIntegrationUseCase"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="employeeQuery">汎用層経由で従業員を検索する問い合わせサービス</param>
    /// <param name="logger">ログの出力先</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public GetEmployeeByBizIdIntegrationUseCase(
        IEmployeeQueryService employeeQuery,
        IAppLogging<GetEmployeeByBizIdIntegrationUseCase> logger)
    {
        _employeeQuery = employeeQuery ?? throw new ArgumentNullException(nameof(employeeQuery));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// BizId で Employee を検索（汎用層経由）
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号、1以上）</param>
    /// <returns>見つかった従業員のクエリ結果（IEmployeeQueryResult）、または null</returns>
    /// <exception cref="ArgumentException">bizId が無効な場合</exception>
    /// <remarks>
    /// <para>【責務】Query Service 経由で IEmployeeQueryResult を取得</para>
    /// <para>【特徴】Employee Context の型に依存しない（汎用層のインターフェースのみ）</para>
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
            // 汎用 Application層の Query Service 経由で Employee を取得
            // BC間直接参照なし、アーキテクチャ準拠
            // 戻り値は IEmployeeQueryResult（汎用層のインターフェース）
            var employeeResult = await _employeeQuery.GetByBizIdAsync(bizId);

            if (employeeResult == null)
            {
                _logger.LogInformation($"[IntegrationPrototype] No employee found for BizId: {bizId}");
                return null;
            }

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
