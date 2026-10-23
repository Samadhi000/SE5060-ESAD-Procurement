using ProcurementApp.Domain.Enums;

namespace ProcurementApp.States;

/// <summary>State pattern: each order lifecycle state knows whether it can receive a delivery and what status follows.</summary>
public interface IOrderState
{
    PurchaseOrderStatus Status { get; }
    bool CanReceiveDelivery { get; }
    PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered);
}

/// <summary>Placed with the supplier, awaiting goods.</summary>
public class PlacedState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.Placed;
    public bool CanReceiveDelivery => true;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) =>
        allLinesFullyDelivered ? PurchaseOrderStatus.FullyDelivered : PurchaseOrderStatus.PartiallyDelivered;
}

/// <summary>Some goods received, more outstanding.</summary>
public class PartiallyDeliveredState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.PartiallyDelivered;
    public bool CanReceiveDelivery => true;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) =>
        allLinesFullyDelivered ? PurchaseOrderStatus.FullyDelivered : PurchaseOrderStatus.PartiallyDelivered;
}

/// <summary>All goods received; no further deliveries.</summary>
public class FullyDeliveredState : IOrderState
{
    public PurchaseOrderStatus Status => PurchaseOrderStatus.FullyDelivered;
    public bool CanReceiveDelivery => false;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) => PurchaseOrderStatus.FullyDelivered;
}

/// <summary>Any state that cannot accept deliveries (Draft, WaitingForApproval, Declined, ...).</summary>
public class NotDeliverableState : IOrderState
{
    public NotDeliverableState(PurchaseOrderStatus status) => Status = status;
    public PurchaseOrderStatus Status { get; }
    public bool CanReceiveDelivery => false;
    public PurchaseOrderStatus AfterDelivery(bool allLinesFullyDelivered) => Status;
}

/// <summary>Maps a stored status to its state object.</summary>
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
