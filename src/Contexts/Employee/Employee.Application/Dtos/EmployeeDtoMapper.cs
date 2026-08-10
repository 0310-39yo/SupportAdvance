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
            Id = employee.Id.Value,
            RowId = employee.RowId.Value,
            Code = employee.Code.ToString(),
            PersonRowId = employee.PersonRowId.Value,
            CreatedAt = DateTime.UtcNow  // TODO: Entity に CreatedAt を追加
        };
    }
}
