using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Data;

/// <summary>
/// Abstraction over purchase-order storage. Modules depend on THIS interface, never on a
/// concrete database. Today it is backed by <see cref="InMemoryPurchaseOrderRepository"/>
/// (test data, no DB needed); later it will be swapped for an EF Core + SQLite version
/// WITHOUT any change to the modules that use it. (Dependency Inversion / Repository pattern.)
/// </summary>
public interface IPurchaseOrderRepository
{
    IReadOnlyList<PurchaseOrder> GetAll();
    PurchaseOrder? GetByReference(string poReference);
    IReadOnlyList<PurchaseOrder> GetByStatus(PurchaseOrderStatus status);
    void Add(PurchaseOrder order);
    void Update(PurchaseOrder order);
}
