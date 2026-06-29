namespace Respondr.Domain.Tests;

using Respondr.Domain.Dispatch;
using Respondr.Domain.Incidents;
using Respondr.Domain.Resources;

public sealed class WorkflowRuleTests
{
    [Fact]
    public void Incident_status_rules_reject_invalid_transition()
    {
        var canTransition = IncidentStatusTransitionRules.CanTransition(IncidentStatus.New, IncidentStatus.Closed);

        Assert.False(canTransition);
    }

    [Fact]
    public void Dispatch_rules_reject_unavailable_responder_assignment()
    {
        var canAssign = DispatchAssignmentRules.CanAssign(IncidentStatus.New, ResponderStatus.Unavailable, hasActiveAssignment: false);

        Assert.False(canAssign);
    }

    [Fact]
    public void Resource_request_rules_reject_rejecting_fulfilled_request()
    {
        var canTransition = ResourceRequestStatusRules.CanTransition(ResourceRequestStatus.Fulfilled, ResourceRequestStatus.Rejected);

        Assert.False(canTransition);
    }
}
