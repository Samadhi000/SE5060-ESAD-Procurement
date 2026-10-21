using Microsoft.EntityFrameworkCore;
using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>EF Core implementation of <see cref="IDeliveryRepository"/>.</summary>
public class EfDeliveryRepository : IDeliveryRepository
{
    private readonly ProcurementDbContext _db;

    public EfDeliveryRepository(ProcurementDbContext db)
    {
        _db = db;
    }

    public void Add(DeliveryNote note)
    {
        _db.DeliveryNotes.Add(note);
        _db.SaveChanges();
    }

    public IReadOnlyList<DeliveryNote> GetForOrder(string purchaseOrderReference) =>
        _db.DeliveryNotes.Include(n => n.Lines)
            .Where(n => n.PurchaseOrderReference == purchaseOrderReference)
            .OrderByDescending(n => n.DeliveryDate)
            .ToList();
}
