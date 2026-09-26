namespace SupportAdvance.Contexts.Employee.Application.Extensions;

using Domain.Entities;
using Dtos;

/// <summary>
/// <see cref="Employee"/> の拡張メソッド
/// </summary>
public static class EmployeeExtensions
{
    /// <summary>
    /// <see cref="Employee"/> の <see cref="EmployeeDto"/> への変換
    /// </summary>
    /// <param name="entity">変換する従業員</param>
    /// <returns>変換した DTO。<c>DepartmentNames</c> は主所属を先頭にしたカンマ区切り（所属なしの場合は空文字）</returns>
    /// <remarks>
    /// <para>【責務】部署名のカンマ区切りでの連結（主部署を先頭に）</para>
    /// </remarks>
    public static EmployeeDto ToDto(this Employee entity)
    {
        // DepartmentMembership から部署名を取得（主部署を先頭に）
        var departmentNames = string.Join(", ",
            entity.DepartmentMemberships
                .OrderByDescending(m => m.IsPrimary.Value)  // 主部署を先頭に
                .Select(m => m.DepartmentDisplayName.Value));  // 名前なしの場合は空文字

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
