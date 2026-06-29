namespace Respondr.Infrastructure.Realtime;

public sealed class RealtimeApiOptions
{
    public const string SectionName = "Realtime";

    public bool Enabled { get; set; } = false;

    public string BaseUrl { get; set; } = "http://localhost:5206";

    public string InternalApiKey { get; set; } = "respondr-local-internal-key";
}
