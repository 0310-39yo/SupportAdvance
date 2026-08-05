namespace SupportAdvance.Contexts.Employee.Infrastructure.DbModels;

/// <summary>
/// 従業員 DbModel - ORM マッピング用
/// 【注意】DateTime はプリミティブ型のまま保持。LocalDateTime への変換は Mapper の責務。
/// </summary>
public class EmployeeDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? HireDate { get; set; }
}
