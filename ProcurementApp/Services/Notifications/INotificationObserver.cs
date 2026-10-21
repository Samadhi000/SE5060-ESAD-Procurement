namespace ProcurementApp.Services;

/// <summary>A single notification raised by the system.</summary>
public record NotificationMessage(string Recipient, string Text, string OrderReference, DateTime Timestamp);

/// <summary>
/// OBSERVER PATTERN — the observer interface. Anything that wants to react when the system
/// raises a notification (store it, log it, send an SMS…) implements this and subscribes to
/// the <see cref="NotificationHub"/> (the subject).
/// </summary>
public interface INotificationObserver
{
    void OnNotify(NotificationMessage message);
}
