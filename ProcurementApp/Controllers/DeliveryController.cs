using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 3 — Log Delivery / Goods Receipt (UC3 / SD3). Uses the State pattern (inside
/// <see cref="IDeliveryService"/>) to decide whether a delivery is allowed and what status
/// the order moves to afterwards.
/// </summary>
public class DeliveryController : Controller
{
    private readonly IDeliveryService _service;
    private const string ReceivedBy = "Sarah Chen (Site Manager)";

    public DeliveryController(IDeliveryService service)
    {
        _service = service;
    }

    // List of orders that can still receive deliveries (wireframe Fig 12, screen 1)
    public IActionResult Index() => View(_service.GetDeliverableOrders());

    // Record-delivery form for one order (wireframe Fig 12, screen 2)
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

        var result = _service.RecordDelivery(id, supplierReference ?? string.Empty,
            deliveryDate == default ? DateTime.Now : deliveryDate, lines, ReceivedBy);

        TempData[result.Success ? "Message" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
