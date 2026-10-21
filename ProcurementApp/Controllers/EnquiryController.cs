using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 4 — Enquire About / View Orders + Reports (UC4). Read-only screens. The notification
/// side of this module uses the Observer pattern (see <see cref="NotificationsController"/>).
/// </summary>
public class EnquiryController : Controller
{
    private readonly IEnquiryService _service;

    public EnquiryController(IEnquiryService service)
    {
        _service = service;
    }

    // My Orders list with search + status filter (wireframes Figs 13-14)
    public IActionResult Index(string? q, PurchaseOrderStatus? status)
    {
        ViewBag.Query = q;
        ViewBag.Status = status;
        return View(_service.Search(q, status));
    }

    // Full order detail: lines + approval history + delivery history (wireframe Fig 13, detail)
    public IActionResult Details(string id)
    {
        var result = _service.GetDetail(id);
        if (result is null) return NotFound();
        return View(result);
    }

    // Management reports
    public IActionResult Reports()
    {
        ViewBag.Total = _service.GetTotalValue();
        return View(_service.GetStatusSummary());
    }
}
