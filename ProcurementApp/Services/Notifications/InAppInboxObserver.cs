namespace ProcurementApp.Services;

/// <summary>Observer that keeps notifications in an in-app inbox for the Notifications page.</summary>
public class InAppInboxObserver : INotificationObserver
{
    private readonly object _lock = new();
    private readonly List<NotificationMessage> _inbox = new();

    public void OnNotify(NotificationMessage message)
    {
        lock (_lock) { _inbox.Insert(0, message); }
    }

    public IReadOnlyList<NotificationMessage> GetRecent(int max = 50)
    {
        lock (_lock) { return _inbox.Take(max).ToList(); }
    }

    public int Count
    {
        get { lock (_lock) { return _inbox.Count; } }
    }
}
