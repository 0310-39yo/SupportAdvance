using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;

public class UserRoleDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public Guid UserRoleId { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public LocalDateTime AssignedAt { get; set; }
}
