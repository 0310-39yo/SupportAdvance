using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;

public class RoleDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PermissionsJson { get; set; } = "[]";
}
