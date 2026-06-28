namespace Respondr.Domain.Identity;

using Respondr.Domain.Common;

public sealed class Role : Entity
{
    public Role(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    private Role()
        : base(Guid.Empty)
    {
        Name = string.Empty;
    }
}
