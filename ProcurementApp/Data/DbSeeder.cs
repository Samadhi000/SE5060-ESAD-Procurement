using Microsoft.EntityFrameworkCore;
using ProcurementApp.Services;

namespace ProcurementApp.Data;

/// <summary>
/// Creates the SQLite database (if it doesn't exist) and fills it with the sample data the
/// first time the app runs. Called once at startup from Program.cs.
/// </summary>
public static class DbSeeder
{
    public static void Initialise(ProcurementDbContext db)
    {
        db.Database.EnsureCreated();          // create the .db file + tables from the model

        if (db.PurchaseOrders.Any())          // already seeded — leave existing data alone
            return;

        var data = SeedData.Build();
        db.Sites.AddRange(data.Sites);
        db.Suppliers.AddRange(data.Suppliers);
        db.Items.AddRange(data.Items);
        db.CatalogueItems.AddRange(data.Catalogue);
        db.Employees.AddRange(data.Employees);
        db.PurchaseOrders.AddRange(data.Orders);
        db.SaveChanges();
    }

    /// <summary>
    /// Points the reference generator at the highest PO number already stored. The generator is
    /// an in-memory Singleton, so without this it restarts from its seed value on every run and
    /// would hand out a reference that an existing order already uses.
    /// </summary>
    public static void ResumeReferenceNumbering(ProcurementDbContext db, IReferenceNumberGenerator generator)
    {
        var highest = db.PurchaseOrders
            .Select(o => o.PoReference)
            .AsEnumerable()                   // the parsing below runs in memory, not in SQL
            .Select(ParseTrailingNumber)
            .DefaultIfEmpty(0)
            .Max();

        generator.ContinueAfter(highest);
    }

    /// <summary>"PO-2026-04818" → 4818; anything unparseable counts as 0.</summary>
    private static int ParseTrailingNumber(string reference)
    {
        var tail = reference.Split('-').LastOrDefault();
        return int.TryParse(tail, out var number) ? number : 0;
    }
}
