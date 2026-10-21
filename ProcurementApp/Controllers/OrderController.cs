using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 1 — Create &amp; Submit a Purchase Order (UC1 / SD1). Uses the Builder pattern
/// (inside <see cref="IOrderCreationService"/>) to assemble the order step by step.
/// </summary>
public class OrderController : Controller
{
    private readonly IOrderCreationService _service;

    public OrderController(IOrderCreationService service)
    {
        _service = service;
    }

    // Step 1 — choose site, supplier and required-by date (wireframe Fig 9, screen 1)
    public IActionResult Create()
    {
        ViewBag.Sites = _service.GetSites();
        ViewBag.Suppliers = _service.GetSuppliers();
        return View();
    }

    // Step 2 — add catalogue items for the chosen supplier (wireframe Fig 9, screen 2)
    public IActionResult AddItems(int siteId, int supplierId, DateTime requiredBy)
    {
        var supplier = _service.GetSuppliers().FirstOrDefault(s => s.Id == supplierId);
        var site = _service.GetSites().FirstOrDefault(s => s.Id == siteId);
        if (supplier is null || site is null) return RedirectToAction(nameof(Create));

        ViewBag.Site = site;
        ViewBag.Supplier = supplier;
        ViewBag.RequiredBy = requiredBy == default ? DateTime.Now.AddDays(14) : requiredBy;
        return View(_service.GetCatalogue(supplier.SupplierId));
    }

    // Submit — the Builder validates and the order is saved (wireframe Fig 9, screen 3)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int siteId, int supplierId, DateTime requiredBy,
                                string[] itemCode, int[] quantity)
    {
        var lines = new List<OrderLineRequest>();
        for (var i = 0; i < itemCode.Length; i++)
        {
            var qty = i < quantity.Length ? quantity[i] : 0;
            if (qty > 0) lines.Add(new OrderLineRequest(itemCode[i], qty));
        }

        var result = _service.Submit(siteId, supplierId, requiredBy, lines);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(AddItems), new { siteId, supplierId, requiredBy });
        }

        TempData["Reference"] = result.Reference;
        return RedirectToAction(nameof(Confirmation));
    }

    public IActionResult Confirmation()
    {
        ViewBag.Reference = TempData["Reference"];
        return View();
    }
}
