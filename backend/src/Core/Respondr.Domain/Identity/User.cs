namespace Respondr.Domain.Identity;

using Respondr.Domain.Common;

public sealed class User : AuditableEntity
{
    public User(Guid id, string email, string passwordHash, string fullName, Guid roleId, bool isActive = true)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        RoleId = roleId;
        IsActive = isActive;
    }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string FullName { get; private set; }

    public Guid RoleId { get; private set; }

    public Role? Role { get; private set; }

    public bool IsActive { get; private set; }

    private User()
        : base(Guid.Empty)
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
    }
}
