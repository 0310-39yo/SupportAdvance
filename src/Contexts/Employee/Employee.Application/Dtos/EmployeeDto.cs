using SupportAdvance.Application.Queries;

namespace SupportAdvance.Contexts.Employee.Application.Dtos;

/// <summary>
/// 従業員データ転送オブジェクト
/// 【実装】IEmployeeQueryResult を実装（汎用層インターフェース経由での参照に対応）
/// </summary>
public record EmployeeDto : IEmployeeQueryResult
{
    /// <summary>
    /// 従業員RowId（集約根）
    /// </summary>
    public required long RowId { get; init; }

    /// <summary>
    /// 従業員種別区分（正社員/派遣/請負）
    /// </summary>
    public required string TypeDivision { get; init; }

    /// <summary>
    /// ビジネスID（従業員番号）
    /// </summary>
    public required string BizId { get; init; }

    /// <summary>
    /// ビジネスコード（表示用）
    /// </summary>
    public required string BizCode { get; init; }

    /// <summary>
    /// 人事マスタ行ID
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 姓
    /// </summary>
    public required string PersonLastName { get; init; }

    /// <summary>
    /// 名
    /// </summary>
    public required string PersonFirstName { get; init; }

    /// <summary>
    /// 姓（カナ）
    /// </summary>
    public required string PersonLastNameKana { get; init; }

    /// <summary>
    /// 名（カナ）
    /// </summary>
    public required string PersonFirstNameKana { get; init; }

    /// <summary>
    /// 所属部署名（カンマ区切り、主部署を先頭に）
    /// </summary>
    public string DepartmentNames { get; init; } = string.Empty;
}
