using ProcurementApp.Data;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Services;

/// <summary>An order plus its delivery history, for the enquiry detail screen.</summary>
public record OrderEnquiryResult(PurchaseOrder Order, IReadOnlyList<DeliveryNote> Deliveries);

/// <summary>One row of the management status report.</summary>
public record StatusSummaryRow(PurchaseOrderStatus Status, int Count, decimal Value);

/// <summary>Business logic for Module 4 — Enquire About / View Orders + Reports (UC4).</summary>
public interface IEnquiryService
{
    IReadOnlyList<PurchaseOrder> GetAll();
    IReadOnlyList<PurchaseOrder> Search(string? text, PurchaseOrderStatus? status);
    OrderEnquiryResult? GetDetail(string reference);

    /// <summary>Management report: how many orders and how much value sit in each status.</summary>
    IReadOnlyList<StatusSummaryRow> GetStatusSummary();
    decimal GetTotalValue();
}

public class EnquiryService : IEnquiryService
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly IDeliveryRepository _deliveries;

    public EnquiryService(IPurchaseOrderRepository orders, IDeliveryRepository deliveries)
    {
        _orders = orders;
        _deliveries = deliveries;
    }

    public IReadOnlyList<PurchaseOrder> GetAll() => _orders.GetAll();

    public IReadOnlyList<PurchaseOrder> Search(string? text, PurchaseOrderStatus? status)
    {
        IEnumerable<PurchaseOrder> result = _orders.GetAll();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim();
            result = result.Where(o =>
                o.PoReference.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                (o.Site?.SiteName.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Supplier?.Name.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (status.HasValue)
            result = result.Where(o => o.Status == status.Value);

        return result.ToList();
    }

    public OrderEnquiryResult? GetDetail(string reference)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return null;
        var deliveries = _deliveries.GetForOrder(reference);
        return new OrderEnquiryResult(order, deliveries);
    }

    public IReadOnlyList<StatusSummaryRow> GetStatusSummary() =>
        _orders.GetAll()
            .GroupBy(o => o.Status)
            .Select(g => new StatusSummaryRow(g.Key, g.Count(), g.Sum(o => o.TotalValue)))
            .OrderBy(x => x.Status.ToString())
            .ToList();

    public decimal GetTotalValue() => _orders.GetAll().Sum(o => o.TotalValue);
}
