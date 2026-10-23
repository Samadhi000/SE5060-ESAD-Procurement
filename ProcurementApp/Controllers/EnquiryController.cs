using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Data;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>Module 4: enquire about and view orders, plus reports. Index/Details are the phone views; Portal is the desktop one.</summary>
public class EnquiryController : Controller
{
    private readonly IEnquiryService _service;
    private readonly IReferenceDataRepository _reference;

    public EnquiryController(IEnquiryService service, IReferenceDataRepository reference)
    {
        _service = service;
        _reference = reference;
    }

    // Phone: My Orders list with search and status chips.
    public IActionResult Index(string? q, PurchaseOrderStatus? status)
    {
        ViewBag.Query = q;
        ViewBag.Status = status;
        return View(_service.Search(q, status));
    }

    // Phone: order detail with Details / Approval History / Deliveries tabs.
    public IActionResult Details(string id, string? tab)
    {
        var result = _service.GetDetail(id);
        if (result is null) return NotFound();

        // Not "Tab": the mobile layout reserves ViewData["Tab"] for the bottom tab bar.
        ViewBag.DetailTab = string.IsNullOrEmpty(tab) ? "details" : tab;
        return View(result);
    }

    // Desktop: Order Enquiry workspace with filters, result list and a detail pane.
    public IActionResult Portal(string? q, PurchaseOrderStatus? status, int? siteId, int? supplierId, string? selected)
    {
        var results = _service.Search(q, status);

        if (siteId is int s && s > 0)
            results = results.Where(o => o.Site?.Id == s).ToList();
        if (supplierId is int sup && sup > 0)
            results = results.Where(o => o.Supplier?.Id == sup).ToList();

        ViewBag.Query = q;
        ViewBag.Status = status;
        ViewBag.SiteId = siteId;
        ViewBag.SupplierId = supplierId;
        ViewBag.Sites = _reference.GetSites();
        ViewBag.Suppliers = _reference.GetSuppliers();

        var reference = selected ?? results.FirstOrDefault()?.PoReference;
        ViewBag.Selected = reference is null ? null : _service.GetDetail(reference);

        return View(results);
    }

    public IActionResult Reports()
    {
        ViewBag.Total = _service.GetTotalValue();
        return View(_service.GetStatusSummary());
    }
}
