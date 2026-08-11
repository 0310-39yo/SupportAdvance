namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員情報を更新する Use Case
/// </summary>
public class UpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;

    public UpdateEmployeeUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を更新
    /// </summary>
    public async Task<EmployeeDto> ExecuteAsync(UpdateEmployeeRequest request)
    {
        if (request.EmployeeRowId <= 0)
            throw new ArgumentException("Invalid EmployeeRowId", nameof(request.EmployeeRowId));

        var id = EmployeeRowId.From(request.EmployeeRowId);

        // 既存 Entity を取得
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException($"Employee not found: {id}");

        // 更新内容を適用（DivisionCode/EmployeeNumber が指定された場合）
        if (request.DivisionCode != null && request.EmployeeNumber.HasValue)
        {
            if (!IsValidDivisionCode(request.DivisionCode))
                throw new ArgumentException($"Invalid DivisionCode: {request.DivisionCode}", nameof(request.DivisionCode));

            if (request.EmployeeNumber.Value < 1001 || request.EmployeeNumber.Value > 9999)
                throw new ArgumentException("EmployeeNumber must be between 1001 and 9999", nameof(request.EmployeeNumber));

            var typeDivision = ConvertToDivision(request.DivisionCode);
            var bizId = EmployeeBizId.From(request.EmployeeNumber.Value);
            var bizCode = EmployeeBizCode.From(typeDivision, bizId);
            // TODO: employee.ChangeCode(newCode) メソッドを呼び出し
        }

        // 更新を永続化
        await _repository.UpdateAsync(employee);

        return employee.ToDto();
    }

    private bool IsValidDivisionCode(string? code)
    {
        return code is "M" or "T" or "C";
    }

    private EmployeeTypeDivision ConvertToDivision(string code)
    {
        return code switch
        {
            "M" => EmployeeTypeDivision.RegularEmployee(),
            "T" => EmployeeTypeDivision.Dispatched(),
            "C" => EmployeeTypeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division code: {code}")
        };
    }
}
