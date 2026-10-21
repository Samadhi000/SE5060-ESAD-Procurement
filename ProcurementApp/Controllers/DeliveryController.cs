using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 3 — Log Delivery / Goods Receipt (UC3 / SD3). Uses the State pattern (inside
/// <see cref="IDeliveryService"/>) to decide whether a delivery is allowed and what status
/// the order moves to afterwards. Screens follow the design's order-detail-deliveries,
/// record-delivery and delivery-confirmation wireframes.
/// </summary>
public class DeliveryController : Controller
{
    private readonly IDeliveryService _service;
    private const string ReceivedBy = "Sarah Chen (Site Manager)";

    public DeliveryController(IDeliveryService service)
    {
        _service = service;
    }

    // Orders that can still receive deliveries.
    public IActionResult Index() => View(_service.GetDeliverableOrders());

    // Per-order outstanding view with a progress bar per line (order-detail-deliveries).
    public IActionResult Items(string id)
    {
        var order = _service.GetOrder(id);
        if (order is null) return NotFound();
        return View(order);
    }

    // Record-delivery form for one order (record-delivery).
    public IActionResult Record(string id)
    {
        var order = _service.GetOrder(id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Record(string id, string supplierReference, DateTime deliveryDate,
                                int[] lineNumber, string[] itemDescription, int[] quantityDelivered, string[] condition)
    {
        var lines = new List<DeliveryLineRequest>();
        for (var i = 0; i < lineNumber.Length; i++)
        {
            var qty = i < quantityDelivered.Length ? quantityDelivered[i] : 0;
            if (qty > 0)
                lines.Add(new DeliveryLineRequest(
                    lineNumber[i],
                    i < itemDescription.Length ? itemDescription[i] : string.Empty,
                    qty,
                    i < condition.Length ? condition[i] : "Good"));
        }

        if (lines.Count == 0)
        {
            TempData["Error"] = "Enter a received quantity for at least one item.";
            return RedirectToAction(nameof(Record), new { id });
        }

        var result = _service.RecordDelivery(id, supplierReference ?? string.Empty,
            deliveryDate == default ? DateTime.Now : deliveryDate, lines, ReceivedBy);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Record), new { id });
        }

        // Carry a short log summary through to the confirmation screen.
        TempData["Reference"] = id;
        TempData["Status"] = result.NewStatus?.ToString();
        TempData["Message"] = result.Message;
        TempData["Summary"] = string.Join("|", (result.Recorded ?? new List<DeliveryLineRequest>())
            .Select(l => $"{l.ItemDescription} — {l.QuantityDelivered} received"));

        return RedirectToAction(nameof(Confirmation));
    }

    // "Delivery recorded" success screen (delivery-confirmation).
    public IActionResult Confirmation()
    {
        var reference = TempData["Reference"] as string;
        if (string.IsNullOrEmpty(reference)) return RedirectToAction(nameof(Index));

        ViewBag.Reference = reference;
        ViewBag.Status = TempData["Status"] as string;
        ViewBag.Message = TempData["Message"] as string;
        ViewBag.Summary = (TempData["Summary"] as string ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries);

        return View();
    }
}
