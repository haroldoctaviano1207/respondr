namespace Respondr.Infrastructure.Identity;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "Identity";

    public bool SeedDevelopmentUsers { get; init; }

    public bool AllowOpenRegistration { get; init; }

    public string DispatcherEmail { get; init; } = string.Empty;

    public string DispatcherPassword { get; init; } = string.Empty;

    public string OperationsLeadEmail { get; init; } = string.Empty;

    public string OperationsLeadPassword { get; init; } = string.Empty;
}
