using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Domain.Entities;

public class User : AggregateRoot<RowId>
{
    public string LoginId { get; private set; }
    public string Email { get; private set; }
    public string HashedPassword { get; private set; }
    public bool IsActive { get; private set; }
    public string? DisplayName { get; private set; }

    public User(
        string loginId,
        string email,
        string hashedPassword,
        string? displayName = null,
        RowId? rowId = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(loginId);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(hashedPassword);

        Id = rowId ?? RowId.New();
        LoginId = loginId;
        Email = email;
        HashedPassword = hashedPassword;
        DisplayName = displayName;
        IsActive = true;
    }

    public void ChangePassword(string newHashedPassword)
    {
        ArgumentNullException.ThrowIfNull(newHashedPassword);
        HashedPassword = newHashedPassword;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateEmail(string newEmail)
    {
        ArgumentNullException.ThrowIfNull(newEmail);
        Email = newEmail;
    }
}
