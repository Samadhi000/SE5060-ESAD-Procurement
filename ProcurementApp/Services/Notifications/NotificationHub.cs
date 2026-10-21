using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>
/// OBSERVER PATTERN — the "subject". Other modules call <see cref="Notify"/> when an order
/// changes (approved, declined, placed, delivered). The hub doesn't know or care who listens —
/// it just forwards the event to every subscribed observer. New listeners can be added without
/// changing any calling code. This is the real implementation of <see cref="INotificationService"/>
/// that replaces the earlier stub.
/// </summary>
public class NotificationHub : INotificationService
{
    private readonly List<INotificationObserver> _observers = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>Register an observer that will be told about every future notification.</summary>
    public void Subscribe(INotificationObserver observer) => _observers.Add(observer);

    public void Notify(string recipient, string message, PurchaseOrder order)
    {
        var msg = new NotificationMessage(recipient, message, order.PoReference, DateTime.Now);
        _logger.LogInformation("Notifying {Count} observer(s): [{Recipient}] {Message}",
            _observers.Count, recipient, message);

        foreach (var observer in _observers)
            observer.OnNotify(msg);     // each observer reacts in its own way
    }
}
