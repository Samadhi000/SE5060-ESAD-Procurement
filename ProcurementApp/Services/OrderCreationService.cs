using ProcurementApp.Builders;
using ProcurementApp.Data;
using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>One requested line coming back from the Create-Order form.</summary>
public record OrderLineRequest(string ItemCode, int Quantity);

public record CreateOrderResult(bool Success, string Message, string? Reference);

/// <summary>Business logic for Module 1 — Create &amp; Submit a Purchase Order (UC1 / SD1).</summary>
public interface IOrderCreationService
{
    IReadOnlyList<Site> GetSites();
    IReadOnlyList<Supplier> GetSuppliers();
    IReadOnlyList<CatalogueItem> GetCatalogue(string supplierBusinessId);

    CreateOrderResult Submit(int siteId, int supplierId, DateTime requiredBy, IEnumerable<OrderLineRequest> lines);
}

public class OrderCreationService : IOrderCreationService
{
    private readonly IReferenceDataRepository _reference;
    private readonly IPurchaseOrderRepository _orders;
    private readonly IReferenceNumberGenerator _references;
    private readonly INotificationService _notifications;

    public OrderCreationService(
        IReferenceDataRepository reference,
        IPurchaseOrderRepository orders,
        IReferenceNumberGenerator references,
        INotificationService notifications)
    {
        _reference = reference;
        _orders = orders;
        _references = references;
        _notifications = notifications;
    }

    public IReadOnlyList<Site> GetSites() => _reference.GetSites();
    public IReadOnlyList<Supplier> GetSuppliers() => _reference.GetSuppliers();
    public IReadOnlyList<CatalogueItem> GetCatalogue(string supplierBusinessId) =>
        _reference.GetCatalogueForSupplier(supplierBusinessId);

    public CreateOrderResult Submit(int siteId, int supplierId, DateTime requiredBy, IEnumerable<OrderLineRequest> lines)
    {
        var site = _reference.GetSite(siteId);
        var supplier = _reference.GetSupplier(supplierId);
        if (site is null || supplier is null)
            return new CreateOrderResult(false, "Please choose a valid site and supplier.", null);

        var catalogue = _reference.GetCatalogueForSupplier(supplier.SupplierId);

        // BUILDER PATTERN in action: assemble the order step by step.
        var builder = new PurchaseOrderBuilder()
            .WithReference(_references.NextPurchaseOrderReference())
            .ForSite(site)
            .FromSupplier(supplier)
            .RequiredBy(requiredBy);

        foreach (var line in lines.Where(l => l.Quantity > 0))
        {
            var priced = catalogue.FirstOrDefault(c => c.Item.ItemCode == line.ItemCode);
            if (priced is null) continue;   // item not in this supplier's catalogue — skip
            builder.AddLine(priced.Item, line.Quantity, priced.AgreedPrice);
        }

        try
        {
            var order = builder.BuildForApproval();   // validates: has lines, all priced
            _orders.Add(order);

            // OBSERVER: tell the approver (and any other listeners) a new requisition arrived.
            _notifications.Notify("Approving Manager", "A new requisition is waiting for approval.", order);

            return new CreateOrderResult(true, "Requisition submitted for approval.", order.PoReference);
        }
        catch (InvalidOperationException ex)
        {
            return new CreateOrderResult(false, ex.Message, null);
        }
    }
}
