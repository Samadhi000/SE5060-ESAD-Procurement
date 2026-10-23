namespace ProcurementApp.Services;

/// <summary>Observer that writes each notification to the console.</summary>
public class ConsoleLogObserver : INotificationObserver
{
    public void OnNotify(NotificationMessage message)
    {
        Console.WriteLine($"[NOTIFY {message.Timestamp:HH:mm:ss}] to {message.Recipient} " +
                          $"re {message.OrderReference}: {message.Text}");
    }
}
