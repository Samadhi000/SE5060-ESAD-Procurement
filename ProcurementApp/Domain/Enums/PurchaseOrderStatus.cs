namespace ProcurementApp.Domain.Enums;

/// <summary>
/// The lifecycle states of a purchase order / requisition, taken from the ESAD-02 design.
/// NOTE (team): this is the SHARED status set. Module 3 (State pattern) owns the full
/// lifecycle transitions — agree any additions with that member before changing this file.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft,
    WaitingForApproval,
    Approved,
    PartiallyApproved,
    Declined,
    Referred,
    ReturnedToOriginator,
    Placed,
    PartiallyDelivered,
    FullyDelivered,
    Void
}
