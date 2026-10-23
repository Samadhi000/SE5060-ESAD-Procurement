namespace ProcurementApp.Services;

/// <summary>A single notification raised by the system.</summary>
public record NotificationMessage(string Recipient, string Text, string OrderReference, DateTime Timestamp);

/// <summary>Observer pattern: implemented by anything that reacts to a notification, then subscribes to the NotificationHub.</summary>
public interface INotificationObserver
{
    void OnNotify(NotificationMessage message);
}
