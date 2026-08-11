namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 新規従業員を作成する Use Case
/// </summary>
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;

    public CreateEmployeeUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を新規作成
    /// </summary>
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 【Step 1】入力検証
        ValidateRequest(request);

        // 【Step 2】ValueObject 生成
        var typeDivision = ConvertToDivision(request.DivisionCode);
        var bizId = EmployeeBizId.From(request.EmployeeNumber);
        var bizCode = EmployeeBizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // 【Step 3】RowId 採番（プレースホルダー：実装では ISequenceProvider を使用）
        var rowId = EmployeeRowId.From(1);  // TODO: ISequenceProvider で採番

        // 【Step 4】Domain Entity 生成
        var employee = Employee.Create(
            rowId,
            typeDivision,
            bizId,
            bizCode,
            personRowId
        );

        // 【Step 5】Repository で永続化
        await _repository.AddAsync(employee);

        // 【Step 6】DTO に変換して返却
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
