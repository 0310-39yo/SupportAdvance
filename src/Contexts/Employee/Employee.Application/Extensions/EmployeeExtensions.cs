namespace SupportAdvance.Contexts.Employee.Application.Extensions;

using Domain.Entities;
using Dtos;
using SupportAdvance.SharedKernel.Entities;

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
        // 要約（IEmployee.ToSummary）を唯一の変換元とする（部署名の連結規則を重複させない）
        var summary = ((IEmployee)entity).ToSummary();

        return new EmployeeDto
        {
            RowId = summary.RowId,
            TypeDivision = summary.TypeDivision,
            BizId = summary.BizId,
            BizCode = summary.BizCode,
            PersonRowId = summary.PersonRowId,
            PersonLastName = summary.PersonLastName,
            PersonFirstName = summary.PersonFirstName,
            PersonLastNameKana = summary.PersonLastNameKana,
            PersonFirstNameKana = summary.PersonFirstNameKana,
            DepartmentNames = summary.DepartmentNames
        };
    }
}
