namespace SupportAdvance.Contexts.Employee.Application.Extensions;

using Domain.Entities;
using Dtos;

/// <summary>
/// Employee Entity の拡張メソッド
/// </summary>
public static class EmployeeExtensions
{
    /// <summary>
    /// Employee Entity を EmployeeDto に変換
    /// 【責務】部署名をカンマ区切りで連結（主部署を先頭に）
    /// </summary>
    public static EmployeeDto ToDto(this Employee entity)
    {
        // DepartmentMembership から部署名を取得（主部署を先頭に）
        var departmentNames = string.Join(", ",
            entity.DepartmentMemberships
                .OrderByDescending(m => m.IsPrimary.Value)  // 主部署を先頭に
                .Select(m => m.DepartmentName ?? string.Empty));

        return new EmployeeDto
        {
            RowId = entity.RowId.Value,
            TypeDivision = entity.TypeDivision.ToString(),
            BizId = entity.BizId.Value.ToString(),
            BizCode = entity.BizCode.ToString(),
            PersonRowId = entity.Person.RowId.Value,
            PersonLastName = entity.Person.LastName.Value,
            PersonFirstName = entity.Person.FirstName.Value,
            PersonLastNameKana = entity.Person.LastNameKana.Value,
            PersonFirstNameKana = entity.Person.FirstNameKana.Value,
            DepartmentNames = departmentNames
        };
    }
}
