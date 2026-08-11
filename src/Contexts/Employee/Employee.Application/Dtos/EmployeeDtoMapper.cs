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
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            RowId = employee.RowId.Value,
            TypeDivision = employee.TypeDivision.ToString(),
            BizId = employee.BizId.ToString(),
            BizCode = employee.BizCode.ToString(),
            PersonRowId = employee.PersonRowId.Value
        };
    }
}
