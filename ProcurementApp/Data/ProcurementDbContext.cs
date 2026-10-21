using Microsoft.EntityFrameworkCore;
using ProcurementApp.Domain;

namespace ProcurementApp.Data;

/// <summary>
/// The Entity Framework Core database context — the bridge between our C# classes and the
/// SQLite database file. Each DbSet becomes a table; EF creates the schema from the classes.
/// </summary>
public class ProcurementDbContext : DbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options) { }

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<CatalogueItem> CatalogueItems => Set<CatalogueItem>();
    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Store enums as readable text in the DB (e.g. "WaitingForApproval") instead of numbers.
        b.Entity<PurchaseOrder>().Property(p => p.Status).HasConversion<string>();
        b.Entity<Approval>().Property(a => a.Decision).HasConversion<string>();

        // The person who created an order is a SiteManager (a kind of Employee).
        b.Entity<PurchaseOrder>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey("CreatedById");

        base.OnModelCreating(b);
    }
}
