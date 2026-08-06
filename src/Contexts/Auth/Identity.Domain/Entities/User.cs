using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.Entities;

public class User : AggregateRoot<UserId>
{
    private RowId _rowId = null!;

    public RowId RowId => _rowId;
    public string LoginId { get; private set; }
    public string Email { get; private set; }
    public string HashedPassword { get; private set; }
    public bool IsActive { get; private set; }
    public string? DisplayName { get; private set; }

    public User(
        UserId id,
        string loginId,
        string email,
        string hashedPassword,
        string? displayName = null,
        RowId? rowId = null,
        IClock? clock = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(loginId);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(hashedPassword);

        Id = id;
        _rowId = rowId ?? RowId.New();
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
