using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.Queries;

/// <summary>
/// Employee 集約の Query Service 実装
///
/// 【責務】他の Bounded Context からの Employee 読み取り
/// 【用途】Context間でのドメインモデル（Employee）の参照
/// 【アーキテクチャ】
///   - IQueryServiceWithBizId<IEmployee, EmployeeRowId> を実装（Domain層対応）
///   - IEmployeeQueryService を実装（汎用層対応、BC間参照用）
///   - IEmployeeQueryResult を返す（汎用層の型、BC間参照可能）
/// 【依存関係】IEmployeeRepository のみに依存
/// </summary>
public class EmployeeQueryService : IQueryServiceWithBizId<IEmployee, EmployeeRowId>, IEmployeeQueryService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeQueryService(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// EmployeeRowId で Employee を検索する（IQueryService 実装）
    /// 【責務】Repository 経由で Employee Aggregate を取得
    /// 【戻り値】IEmployee インターフェース経由で返す（Domain Entity は隠蔽）
    /// </summary>
    public async Task<IEmployee?> GetByIdAsync(EmployeeRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _repository.GetByIdAsync(id);
    }

    /// <summary>
    /// BizId（ビジネスID）で Employee を検索する（IQueryServiceWithBizId 実装）
    /// 【責務】Repository 経由で Employee Aggregate を取得
    /// 【戻り値】IEmployee インターフェース経由で返す
    /// </summary>
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
    /// 【責務】Repository 経由で Employee を取得し、EmployeeDto に変換
    /// 【戻り値】IEmployeeQueryResult インターフェース経由で返す（BC間参照用）
    /// 【用途】IntegrationPrototype など、他 Context が汎用層インターフェース経由でアクセス
    /// </summary>
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
    /// 【責務】Repository 経由で Employee を取得し、EmployeeDto に変換
    /// 【戻り値】IEmployeeQueryResult インターフェース経由で返す（BC間参照用）
    /// </summary>
    async Task<IEmployeeQueryResult?> IEmployeeQueryService.GetByRowIdAsync(long rowId)
    {
        var employee = await _repository.GetByIdAsync(EmployeeRowId.From(rowId));
        return employee == null ? null : Extensions.EmployeeExtensions.ToDto(employee);
    }
}
