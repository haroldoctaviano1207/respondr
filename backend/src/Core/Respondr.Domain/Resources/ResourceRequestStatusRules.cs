namespace Respondr.Domain.Resources;

public static class ResourceRequestStatusRules
{
    public static bool CanTransition(ResourceRequestStatus currentStatus, ResourceRequestStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            ResourceRequestStatus.Pending => nextStatus is ResourceRequestStatus.Approved
                or ResourceRequestStatus.Allocated
                or ResourceRequestStatus.Rejected
                or ResourceRequestStatus.Cancelled,
            ResourceRequestStatus.Approved => nextStatus is ResourceRequestStatus.Allocated
                or ResourceRequestStatus.Cancelled,
            ResourceRequestStatus.Allocated => nextStatus is ResourceRequestStatus.Fulfilled,
            ResourceRequestStatus.Rejected => false,
            ResourceRequestStatus.Fulfilled => false,
            ResourceRequestStatus.Cancelled => false,
            _ => false
        };
    }
}
