namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 新規従業員を作成する Use Case
/// </summary>
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly IClock _clock;

    public CreateEmployeeUseCase(IEmployeeRepository repository, IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// 従業員を新規作成
    /// </summary>
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 【Step 1】入力検証
        ValidateRequest(request);

        // 【Step 2】ValueObject 生成
        var division = ConvertToDivision(request.DivisionCode);
        var number = EmployeeNumber.From(request.EmployeeNumber);
        var code = EmployeeCode.From(division, number);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // 【Step 3】Domain Entity 生成
        // Note: Repository が rowId を採番するため、ここでは一時的に 1 を使用
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1),  // Repository で上書きされる
            code,
            personRowId
        );

        // 【Step 4】Repository で永続化
        await _repository.AddAsync(employee);

        // 【Step 5】DTO に変換して返却
        return employee.ToDto();
    }

    private void ValidateRequest(CreateEmployeeRequest request)
    {
        if (request.PersonRowId <= 0)
            throw new ArgumentException("PersonRowId must be > 0", nameof(request.PersonRowId));

        if (!IsValidDivisionCode(request.DivisionCode))
            throw new ArgumentException($"Invalid DivisionCode: {request.DivisionCode}", nameof(request.DivisionCode));

        if (request.EmployeeNumber < 1001 || request.EmployeeNumber > 9999)
            throw new ArgumentException("EmployeeNumber must be between 1001 and 9999", nameof(request.EmployeeNumber));
    }

    private bool IsValidDivisionCode(string? code)
    {
        return code is "M" or "T" or "C";
    }

    private EmployeeDivision ConvertToDivision(string code)
    {
        return code switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division code: {code}")
        };
    }
}
