using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.Queries;

/// <summary>
/// 他の Bounded Context からの従業員の読み取りを提供する、Employee 集約の問い合わせサービス
/// </summary>
/// <remarks>
/// <para>【実装するインターフェース】<see cref="IQueryServiceWithBizId{TAggregate, TId}"/>（<see cref="IEmployee"/> を返す、標準パターン）</para>
/// <para>【依存関係】<see cref="IEmployeeRepository"/> のみ</para>
/// <para>【アーキテクチャ】ジェネリック Query Service パターンに統一。他 Context が Entity → DTO 変換を責務とする</para>
/// </remarks>
public class EmployeeQueryService : IQueryServiceWithBizId<IEmployee, EmployeeRowId>
{
    private readonly IEmployeeRepository _repository;

    /// <summary>
    /// <see cref="EmployeeQueryService"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="repository">従業員集約の取得元</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> が <see langword="null"/> の場合</exception>
    public EmployeeQueryService(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// EmployeeRowId で Employee を検索する（IQueryService 実装）
    /// </summary>
    /// <param name="id">検索する従業員の行ID</param>
    /// <returns>見つかった従業員。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【責務】Repository 経由で Employee Aggregate を取得</para>
    /// <para>【戻り値】IEmployee インターフェース経由で返す（Domain Entity は隠蔽）</para>
    /// </remarks>
    public async Task<IEmployee?> GetByIdAsync(EmployeeRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _repository.GetByIdAsync(id);
    }

    /// <summary>
    /// BizId（ビジネスID）で Employee を検索する（IQueryServiceWithBizId 実装）
    /// </summary>
    /// <param name="bizId">検索する従業員番号（1 以上）</param>
    /// <returns>見つかった従業員。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentException"><paramref name="bizId"/> が 0 以下の場合</exception>
    /// <remarks>
    /// <para>【責務】Repository 経由で Employee Aggregate を取得</para>
    /// <para>【戻り値】IEmployee インターフェース経由で返す</para>
    /// </remarks>
    public async Task<IEmployee?> GetByBizIdAsync(int bizId)
    {
        if (bizId <= 0)
        {
            throw new ArgumentException("BizId must be greater than 0", nameof(bizId));
        }
        return await _repository.GetByBizIdAsync(bizId);
    }

}
