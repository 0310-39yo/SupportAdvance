namespace SupportAdvance.Contexts.Employee.Application.Dtos;

/// <summary>
/// 従業員作成リクエスト
/// </summary>
public record CreateEmployeeRequest
{
    /// <summary>
    /// 人事マスタ行ID（必須、1以上）
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 従業員区分コード（必須、M/T/C のいずれか）
    /// </summary>
    public required string DivisionCode { get; init; }

    /// <summary>
    /// 従業員番号（必須、1001-9999）
    /// </summary>
    public required int EmployeeNumber { get; init; }

    /// <summary>
    /// 姓（必須）
    /// </summary>
    public required string PersonLastName { get; init; }

    /// <summary>
    /// 名（必須）
    /// </summary>
    public required string PersonFirstName { get; init; }

    /// <summary>
    /// 姓（カナ）（必須）
    /// </summary>
    public required string PersonLastNameKana { get; init; }

    /// <summary>
    /// 名（カナ）（必須）
    /// </summary>
    public required string PersonFirstNameKana { get; init; }
}
