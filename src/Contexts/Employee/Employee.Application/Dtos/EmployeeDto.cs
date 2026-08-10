namespace SupportAdvance.Contexts.Employee.Application.Dtos;

/// <summary>
/// 従業員データ転送オブジェクト
/// </summary>
public record EmployeeDto
{
    /// <summary>
    /// 従業員ID（集約根）
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// DB行ID
    /// </summary>
    public required long RowId { get; init; }

    /// <summary>
    /// 従業員コード（"M/1234" 形式）
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 人事マスタ行ID
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
