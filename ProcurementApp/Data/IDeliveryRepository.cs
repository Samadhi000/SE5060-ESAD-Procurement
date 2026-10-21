using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>Storage for delivery notes / goods receipts (UC3).</summary>
public interface IDeliveryRepository
{
    void Add(DeliveryNote note);
    IReadOnlyList<DeliveryNote> GetForOrder(string purchaseOrderReference);
}
