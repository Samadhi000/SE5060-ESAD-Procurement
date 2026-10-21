using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Module 2 — Review and Approve / Decline / Refer a Requisition (UC2 / SD2).
/// Thin controller: it only receives the request and delegates to <see cref="IApprovalService"/>.
/// No authentication (out of scope); the acting manager is a fixed demo user.
/// Screens follow the design's pending-approvals and requisition-review wireframes.
/// </summary>
public class ApprovalController : Controller
{
    private readonly IApprovalService _approvals;

    // No login in scope — the signed-in approver is assumed to be this seeded manager.
    private const string CurrentManager = "Sarah Chen (Commercial Manager)";

    /// <summary>An order still waiting this many days after being raised is flagged overdue.</summary>
    private const int OverdueAfterDays = 5;

    public ApprovalController(IApprovalService approvals)
    {
        _approvals = approvals;
    }

    // GET /Approval  → Pending Approvals list (pending-approvals.png)
    public IActionResult Index(string? filter)
    {
        var queue = _approvals.GetQueue();

        ViewBag.Filter = filter ?? "all";
        ViewBag.PolicyName = _approvals.PolicyName;
        ViewBag.Counts = new Dictionary<string, int>
        {
            ["all"] = queue.Count,
            ["waiting"] = queue.Count(o => o.Status == PurchaseOrderStatus.WaitingForApproval),
            ["referred"] = queue.Count(o => o.Status == PurchaseOrderStatus.Referred),
            ["overdue"] = queue.Count(IsOverdue)
        };
        ViewBag.PolicyFor = queue.ToDictionary(o => o.PoReference, o => _approvals.EvaluatePolicy(o));
        ViewBag.OverdueRefs = queue.Where(IsOverdue).Select(o => o.PoReference).ToHashSet();

        var shown = filter switch
        {
            "waiting" => queue.Where(o => o.Status == PurchaseOrderStatus.WaitingForApproval).ToList(),
            "referred" => queue.Where(o => o.Status == PurchaseOrderStatus.Referred).ToList(),
            "overdue" => queue.Where(IsOverdue).ToList(),
            _ => queue.ToList()
        };

        return View(shown);
    }

    // GET /Approval/Details/PO-2026-04817  → decision screen (requisition-review.png)
    public IActionResult Details(string id)
    {
        var order = _approvals.Get(id);
        if (order is null) return NotFound();
        ViewBag.PolicyResult = _approvals.EvaluatePolicy(order);
        ViewBag.CurrentManager = CurrentManager;
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Approve(string id, string? comment)
    {
        var result = _approvals.Approve(id, CurrentManager, comment);
        TempData["Message"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Decline(string id, string comment)
    {
        var result = _approvals.Decline(id, CurrentManager, comment ?? string.Empty);
        TempData["Message"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Refer(string id, string comment)
    {
        var result = _approvals.Refer(id, CurrentManager, comment ?? string.Empty);
        TempData["Message"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Approval/ApplyPolicy → runs the Strategy over all pending orders
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyPolicy()
    {
        var count = _approvals.ApplyPolicyAutoApprovals("System (policy)");
        TempData["Message"] = $"Approval policy applied: {count} order(s) auto-approved.";
        return RedirectToAction(nameof(Index));
    }

    private static bool IsOverdue(PurchaseOrder order) =>
        order.Status == PurchaseOrderStatus.WaitingForApproval &&
        (DateTime.Now - order.DateCreated).TotalDays > OverdueAfterDays;
}
