using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.Queries;

/// <summary>
/// 他の Bounded Context からの従業員の読み取りを提供する、Employee 集約の問い合わせサービス
/// </summary>
/// <remarks>
/// <para>【実装するインターフェース】<see cref="IQueryServiceWithBizId{TAggregate, TId}"/>（<see cref="IEmployee"/> を返す）と、
/// <see cref="IEmployeeQueryService"/>（汎用層の <see cref="IEmployeeQueryResult"/> を返す、BC 間参照用）</para>
/// <para>【依存関係】<see cref="IEmployeeRepository"/> のみ</para>
/// </remarks>
public class EmployeeQueryService : IQueryServiceWithBizId<IEmployee, EmployeeRowId>, IEmployeeQueryService
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

    /// <summary>
    /// BizId（ビジネスID）で Employee を検索する（IEmployeeQueryService 実装）
    /// </summary>
    /// <remarks>
    /// <para>【責務】Repository 経由で Employee を取得し、EmployeeDto に変換</para>
    /// <para>【戻り値】IEmployeeQueryResult インターフェース経由で返す（BC間参照用）</para>
    /// <para>【用途】IntegrationPrototype など、他 Context が汎用層インターフェース経由でアクセス</para>
    /// </remarks>
    async Task<IEmployeeQueryResult?> IEmployeeQueryService.GetByBizIdAsync(int bizId)
    {
        if (bizId <= 0)
        {
            throw new ArgumentException("BizId must be greater than 0", nameof(bizId));
        }

        var employee = await _repository.GetByBizIdAsync(bizId);
        return employee == null ? null : Extensions.EmployeeExtensions.ToDto(employee);
    }

    /// <summary>
    /// EmployeeRowId で Employee を検索する（IEmployeeQueryService 実装）
    /// </summary>
    /// <remarks>
    /// <para>【責務】Repository 経由で Employee を取得し、EmployeeDto に変換</para>
    /// <para>【戻り値】IEmployeeQueryResult インターフェース経由で返す（BC間参照用）</para>
    /// </remarks>
    async Task<IEmployeeQueryResult?> IEmployeeQueryService.GetByRowIdAsync(long rowId)
    {
        var employee = await _repository.GetByIdAsync(EmployeeRowId.From(rowId));
        return employee == null ? null : Extensions.EmployeeExtensions.ToDto(employee);
    }
}
