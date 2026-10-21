namespace ProcurementApp.Services;

/// <summary>
/// OBSERVER #1 — keeps notifications in an in-app inbox so they can be shown on the
/// Notifications page (the bell icon). Registered as a Singleton so the inbox is shared.
/// </summary>
public class InAppInboxObserver : INotificationObserver
{
    private readonly object _lock = new();
    private readonly List<NotificationMessage> _inbox = new();

    public void OnNotify(NotificationMessage message)
    {
        lock (_lock) { _inbox.Insert(0, message); }   // newest first
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
