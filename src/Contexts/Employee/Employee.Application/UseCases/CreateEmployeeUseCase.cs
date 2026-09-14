using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Common.Clocks;

/// <summary>
/// 新規従業員を作成する Use Case
/// </summary>
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly IClock _clock;
    private readonly ISequenceProvider _sequenceProvider;

    public CreateEmployeeUseCase(IEmployeeRepository repository, IClock clock, ISequenceProvider sequenceProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
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
        var bizId = BizId.From(request.EmployeeNumber);
        var bizCode = BizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // 【Step 3】Person Entity 生成
        var personLastName = LastName.From(request.PersonLastName);
        var personFirstName = FirstName.From(request.PersonFirstName);
        var personLastNameKana = LastNameKana.From(request.PersonLastNameKana);
        var personFirstNameKana = FirstNameKana.From(request.PersonFirstNameKana);
        var person = Person.Create(personRowId, personLastName, personFirstName, personLastNameKana, personFirstNameKana);

        // 【Step 4】RowId 採番（DB シーケンスから採番）
        var sequenceValue = await _sequenceProvider.GetNextValueAsync();
        var rowId = EmployeeRowId.From(sequenceValue);

        // 【Step 5】Domain Entity 生成
        var employee = Employee.Create(
            rowId,
            typeDivision,
            bizId,
            bizCode,
            null,
            person,
            new List<DepartmentMembership>()
        );

        // 【Step 6】Repository で永続化
        await _repository.AddAsync(employee);

        // 【Step 7】DTO に変換して返却
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

    private BizDivision ConvertToDivision(string code)
    {
        if (BizDivision.TryFromString(code, out var division))
            return division;

        throw new ArgumentException($"Invalid division code: {code}", nameof(code));
    }
}
