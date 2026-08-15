namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using Dtos;
using Repositories;

/// <summary>
/// BizId（従業員番号）で従業員を取得する Use Case
/// </summary>
public class GetEmployeeByBizIdUseCase(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// 従業員を BizId で検索
    /// </summary>
    /// <param name="bizId">ビジネスID（従業員番号、1001以上）</param>
    /// <returns>見つかった従業員の DTO、または null</returns>
    /// <exception cref="ArgumentException">bizId が無効な場合</exception>
    public async Task<EmployeeDto?> ExecuteAsync(int bizId)
    {
        if (bizId <= 0)
        {
            throw new ArgumentException("BizId must be greater than 0", nameof(bizId));
        }

        var employee = await _repository.GetByBizIdAsync(bizId);

        return employee == null ? null : Extensions.EmployeeExtensions.ToDto(employee);
    }
}
