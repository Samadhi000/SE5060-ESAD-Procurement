using Microsoft.AspNetCore.Mvc;
using ProcurementApp.Services;

namespace ProcurementApp.Controllers;

/// <summary>
/// Shows the in-app notification inbox. The messages here are produced by the Observer pattern:
/// the <see cref="NotificationHub"/> (subject) forwards every event to the
/// <see cref="InAppInboxObserver"/>, which this page reads from. (Module 4.)
/// </summary>
public class NotificationsController : Controller
{
    private readonly InAppInboxObserver _inbox;

    public NotificationsController(InAppInboxObserver inbox)
    {
        _inbox = inbox;
    }

    public IActionResult Index() => View(_inbox.GetRecent());
}
