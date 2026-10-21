using ProcurementApp.Domain;

namespace ProcurementApp.Services;

/// <summary>
/// Sends notifications when an order changes (e.g. approved, declined, placed).
/// Module 2 depends only on this interface. Module 4 will replace the simple stub below
/// with a real Observer-pattern notification service — without changing the approval code.
/// </summary>
public interface INotificationService
{
    void Notify(string recipient, string message, PurchaseOrder order);
}
