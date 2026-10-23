using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>Module 1: create and submit a purchase order through a three-step wizard. Uses the Builder.</summary>
public class OrderController : Controller
{
    private readonly IOrderCreationService _service;

    public OrderController(IOrderCreationService service)
    {
        _service = service;
    }

    // Step 1: choose site, supplier, optional depot and required-by date.
    public IActionResult Create()
    {
        ViewBag.Sites = _service.GetSites();
        ViewBag.Suppliers = _service.GetSuppliers();
        return View();
    }

    // Step 2: add catalogue items for the chosen supplier.
    public IActionResult AddItems(int siteId, int supplierId, int? depotId, DateTime requiredBy)
    {
        var supplier = _service.GetSuppliers().FirstOrDefault(s => s.Id == supplierId);
        var site = _service.GetSites().FirstOrDefault(s => s.Id == siteId);
        if (supplier is null || site is null) return RedirectToAction(nameof(Create));

        ViewBag.Site = site;
        ViewBag.Supplier = supplier;
        ViewBag.DepotId = depotId;
        ViewBag.RequiredBy = requiredBy == default ? DateTime.Now.AddDays(14) : requiredBy;
        return View(_service.GetCatalogue(supplier.SupplierId));
    }

    // Step 3: review the assembled order (shown as an unsaved draft) before submitting.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Review(int siteId, int supplierId, int? depotId, DateTime requiredBy,
                                string[] itemCode, int[] quantity)
    {
        var request = ToDraftRequest(siteId, supplierId, depotId, requiredBy, itemCode, quantity);

        if (request.Lines.Count == 0)
        {
            TempData["Error"] = "Add a quantity for at least one item before continuing.";
            return RedirectToAction(nameof(AddItems), new { siteId, supplierId, depotId, requiredBy });
        }

        var draft = _service.BuildDraftForReview(request);
        if (draft is null) return RedirectToAction(nameof(Create));

        ViewBag.Request = request;
        return View(draft);
    }

    // Submit: the Builder validates and the order is saved.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int siteId, int supplierId, int? depotId, DateTime requiredBy,
                                string[] itemCode, int[] quantity, string? comments)
    {
        var request = ToDraftRequest(siteId, supplierId, depotId, requiredBy, itemCode, quantity);

        var result = _service.Submit(request, comments);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(AddItems), new { siteId, supplierId, depotId, requiredBy });
        }

        TempData["Reference"] = result.Reference;
        return RedirectToAction(nameof(Confirmation));
    }

    public IActionResult Confirmation()
    {
        var reference = TempData["Reference"] as string;
        if (string.IsNullOrEmpty(reference)) return RedirectToAction(nameof(Create));

        ViewBag.Reference = reference;
        return View();
    }

    // Pairs the parallel itemCode[]/quantity[] form arrays into line requests.
    private static OrderDraftRequest ToDraftRequest(int siteId, int supplierId, int? depotId,
                                                    DateTime requiredBy, string[] itemCode, int[] quantity)
    {
        var lines = new List<OrderLineRequest>();
        for (var i = 0; i < itemCode.Length; i++)
        {
            var qty = i < quantity.Length ? quantity[i] : 0;
            if (qty > 0) lines.Add(new OrderLineRequest(itemCode[i], qty));
        }

        return new OrderDraftRequest(siteId, supplierId, depotId, requiredBy, lines);
    }
}
