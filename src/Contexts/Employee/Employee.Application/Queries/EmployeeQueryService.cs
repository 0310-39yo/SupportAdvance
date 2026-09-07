using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Application.Queries;

/// <summary>
/// Employee 集約の Query Service 実装
///
/// 【責務】他の Bounded Context からの Employee 読み取り
/// 【用途】Context間でのドメインモデル（Employee）の参照
/// 【アーキテクチャ】IEmployee インターフェース経由（Application層）で参照を提供
/// 【依存関係】IEmployeeRepository のみに依存（その他 Context の Application には依存しない）
/// </summary>
public class EmployeeQueryService : IQueryService<IEmployee, EmployeeRowId>
{
    private readonly IEmployeeRepository _repository;

    public EmployeeQueryService(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// EmployeeRowId で Employee を検索する
    /// 【責務】Repository 経由で Employee Aggregate を取得
    /// 【戻り値】IEmployee インターフェース経由で返す（Domain Entity は隠蔽）
    /// </summary>
    public async Task<IEmployee?> GetByIdAsync(EmployeeRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _repository.GetByIdAsync(id);
    }
}
