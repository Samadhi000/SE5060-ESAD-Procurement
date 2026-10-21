using Microsoft.EntityFrameworkCore;

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
}
