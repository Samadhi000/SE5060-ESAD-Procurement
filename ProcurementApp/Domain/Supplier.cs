using System.ComponentModel.DataAnnotations;

namespace ProcurementApp.Domain;

/// <summary>An approved supplier that orders can be placed with (ESAD-02 class diagram: Supplier).</summary>
public class Supplier
{
    [Key] public int Id { get; set; }

    public string SupplierId { get; set; } = string.Empty;    // business id, e.g. SUP01
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactDetails { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    /// <summary>A supplier may operate from multiple depots (brief: "handle multiple depots for the same supplier").</summary>
    public List<Depot> Depots { get; set; } = new();

    public List<Depot> GetAvailableDepots() => Depots;
    public bool IsAvailable() => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);
}

/// <summary>A depot belonging to a supplier.</summary>
public class Depot
{
    [Key] public int Id { get; set; }

    public string DepotId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactDetails { get; set; } = string.Empty;
}
