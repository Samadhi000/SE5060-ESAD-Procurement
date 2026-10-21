using System.ComponentModel.DataAnnotations;

namespace ProcurementApp.Domain;

/// <summary>
/// Goods-receipt record against a purchase order (ESAD-02 class diagram: DeliveryNote).
/// Used by Module 3 (Log Delivery / State pattern).
/// </summary>
public class DeliveryNote
{
    [Key] public int Id { get; set; }

    public string DeliveryNoteId { get; set; } = string.Empty;
    public string PurchaseOrderReference { get; set; } = string.Empty;  // which order this delivery is against
    public DateTime DeliveryDate { get; set; }
    public string SupplierReference { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public List<DeliveryLine> Lines { get; set; } = new();
}

/// <summary>A delivered line matched against a purchase order line.</summary>
public class DeliveryLine
{
    [Key] public int Id { get; set; }

    public int LineNumber { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public int QuantityDelivered { get; set; }
    public string Condition { get; set; } = "Good";
}
