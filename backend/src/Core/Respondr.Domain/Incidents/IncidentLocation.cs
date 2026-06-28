namespace Respondr.Domain.Incidents;

using Respondr.Domain.Common;

public sealed class IncidentLocation : Entity
{
    public IncidentLocation(
        Guid id,
        string address,
        string? city = null,
        string? region = null,
        double? latitude = null,
        double? longitude = null)
        : base(id)
    {
        Address = address;
        City = city;
        Region = region;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Address { get; private set; }

    public string? City { get; private set; }

    public string? Region { get; private set; }

    public double? Latitude { get; private set; }

    public double? Longitude { get; private set; }

    private IncidentLocation()
        : base(Guid.Empty)
    {
        Address = string.Empty;
    }
}
