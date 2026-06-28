namespace Respondr.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    protected void MarkCreated(DateTimeOffset createdAt)
    {
        CreatedAt = createdAt;
    }

    protected void MarkUpdated(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
