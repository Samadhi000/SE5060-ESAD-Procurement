using ProcurementApp.Builders;
using ProcurementApp.Data;
using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>One requested line from the Create-Order form.</summary>
public record OrderLineRequest(string ItemCode, int Quantity);

public record CreateOrderResult(bool Success, string Message, string? Reference);

/// <summary>Everything the wizard collected before the Review step.</summary>
public record OrderDraftRequest(int SiteId, int SupplierId, int? DepotId, DateTime RequiredBy,
                                IReadOnlyList<OrderLineRequest> Lines);

/// <summary>Module 1: create and submit a purchase order, using the Builder.</summary>
public interface IOrderCreationService
{
    IReadOnlyList<Site> GetSites();
    IReadOnlyList<Supplier> GetSuppliers();
    IReadOnlyList<CatalogueItem> GetCatalogue(string supplierBusinessId);
    PurchaseOrder? BuildDraftForReview(OrderDraftRequest request);
    CreateOrderResult Submit(OrderDraftRequest request, string? comments);
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

    // Build the order as an unsaved draft so it can be shown on the Review screen.
    public PurchaseOrder? BuildDraftForReview(OrderDraftRequest request)
    {
        var builder = Assemble(request);
        if (builder is null) return null;

        try { return builder.BuildAsDraft(); }
        catch (InvalidOperationException) { return null; }
    }

    public CreateOrderResult Submit(OrderDraftRequest request, string? comments)
    {
        var builder = Assemble(request);
        if (builder is null)
            return new CreateOrderResult(false, "Please choose a valid site and supplier.", null);

        try
        {
            var order = builder
                .WithReference(_references.NextPurchaseOrderReference())
                .BuildForApproval();

            if (!string.IsNullOrWhiteSpace(comments))
            {
                order.AddComment(new Comment
                {
                    Text = comments.Trim(),
                    Author = order.CreatedBy?.Name ?? "Site Manager"
                });
            }

            _orders.Add(order);
            _notifications.Notify("Approving Manager", "A new requisition is waiting for approval.", order);
            return new CreateOrderResult(true, "Requisition submitted for approval.", order.PoReference);
        }
        catch (InvalidOperationException ex)
        {
            return new CreateOrderResult(false, ex.Message, null);
        }
    }

    // Builder in use: assemble the order from what the wizard collected. The reference is drawn at submit time only.
    private PurchaseOrderBuilder? Assemble(OrderDraftRequest request)
    {
        var site = _reference.GetSite(request.SiteId);
        var supplier = _reference.GetSupplier(request.SupplierId);
        if (site is null || supplier is null) return null;

        var builder = new PurchaseOrderBuilder()
            .ForSite(site)
            .FromSupplier(supplier)
            .RequiredBy(request.RequiredBy);

        if (request.DepotId is int depotId && depotId > 0)
            builder.WithDepot(_reference.GetDepot(depotId));

        var manager = _reference.GetActingSiteManager();
        if (manager is not null) builder.CreatedBy(manager);

        var catalogue = _reference.GetCatalogueForSupplier(supplier.SupplierId);
        foreach (var line in request.Lines.Where(l => l.Quantity > 0))
        {
            var priced = catalogue.FirstOrDefault(c => c.Item.ItemCode == line.ItemCode);
            if (priced is null) continue;
            builder.AddLine(priced.Item, line.Quantity, priced.AgreedPrice);
        }

        return builder;
    }
}
