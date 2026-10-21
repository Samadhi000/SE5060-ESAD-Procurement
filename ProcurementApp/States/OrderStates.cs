using ProcurementApp.Domain.Enums;

namespace ProcurementApp.States;

/// <summary>
/// STATE PATTERN — the state interface. The behaviour of a purchase order (can it receive a
/// delivery? what status does it become after one?) depends on which lifecycle state it is in.
/// Instead of scattering big if/switch statements on Status throughout the code, each state is
/// its own class that knows its own rules and its own transitions. Used by Module 3.
/// </summary>
public interface IOrderState
{
    PurchaseOrderStatus Status { get; }

    /// <summary>Whether a delivery can be logged while the order is in this state.</summary>
    bool CanReceiveDelivery { get; }

    /// <summary>The state to move to after a delivery, given whether every line is now fully delivered.</summary>
    PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered);
}

/// <summary>Order has been placed with the supplier and is awaiting goods.</summary>
public class PlacedState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.Placed;
    public bool CanReceiveDelivery => true;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) =>
        allLinesFullyDelivered ? PurchaseOrderStatus.FullyDelivered : PurchaseOrderStatus.PartiallyDelivered;
}

/// <summary>Some goods received; more still outstanding.</summary>
public class PartiallyDeliveredState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.PartiallyDelivered;
    public bool CanReceiveDelivery => true;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) =>
        allLinesFullyDelivered ? PurchaseOrderStatus.FullyDelivered : PurchaseOrderStatus.PartiallyDelivered;
}

/// <summary>All goods received — no further deliveries allowed.</summary>
public class FullyDeliveredState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.FullyDelivered;
    public bool CanReceiveDelivery => false;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) => PurchaseOrderStatus.FullyDelivered;
}

/// <summary>Any state that cannot accept deliveries (Draft, WaitingForApproval, Declined, …).</summary>
public class NotDeliverableState : IOrderState
{
    public NotDeliverableState(PurchaseOrderStatus status) => Status = status;
    public PurchaseOrderStatus Status { get; }
    public bool CanReceiveDelivery => false;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) => Status;
}

/// <summary>Maps a stored status value to the matching state object (part of the State pattern).</summary>
public static class OrderStateFactory
{
    public static IOrderState For(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.Placed => new PlacedState(),
        PurchaseOrderStatus.PartiallyDelivered => new PartiallyDeliveredState(),
        PurchaseOrderStatus.FullyDelivered => new FullyDeliveredState(),
        _ => new NotDeliverableState(status)
    };
}
