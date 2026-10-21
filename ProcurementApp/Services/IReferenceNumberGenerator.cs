namespace ProcurementApp.Services;

/// <summary>
/// Produces the unique sequential purchase-order reference the brief requires
/// (e.g. "PO-2026-04818"). Implemented as a single shared instance (Singleton) so the counter
/// is never duplicated across the app. Used by Module 1 (Create Order / Builder).
/// </summary>
public interface IReferenceNumberGenerator
{
    string NextPurchaseOrderReference();

    /// <summary>
    /// Continues numbering after the highest reference already in the database. Called once at
    /// startup: without it the in-memory counter restarts on every run and would hand out a
    /// reference that an existing order already uses.
    /// </summary>
    void ContinueAfter(int lastUsedNumber);
}

/// <summary>Thread-safe sequential generator (Singleton via DI).</summary>
public class SequentialReferenceGenerator : IReferenceNumberGenerator
{
    private const int FirstReference = 4817;   // matches the seeded sample references

    private readonly object _lock = new();
    private int _counter = FirstReference;

    public void ContinueAfter(int lastUsedNumber)
    {
        lock (_lock)
        {
            if (lastUsedNumber > _counter) _counter = lastUsedNumber;
        }
    }

    public string NextPurchaseOrderReference()
    {
        lock (_lock)
        {
            _counter++;
            return $"PO-{DateTime.Now:yyyy}-{_counter:D5}";
        }
    }
}
