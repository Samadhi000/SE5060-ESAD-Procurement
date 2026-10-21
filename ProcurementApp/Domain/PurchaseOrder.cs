using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Domain;

/// <summary>
/// The central domain object (ESAD-02 class diagram: PurchaseOrder). A requisition and a
/// purchase order are the SAME object; the <see cref="Status"/> drives the workflow.
/// SHARED CLASS — used by every module. Do not add module-specific logic here; put that in
/// your own service. Coordinate any structural change with the team.
/// </summary>
public class PurchaseOrder
{
    [Key] public int Id { get; set; }                         // database primary key

    public string PoReference { get; set; } = string.Empty;   // business reference, e.g. PO-2026-04817
    public DateTime DateCreated { get; set; }
    public DateTime RequiredByDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public Site? Site { get; set; }
    public Supplier? Supplier { get; set; }
    public Depot? Depot { get; set; }
    public SiteManager? CreatedBy { get; set; }

    public List<PurchaseOrderLine> Lines { get; set; } = new();
    public List<Approval> Approvals { get; set; } = new();   // approval history (UC2)
    public List<Comment> Comments { get; set; } = new();

    [NotMapped] public decimal TotalValue => CalculateTotal();   // computed, not stored

    public decimal CalculateTotal() => Lines.Sum(l => l.LineTotal);

    public bool HasUnpricedItems() => Lines.Any(l => l.UnitPrice <= 0m);

    public void ChangeStatus(PurchaseOrderStatus newStatus) => Status = newStatus;

    public void AddComment(Comment comment) => Comments.Add(comment);
}

/// <summary>A single ordered line within a purchase order (ESAD-02: PurchaseOrderLine).</summary>
public class PurchaseOrderLine
{
    [Key] public int Id { get; set; }

    public int LineNumber { get; set; }
    public Item Item { get; set; } = new();
    public int QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public int QuantityDelivered { get; set; }

    [NotMapped] public decimal LineTotal => QuantityOrdered * UnitPrice;   // computed, not stored
    public bool IsFullyDelivered() => QuantityDelivered >= QuantityOrdered;
}

/// <summary>A note/comment left on an order, e.g. by an approver for the originator.</summary>
public class Comment
{
    [Key] public int Id { get; set; }

    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Author { get; set; } = string.Empty;
}
