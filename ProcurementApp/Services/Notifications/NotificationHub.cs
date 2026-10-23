using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>Observer pattern (subject): forwards each order event to every subscribed observer, without knowing who listens.</summary>
public class NotificationHub : INotificationService
{
    private readonly List<INotificationObserver> _observers = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public void Subscribe(INotificationObserver observer) => _observers.Add(observer);

    public void Notify(string recipient, string message, PurchaseOrder order)
    {
        var msg = new NotificationMessage(recipient, message, order.PoReference, DateTime.Now);
        _logger.LogInformation("Notifying {Count} observer(s): [{Recipient}] {Message}",
            _observers.Count, recipient, message);

        foreach (var observer in _observers)
            observer.OnNotify(msg);
    }
}
