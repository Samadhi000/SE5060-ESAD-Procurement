using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>
/// Read access to the master/reference data (sites, suppliers, priced catalogue) that a Site
/// Manager picks from when creating an order (UC1). This data is assumed to be maintained by
/// Procurement staff beforehand (see the design's assumptions).
/// </summary>
public interface IReferenceDataRepository
{
    IReadOnlyList<Site> GetSites();
    IReadOnlyList<Supplier> GetSuppliers();
    Site? GetSite(int id);
    Supplier? GetSupplier(int id);

    /// <summary>The priced catalogue items offered by a given supplier.</summary>
    IReadOnlyList<CatalogueItem> GetCatalogueForSupplier(string supplierBusinessId);
}
