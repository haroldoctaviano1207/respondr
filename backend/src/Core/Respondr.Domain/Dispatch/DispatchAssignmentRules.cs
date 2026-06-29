namespace Respondr.Domain.Dispatch;

using Respondr.Domain.Incidents;

public static class DispatchAssignmentRules
{
    public static bool CanAssign(IncidentStatus incidentStatus, ResponderStatus responderStatus, bool hasActiveAssignment)
    {
        if (hasActiveAssignment)
        {
            return false;
        }

        if (responderStatus != ResponderStatus.Available)
        {
            return false;
        }

        return incidentStatus is not IncidentStatus.Closed and not IncidentStatus.Cancelled;
    }
}
