using System.ComponentModel.DataAnnotations;

namespace ProcurementApp.Domain;

/// <summary>A material/product that can be ordered (ESAD-02 class diagram: Item).</summary>
public class Item
{
    [Key] public int Id { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>A pre-agreed price for an item from a supplier (ESAD-02 class diagram: CatalogueItem).</summary>
public class CatalogueItem
{
    [Key] public int Id { get; set; }

    public Item Item { get; set; } = new();
    public decimal AgreedPrice { get; set; }
    public DateTime EffectiveFrom { get; set; }

    /// <summary>Which supplier offers this price (so the catalogue can be filtered per supplier).</summary>
    public string SupplierId { get; set; } = string.Empty;

    public bool IsPriced() => AgreedPrice > 0m;
}
