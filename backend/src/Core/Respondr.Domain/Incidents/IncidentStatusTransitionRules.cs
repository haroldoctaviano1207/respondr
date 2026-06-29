namespace Respondr.Domain.Incidents;

public static class IncidentStatusTransitionRules
{
    public static bool CanTransition(IncidentStatus currentStatus, IncidentStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            IncidentStatus.New => nextStatus is IncidentStatus.Assigned or IncidentStatus.Cancelled,
            IncidentStatus.Assigned => nextStatus is IncidentStatus.InProgress or IncidentStatus.Cancelled,
            IncidentStatus.InProgress => nextStatus is IncidentStatus.Resolved or IncidentStatus.Cancelled,
            IncidentStatus.Resolved => nextStatus is IncidentStatus.Closed,
            IncidentStatus.Closed => false,
            IncidentStatus.Cancelled => false,
            _ => false
        };
    }
}
