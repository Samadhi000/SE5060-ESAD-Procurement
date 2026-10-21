using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Models;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// The landing dashboard. The ESAD-02 design deploys this system as a phone app for the Site
/// Manager plus a web portal for the Approving Manager, so this page is the entry point to both.
/// </summary>
public class HomeController : Controller
{
    private readonly IApprovalService _approvals;
    private readonly IDeliveryService _deliveries;
    private readonly IEnquiryService _enquiry;

    public HomeController(IApprovalService approvals, IDeliveryService deliveries, IEnquiryService enquiry)
    {
        _approvals = approvals;
        _deliveries = deliveries;
        _enquiry = enquiry;
    }

    public IActionResult Index()
    {
        ViewBag.PendingCount = _approvals.GetQueue().Count;
        ViewBag.DeliverableCount = _deliveries.GetDeliverableOrders().Count;
        ViewBag.OrderCount = _enquiry.GetAll().Count;
        ViewBag.TotalValue = _enquiry.GetTotalValue();
        ViewBag.PolicyName = _approvals.PolicyName;
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
