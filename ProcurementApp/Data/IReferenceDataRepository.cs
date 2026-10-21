using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>
/// Read access to the master/reference data (sites, suppliers, depots, priced catalogue) that a
/// Site Manager picks from when creating an order (UC1). This data is assumed to be maintained by
/// Procurement staff beforehand (see the design's assumptions).
/// </summary>
public interface IReferenceDataRepository
{
    IReadOnlyList<Site> GetSites();
    IReadOnlyList<Supplier> GetSuppliers();
    Site? GetSite(int id);
    Supplier? GetSupplier(int id);

    /// <summary>A supplier depot, used for the optional "Depot" step of the order wizard.</summary>
    Depot? GetDepot(int id);

    /// <summary>
    /// The Site Manager acting in the app. Authentication is out of scope per the design's own
    /// assumptions, so the demo always acts as the first seeded Site Manager (Sarah Chen).
    /// </summary>
    SiteManager? GetActingSiteManager();

    /// <summary>The priced catalogue items offered by a given supplier.</summary>
    IReadOnlyList<CatalogueItem> GetCatalogueForSupplier(string supplierBusinessId);
}
