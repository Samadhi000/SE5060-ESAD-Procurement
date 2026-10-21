namespace ProcurementApp.Services;

/// <summary>
/// OBSERVER #2 — simply writes each notification to the console. Demonstrates that multiple,
/// independent observers can react to the same event without the subject knowing about them.
/// (A real system might swap this for an SMS or e-mail observer — no other code would change.)
/// </summary>
public class ConsoleLogObserver : INotificationObserver
{
    public void OnNotify(NotificationMessage message)
    {
        Console.WriteLine($"[NOTIFY {message.Timestamp:HH:mm:ss}] to {message.Recipient} " +
                          $"re {message.OrderReference}: {message.Text}");
    }
}
