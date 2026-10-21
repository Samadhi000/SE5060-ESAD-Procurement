namespace ProcurementApp.Services;

/// <summary>
/// Produces the unique sequential purchase-order reference the brief requires
/// (e.g. "PO-2026-04818"). Implemented as a single shared instance (Singleton) so the counter
/// is never duplicated across the app. Used by Module 1 (Create Order / Builder).
/// </summary>
public interface IReferenceNumberGenerator
{
    string NextPurchaseOrderReference();
}

/// <summary>Thread-safe in-memory sequential generator (Singleton via DI).</summary>
public class SequentialReferenceGenerator : IReferenceNumberGenerator
{
    private readonly object _lock = new();
    private int _counter = 4817;   // continues after the seeded sample references

    public string NextPurchaseOrderReference()
    {
        lock (_lock)
        {
            _counter++;
            return $"PO-{DateTime.Now:yyyy}-{_counter:D5}";
        }
    }
}
