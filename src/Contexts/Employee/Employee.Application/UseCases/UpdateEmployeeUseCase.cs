using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using Repositories;
using Dtos;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員情報を更新する Use Case
/// </summary>
public class UpdateEmployeeUseCase(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// 従業員を更新
    /// </summary>
    /// <param name="request">更新する従業員の内容</param>
    /// <returns>更新後の従業員の DTO</returns>
    /// <exception cref="ArgumentException">従業員の行ID が 0 以下の場合、区分コードが不正な場合、または従業員番号が 1001〜9999 の範囲外の場合</exception>
    /// <exception cref="InvalidOperationException">従業員が見つからない場合</exception>
    /// <remarks>
    /// <para>【注意】区分コードと従業員番号は検証のみで、従業員への反映は未実装（TODO）</para>
    /// </remarks>
    public async Task<EmployeeDto> ExecuteAsync(UpdateEmployeeRequest request)
    {
        if (request.EmployeeRowId <= 0)
        {
            throw new ArgumentException("Invalid EmployeeRowId", nameof(request.EmployeeRowId));
        }

        var id = EmployeeRowId.From(request.EmployeeRowId);

        // 既存 Entity を取得
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee not found: {id}");
        }

        // 更新内容を適用（DivisionCode/EmployeeNumber が指定された場合）
        if (request.DivisionCode != null && request.EmployeeNumber.HasValue)
        {
            if (!IsValidDivisionCode(request.DivisionCode))
            {
                throw new ArgumentException($"Invalid DivisionCode: {request.DivisionCode}",
                    nameof(request.DivisionCode));
            }

            if (request.EmployeeNumber.Value < 1001 || request.EmployeeNumber.Value > 9999)
            {
                throw new ArgumentException("EmployeeNumber must be between 1001 and 9999",
                    nameof(request.EmployeeNumber));
            }

            var typeDivision = ConvertToDivision(request.DivisionCode);
            var bizId = BizId.From(request.EmployeeNumber.Value);
            var bizCode = BizCode.From(typeDivision, bizId);
            // TODO: employee.ChangeCode(newCode) メソッドを呼び出し
        }

        // 更新を永続化
        await _repository.UpdateAsync(employee);

        return employee.ToDto();
    }

    private bool IsValidDivisionCode(string? code) => code is "M" or "T" or "C";

    private BizDivision ConvertToDivision(string code)
    {
        if (BizDivision.TryFromString(code, out var division))
        {
            return division;
        }

        throw new ArgumentException($"Invalid division code: {code}", nameof(code));
    }
}
