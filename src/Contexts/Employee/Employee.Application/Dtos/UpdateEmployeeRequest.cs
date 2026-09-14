namespace SupportAdvance.Contexts.Employee.Application.Dtos;

/// <summary>
/// 従業員更新リクエスト
/// </summary>
public record UpdateEmployeeRequest
{
    /// <summary>
    /// 更新対象の従業員RowId（必須）
    /// </summary>
    public required long EmployeeRowId { get; init; }

    /// <summary>
    /// 新しい従業員区分コード（オプション、M/T/C のいずれか）
    /// </summary>
    public string? DivisionCode { get; init; }

    /// <summary>
    /// 新しい従業員番号（オプション、1001-9999）
    /// </summary>
    public int? EmployeeNumber { get; init; }
}
