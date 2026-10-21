using Microsoft.EntityFrameworkCore;
using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>EF Core implementation of <see cref="IReferenceDataRepository"/>.</summary>
public class EfReferenceDataRepository : IReferenceDataRepository
{
    private readonly ProcurementDbContext _db;

    public EfReferenceDataRepository(ProcurementDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<Site> GetSites() => _db.Sites.OrderBy(s => s.SiteName).ToList();

    public IReadOnlyList<Supplier> GetSuppliers() =>
        _db.Suppliers.Include(s => s.Depots).OrderBy(s => s.Name).ToList();

    public Site? GetSite(int id) => _db.Sites.Find(id);

    public Supplier? GetSupplier(int id) =>
        _db.Suppliers.Include(s => s.Depots).FirstOrDefault(s => s.Id == id);

    public IReadOnlyList<CatalogueItem> GetCatalogueForSupplier(string supplierBusinessId) =>
        _db.CatalogueItems.Include(c => c.Item)
            .Where(c => c.SupplierId == supplierBusinessId)
            .OrderBy(c => c.Item.Description)
            .ToList();
}
