using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Builders;

/// <summary>
/// BUILDER PATTERN — assembles a <see cref="PurchaseOrder"/> step by step (choose site, choose
/// supplier, add each priced line) and only produces a finished order through <c>Build…()</c>,
/// which validates it first. This matches the multi-step "New Purchase Order" wizard in the
/// wireframe and guarantees an order can never exist in a half-valid state. Used by Module 1.
/// </summary>
public class PurchaseOrderBuilder
{
    private readonly PurchaseOrder _order = new();
    private int _nextLineNumber = 1;

    public PurchaseOrderBuilder WithReference(string reference)
    {
        _order.PoReference = reference;
        return this;
    }

    public PurchaseOrderBuilder ForSite(Site site)
    {
        _order.Site = site;
        _order.DeliveryAddress = site.Address;
        return this;
    }

    public PurchaseOrderBuilder FromSupplier(Supplier supplier)
    {
        _order.Supplier = supplier;
        return this;
    }

    public PurchaseOrderBuilder WithDepot(Depot? depot)
    {
        _order.Depot = depot;
        return this;
    }

    public PurchaseOrderBuilder CreatedBy(SiteManager manager)
    {
        _order.CreatedBy = manager;
        return this;
    }

    public PurchaseOrderBuilder RequiredBy(DateTime date)
    {
        _order.RequiredByDate = date;
        return this;
    }

    public PurchaseOrderBuilder AddLine(Item item, int quantity, decimal unitPrice)
    {
        _order.Lines.Add(new PurchaseOrderLine
        {
            LineNumber = _nextLineNumber++,
            Item = item,
            QuantityOrdered = quantity,
            UnitPrice = unitPrice
        });
        return this;
    }

    /// <summary>Finish as a draft (saved for later) — minimal checks only.</summary>
    public PurchaseOrder BuildAsDraft()
    {
        Require(_order.Site is not null, "A delivery site must be selected.");
        Require(_order.Supplier is not null, "A supplier must be selected.");
        _order.DateCreated = DateTime.Now;
        _order.Status = PurchaseOrderStatus.Draft;
        return _order;
    }

    /// <summary>Finish and submit for approval — full validation (brief: no line may be unpriced).</summary>
    public PurchaseOrder BuildForApproval()
    {
        Require(_order.Site is not null, "A delivery site must be selected.");
        Require(_order.Supplier is not null, "A supplier must be selected.");
        Require(_order.Lines.Count > 0, "At least one item must be added.");
        Require(!_order.HasUnpricedItems(), "Every line must have an agreed price before submitting.");
        _order.DateCreated = DateTime.Now;
        _order.Status = PurchaseOrderStatus.WaitingForApproval;
        return _order;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
