using ProcurementApp.Builders;
using ProcurementApp.Data;
using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>One requested line coming back from the Create-Order form.</summary>
public record OrderLineRequest(string ItemCode, int Quantity);

public record CreateOrderResult(bool Success, string Message, string? Reference);

/// <summary>Everything the wizard collected before step 3 (Review &amp; Submit).</summary>
public record OrderDraftRequest(int SiteId, int SupplierId, int? DepotId, DateTime RequiredBy,
                                IReadOnlyList<OrderLineRequest> Lines);

/// <summary>Business logic for Module 1 — Create &amp; Submit a Purchase Order (UC1 / SD1).</summary>
public interface IOrderCreationService
{
    IReadOnlyList<Site> GetSites();
    IReadOnlyList<Supplier> GetSuppliers();
    IReadOnlyList<CatalogueItem> GetCatalogue(string supplierBusinessId);

    /// <summary>
    /// Step 3 of the wizard: assemble the order with the Builder and return it as an unsaved
    /// DRAFT so the Site Manager can review it before committing.
    /// </summary>
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

    public PurchaseOrder? BuildDraftForReview(OrderDraftRequest request)
    {
        var builder = Assemble(request);
        if (builder is null) return null;

        // BuildAsDraft() applies the light validation only — the order is shown, not saved.
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
            // BuildForApproval() validates: has lines, every line priced.
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

            // OBSERVER: tell the approver (and any other listeners) a new requisition arrived.
            _notifications.Notify("Approving Manager", "A new requisition is waiting for approval.", order);

            return new CreateOrderResult(true, "Requisition submitted for approval.", order.PoReference);
        }
        catch (InvalidOperationException ex)
        {
            return new CreateOrderResult(false, ex.Message, null);
        }
    }

    /// <summary>
    /// BUILDER PATTERN in action: assemble the order step by step from what the wizard collected.
    /// Returns the loaded builder, or null if the chosen site/supplier no longer exist.
    /// </summary>
    private PurchaseOrderBuilder? Assemble(OrderDraftRequest request)
    {
        var site = _reference.GetSite(request.SiteId);
        var supplier = _reference.GetSupplier(request.SupplierId);
        if (site is null || supplier is null) return null;

        // The PO reference is only drawn at submit time, so previewing never burns a number.
        var builder = new PurchaseOrderBuilder()
            .ForSite(site)
            .FromSupplier(supplier)
            .RequiredBy(request.RequiredBy);

        // Optional depot — only offered for suppliers that operate more than one.
        if (request.DepotId is int depotId && depotId > 0)
            builder.WithDepot(_reference.GetDepot(depotId));

        // Record who raised the requisition, so it shows as "Submitted By" on the approval screens.
        var manager = _reference.GetActingSiteManager();
        if (manager is not null) builder.CreatedBy(manager);

        var catalogue = _reference.GetCatalogueForSupplier(supplier.SupplierId);
        foreach (var line in request.Lines.Where(l => l.Quantity > 0))
        {
            var priced = catalogue.FirstOrDefault(c => c.Item.ItemCode == line.ItemCode);
            if (priced is null) continue;   // item not in this supplier's catalogue — skip
            builder.AddLine(priced.Item, line.Quantity, priced.AgreedPrice);
        }

        return builder;
    }
}
