namespace Respondr.Shared.Time;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
