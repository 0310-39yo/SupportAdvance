namespace SupportAdvance.Contexts.Employee.Application.Dtos;

using SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// Employee → EmployeeDto マッピング
/// </summary>
public static class EmployeeDtoMapper
{
    /// <summary>
    /// Domain Entity を DTO に変換
    /// </summary>
    /// <param name="employee">変換する従業員</param>
    /// <returns>変換した DTO</returns>
    /// <remarks>
    /// <para>【注意】<c>DepartmentNames</c> は未設定。部署名が必要な場合は <c>EmployeeExtensions.ToDto</c> を使用</para>
    /// </remarks>
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            RowId = employee.RowId.Value,
            TypeDivision = employee.TypeDivision.ToString(),
            BizId = employee.BizId.ToString(),
            BizCode = employee.BizCode.ToString(),
            PersonRowId = employee.Person.RowId.Value,
            PersonLastName = employee.Person.LastName.Value,
            PersonFirstName = employee.Person.FirstName.Value,
            PersonLastNameKana = employee.Person.LastNameKana.Value,
            PersonFirstNameKana = employee.Person.FirstNameKana.Value
        };
    }
}
