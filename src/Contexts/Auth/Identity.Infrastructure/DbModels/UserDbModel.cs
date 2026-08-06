using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Auth.Identity.Infrastructure.DbModels;

public class UserDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public Guid UserId { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? DisplayName { get; set; }
}
