using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Data;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 4 — Enquire About / View Orders + Reports (UC4). Read-only screens. The notification
/// side of this module uses the Observer pattern (see <see cref="NotificationsController"/>).
///
/// The design provides both a phone view (my-orders-mobile, order-detail-mobile) and an office
/// view (order-enquiry-desktop), so both are implemented: Index/Details are the mobile screens,
/// Portal is the desktop one.
/// </summary>
public class EnquiryController : Controller
{
    private readonly IEnquiryService _service;
    private readonly IReferenceDataRepository _reference;

    public EnquiryController(IEnquiryService service, IReferenceDataRepository reference)
    {
        _service = service;
        _reference = reference;
    }

    // MOBILE — My Orders list with search + status chips (my-orders-mobile).
    public IActionResult Index(string? q, PurchaseOrderStatus? status)
    {
        ViewBag.Query = q;
        ViewBag.Status = status;
        return View(_service.Search(q, status));
    }

    // MOBILE — order detail with Details / Approval History / Deliveries tabs (order-detail-mobile).
    public IActionResult Details(string id, string? tab)
    {
        var result = _service.GetDetail(id);
        if (result is null) return NotFound();

        // NOTE: not "Tab" — the mobile layout reserves ViewData["Tab"] for the bottom tab bar.
        ViewBag.DetailTab = string.IsNullOrEmpty(tab) ? "details" : tab;
        return View(result);
    }

    // DESKTOP — Order Enquiry workspace: filters, result list, detail pane (order-enquiry-desktop).
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

        // The right-hand pane shows whichever order is selected, defaulting to the first result.
        var reference = selected ?? results.FirstOrDefault()?.PoReference;
        ViewBag.Selected = reference is null ? null : _service.GetDetail(reference);

        return View(results);
    }

    // Management reports.
    public IActionResult Reports()
    {
        ViewBag.Total = _service.GetTotalValue();
        return View(_service.GetStatusSummary());
    }
}
