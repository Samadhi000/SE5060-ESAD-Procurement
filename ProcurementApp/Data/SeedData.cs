using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Data;

/// <summary>Everything needed to populate a fresh database for the demo.</summary>
public class SeedBundle
{
    public List<Site> Sites { get; } = new();
    public List<Supplier> Suppliers { get; } = new();
    public List<Item> Items { get; } = new();
    public List<CatalogueItem> Catalogue { get; } = new();
    public List<Employee> Employees { get; } = new();
    public List<PurchaseOrder> Orders { get; } = new();
}

/// <summary>
/// Builds reference data (sites, suppliers, priced catalogue, staff) and a set of sample
/// purchase orders. Order values deliberately straddle the Rs.100,000 threshold to show the
/// Approval Strategy pattern working.
/// </summary>
public static class SeedData
{
    public static SeedBundle Build()
    {
        var bundle = new SeedBundle();

        // --- Staff ---
        var sarah = new SiteManager { EmployeeId = "SM01", Name = "Sarah Chen", Email = "sarah@sitepro.lk", ContactNumber = "077-1234567" };
        var david = new SiteManager { EmployeeId = "SM02", Name = "David Vance", Email = "david@sitepro.lk", ContactNumber = "077-7654321" };
        bundle.Employees.Add(sarah);
        bundle.Employees.Add(david);

        // --- Items ---
        var cement = new Item { ItemCode = "CEM-25", Description = "Portland Cement 25kg", UnitOfMeasure = "bag", Category = "Cement" };
        var rebar = new Item { ItemCode = "RB-T12", Description = "High-Tensile Rebar T12", UnitOfMeasure = "length", Category = "Steel" };
        var concrete = new Item { ItemCode = "RMX-C40", Description = "C40/50 Ready-Mix Concrete", UnitOfMeasure = "m3", Category = "Concrete" };
        var timber = new Item { ItemCode = "TMB-3M", Description = "Timber Joists 3m", UnitOfMeasure = "piece", Category = "Timber" };
        var goggles = new Item { ItemCode = "SG-01", Description = "Standard Safety Goggles", UnitOfMeasure = "unit", Category = "Safety" };
        bundle.Items.AddRange(new[] { cement, rebar, concrete, timber, goggles });

        // --- Sites ---
        var london = new Site { SiteId = "S01", SiteName = "London Gateway Hub", Address = "Plot 4B, London Gateway", SiteContactPerson = "Sarah Chen", SiteContactNumber = "077-1234567" };
        var westfield = new Site { SiteId = "S02", SiteName = "Westfield Phase 2", Address = "Westfield Rd", SiteContactPerson = "David Vance", SiteContactNumber = "077-7654321" };
        var manchester = new Site { SiteId = "S03", SiteName = "Manchester Arena Phase 2", Address = "Arena Way", SiteContactPerson = "Lucy Gray", SiteContactNumber = "077-5557777" };
        bundle.Sites.AddRange(new[] { london, westfield, manchester });

        // --- Suppliers (with depots) ---
        var cemex = new Supplier
        {
            SupplierId = "SUP01", Name = "Cemex Concrete Ltd", ContactDetails = "orders@cemex.lk", Status = "Active",
            Depots = { new Depot { DepotId = "D01", Location = "Colombo Depot" }, new Depot { DepotId = "D02", Location = "Kandy Depot" } }
        };
        var buildMat = new Supplier
        {
            SupplierId = "SUP02", Name = "BuildMaterials Ltd", ContactDetails = "sales@buildmat.lk", Status = "Active",
            Depots = { new Depot { DepotId = "D03", Location = "Gampaha Depot" } }
        };
        var brc = new Supplier { SupplierId = "SUP03", Name = "BRC Rebar", ContactDetails = "info@brcrebar.lk", Status = "Active" };
        bundle.Suppliers.AddRange(new[] { cemex, buildMat, brc });

        // --- Priced catalogue (which supplier sells what, at what agreed price) ---
        void Price(Supplier s, Item i, decimal p) =>
            bundle.Catalogue.Add(new CatalogueItem { SupplierId = s.SupplierId, Item = i, AgreedPrice = p, EffectiveFrom = DateTime.Now.AddMonths(-3) });

        Price(cemex, concrete, 95m); Price(cemex, cement, 15m); Price(cemex, goggles, 15m);
        Price(buildMat, cement, 14.50m); Price(buildMat, timber, 110m); Price(buildMat, goggles, 15m);
        Price(brc, rebar, 35.30m); Price(brc, timber, 110m);

        // --- Sample orders (values straddle Rs.100,000 for the Strategy demo) ---
        bundle.Orders.Add(MakeOrder("PO-2026-04817", london, cemex, sarah, DateTime.Now.AddDays(-2),
            (concrete, 24, 95m), (timber, 1, 120m)));                              // ~Rs.2,400 - auto
        bundle.Orders.Add(MakeOrder("PO-2026-04792", manchester, brc, david, DateTime.Now.AddDays(-3),
            (rebar, 400, 35.30m)));                                                // Rs.14,120 - auto
        bundle.Orders.Add(MakeOrder("PO-2026-04780", westfield, buildMat, sarah, DateTime.Now.AddDays(-4),
            (cement, 100, 14.50m)));                                               // Rs.1,450 - auto
        bundle.Orders.Add(MakeOrder("PO-2026-04751", london, cemex, david, DateTime.Now.AddDays(-5),
            (concrete, 1500, 95m), (cement, 3000, 15m)));                          // Rs.187,500 - needs approval
        bundle.Orders.Add(MakeOrder("PO-2026-04712", manchester, brc, sarah, DateTime.Now.AddDays(-6),
            (rebar, 5000, 95m), (timber, 1000, 110m)));                            // Rs.585,000 - needs approval

        return bundle;
    }

    /// <summary>Kept for the in-memory repository fallback.</summary>
    public static List<PurchaseOrder> CreateSampleOrders() => Build().Orders;

    private static PurchaseOrder MakeOrder(
        string reference, Site site, Supplier supplier, SiteManager createdBy,
        DateTime created, params (Item item, int qty, decimal price)[] lines)
    {
        var order = new PurchaseOrder
        {
            PoReference = reference,
            Site = site,
            Supplier = supplier,
            CreatedBy = createdBy,
            DeliveryAddress = site.Address,
            DateCreated = created,
            RequiredByDate = created.AddDays(14),
            Status = PurchaseOrderStatus.WaitingForApproval
        };

        var n = 1;
        foreach (var (item, qty, price) in lines)
            order.Lines.Add(new PurchaseOrderLine { LineNumber = n++, Item = item, QuantityOrdered = qty, UnitPrice = price });

        return order;
    }
}
