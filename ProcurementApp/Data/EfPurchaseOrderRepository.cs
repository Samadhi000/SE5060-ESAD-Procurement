using Microsoft.EntityFrameworkCore;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Data;

/// <summary>
/// REPOSITORY PATTERN — the real database-backed implementation of
/// <see cref="IPurchaseOrderRepository"/>, using EF Core + SQLite. Because every module
/// depends on the interface (not on this class), we could swap in-memory ↔ database with a
/// one-line change in Program.cs and nothing else in the app changes.
/// </summary>
public class EfPurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ProcurementDbContext _db;

    public EfPurchaseOrderRepository(ProcurementDbContext db)
    {
        _db = db;
    }

    // Load each order together with everything we display (lines, items, approvals, site, supplier).
    private IQueryable<PurchaseOrder> OrdersWithDetails() =>
        _db.PurchaseOrders
            .Include(o => o.Site)
            .Include(o => o.Supplier)
            .Include(o => o.CreatedBy)
            .Include(o => o.Lines).ThenInclude(l => l.Item)
            .Include(o => o.Approvals)
            .Include(o => o.Comments);

    public IReadOnlyList<PurchaseOrder> GetAll() =>
        OrdersWithDetails().OrderByDescending(o => o.DateCreated).ToList();

    public PurchaseOrder? GetByReference(string poReference) =>
        OrdersWithDetails().FirstOrDefault(o => o.PoReference == poReference);

    public IReadOnlyList<PurchaseOrder> GetByStatus(PurchaseOrderStatus status) =>
        OrdersWithDetails().Where(o => o.Status == status)
            .OrderByDescending(o => o.DateCreated).ToList();

    public void Add(PurchaseOrder order)
    {
        _db.PurchaseOrders.Add(order);
        _db.SaveChanges();
    }

    public void Update(PurchaseOrder order)
    {
        _db.PurchaseOrders.Update(order);
        _db.SaveChanges();
    }
}
