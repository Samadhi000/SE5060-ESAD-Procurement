using ProcurementApp.Data;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;
using ProcurementApp.States;

namespace ProcurementApp.Services;

/// <summary>One delivered line entered on the Record-Delivery form.</summary>
public record DeliveryLineRequest(int LineNumber, string ItemDescription, int QuantityDelivered, string Condition);

public record DeliveryResult(bool Success, string Message, PurchaseOrderStatus? NewStatus,
                             string? Reference = null,
                             IReadOnlyList<DeliveryLineRequest>? Recorded = null);

/// <summary>Business logic for Module 3 — Log Delivery / Goods Receipt (UC3 / SD3).</summary>
public interface IDeliveryService
{
    IReadOnlyList<PurchaseOrder> GetDeliverableOrders();
    PurchaseOrder? GetOrder(string reference);
    DeliveryResult RecordDelivery(string reference, string supplierReference, DateTime date,
                                  IEnumerable<DeliveryLineRequest> lines, string receivedBy);
}

public class DeliveryService : IDeliveryService
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly IDeliveryRepository _deliveries;
    private readonly INotificationService _notifications;

    public DeliveryService(IPurchaseOrderRepository orders, IDeliveryRepository deliveries, INotificationService notifications)
    {
        _orders = orders;
        _deliveries = deliveries;
        _notifications = notifications;
    }

    // Orders that are placed or partly delivered can still receive goods.
    public IReadOnlyList<PurchaseOrder> GetDeliverableOrders() =>
        _orders.GetByStatus(PurchaseOrderStatus.Placed)
            .Concat(_orders.GetByStatus(PurchaseOrderStatus.PartiallyDelivered))
            .ToList();

    public PurchaseOrder? GetOrder(string reference) => _orders.GetByReference(reference);

    public DeliveryResult RecordDelivery(string reference, string supplierReference, DateTime date,
                                         IEnumerable<DeliveryLineRequest> lines, string receivedBy)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return new DeliveryResult(false, $"Order {reference} not found.", null);

        // STATE PATTERN: ask the current state whether a delivery is even allowed.
        var state = OrderStateFactory.For(order.Status);
        if (!state.CanReceiveDelivery)
            return new DeliveryResult(false, $"Order is '{order.Status}' and cannot receive deliveries.", order.Status);

        // Build the delivery note and apply each delivered quantity to the matching order line.
        var note = new DeliveryNote
        {
            DeliveryNoteId = "DN-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
            PurchaseOrderReference = reference,
            SupplierReference = supplierReference,
            DeliveryDate = date,
            ReceivedBy = receivedBy
        };

        foreach (var reqLine in lines.Where(l => l.QuantityDelivered > 0))
        {
            note.Lines.Add(new DeliveryLine
            {
                LineNumber = reqLine.LineNumber,
                ItemDescription = reqLine.ItemDescription,
                QuantityDelivered = reqLine.QuantityDelivered,
                Condition = reqLine.Condition
            });

            var orderLine = order.Lines.FirstOrDefault(l => l.LineNumber == reqLine.LineNumber);
            if (orderLine is not null)
                orderLine.QuantityDelivered += reqLine.QuantityDelivered;
        }

        // STATE PATTERN: the state decides the resulting status (partially vs fully delivered).
        var allFully = order.Lines.All(l => l.IsFullyDelivered());
        var newStatus = state.AfterDelivery(allFully);
        order.ChangeStatus(newStatus);

        _deliveries.Add(note);
        _orders.Update(order);

        // OBSERVER: notify procurement when the order is ready for invoice matching.
        var message = newStatus == PurchaseOrderStatus.FullyDelivered
            ? "Order fully delivered — ready for invoice matching."
            : "Partial delivery recorded — balance still outstanding.";
        _notifications.Notify("Procurement / Accounting Staff", message, order);

        return new DeliveryResult(true, message, newStatus, reference, note.Lines
            .Select(l => new DeliveryLineRequest(l.LineNumber, l.ItemDescription, l.QuantityDelivered, l.Condition))
            .ToList());
    }
}
