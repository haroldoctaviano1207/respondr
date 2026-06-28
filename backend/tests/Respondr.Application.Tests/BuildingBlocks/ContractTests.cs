using Respondr.Contracts.Incidents;
using Respondr.Contracts.Realtime;

namespace Respondr.Application.Tests.BuildingBlocks;

public class ContractTests
{
    [Fact]
    public void IncidentCreatedEvent_PreservesEventData()
    {
        var incidentId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new IncidentCreatedEvent(
            incidentId,
            "INC-000001",
            "Flood response",
            "Critical",
            "New",
            occurredAt);

        Assert.Equal(incidentId, @event.IncidentId);
        Assert.Equal("INC-000001", @event.IncidentNumber);
        Assert.Equal("Critical", @event.Priority);
        Assert.Equal(occurredAt, @event.CreatedAt);
    }

    [Fact]
    public void RealtimeMessage_WrapsPayloadWithMetadata()
    {
        var payload = new DashboardUpdatedEvent(
            ActiveIncidents: 4,
            CriticalIncidents: 1,
            AvailableResponders: 8,
            PendingResourceRequests: 2,
            UpdatedAt: DateTimeOffset.UtcNow);

        var message = new RealtimeMessage<DashboardUpdatedEvent>(
            "DashboardUpdated",
            payload,
            DateTimeOffset.UtcNow,
            "correlation-id");

        Assert.Equal("DashboardUpdated", message.EventType);
        Assert.Equal(payload, message.Payload);
        Assert.Equal("correlation-id", message.CorrelationId);
    }
}
