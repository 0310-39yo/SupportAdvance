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

    /// <summary>
    /// <see cref="CreateEmployeeUseCase"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="repository">作成した従業員集約の保存先</param>
    /// <param name="clock">現在日時（JST）の取得元</param>
    /// <param name="sequenceProvider">行ID の採番元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public CreateEmployeeUseCase(IEmployeeRepository repository, IClock clock, ISequenceProvider sequenceProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
    }

    /// <summary>
    /// 従業員を新規作成
    /// </summary>
    /// <param name="request">作成する従業員の内容</param>
    /// <returns>作成した従業員の DTO（部署への所属なし）</returns>
    /// <exception cref="ArgumentException">人物行ID が 0 以下の場合、区分コードが不正な場合、従業員番号が 1001〜9999 の範囲外の場合、区分と番号の組み合わせが範囲外の場合、または氏名が空・100 文字超の場合</exception>
    /// <remarks>
    /// <para>【副作用】行ID を採番し、従業員を DB に追加</para>
    /// </remarks>
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
            new List<DepartmentMembership>(),
            _clock
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
