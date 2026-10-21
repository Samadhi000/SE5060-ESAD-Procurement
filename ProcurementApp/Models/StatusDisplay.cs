using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Models;

/// <summary>
/// Presentation helpers shared by the phone and portal screens, so a status is always worded
/// and coloured the same way (the design's wireframes use "Awaiting Approval", not the raw
/// enum name "WaitingForApproval").
/// </summary>
public static class StatusDisplay
{
    public static string Label(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.WaitingForApproval => "Awaiting Approval",
        PurchaseOrderStatus.PartiallyApproved => "Partially Approved",
        PurchaseOrderStatus.ReturnedToOriginator => "Returned",
        PurchaseOrderStatus.PartiallyDelivered => "Partially Delivered",
        PurchaseOrderStatus.FullyDelivered => "Fully Delivered",
        _ => status.ToString()
    };

    /// <summary>The pill modifier class that matches the wireframe's colour for this status.</summary>
    public static string PillClass(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.WaitingForApproval => "pill--amber",
        PurchaseOrderStatus.PartiallyApproved => "pill--amber",
        PurchaseOrderStatus.Referred => "pill--blue",
        PurchaseOrderStatus.Approved => "pill--green",
        PurchaseOrderStatus.Placed => "pill--green",
        PurchaseOrderStatus.PartiallyDelivered => "pill--blue",
        PurchaseOrderStatus.FullyDelivered => "pill--grey",
        PurchaseOrderStatus.Declined => "pill--red",
        PurchaseOrderStatus.Void => "pill--red",
        _ => "pill--grey"
    };

    /// <summary>"2 days ago" style age used on the phone's order cards.</summary>
    public static string Age(DateTime when)
    {
        var days = (DateTime.Now.Date - when.Date).Days;
        return days switch
        {
            <= 0 => "today",
            1 => "yesterday",
            < 7 => $"{days} days ago",
            < 14 => "1 week ago",
            < 60 => $"{days / 7} weeks ago",
            _ => when.ToString("d MMM yyyy")
        };
    }
}
