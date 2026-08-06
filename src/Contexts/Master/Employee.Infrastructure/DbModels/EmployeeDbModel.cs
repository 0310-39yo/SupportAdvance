using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;

/// <summary>
/// 従業員 DbModel - ORM マッピング用
/// 【注意】LocalDateTime はグローバル ORM マッピングで DateTime2 に変換される。
/// </summary>
public class EmployeeDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public Guid EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public LocalDateTime? HireDate { get; set; }
}
