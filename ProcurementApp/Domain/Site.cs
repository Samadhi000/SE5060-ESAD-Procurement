using System.ComponentModel.DataAnnotations;

namespace ProcurementApp.Domain;

/// <summary>A construction site that an order is delivered to (ESAD-02 class diagram: Site).</summary>
public class Site
{
    [Key] public int Id { get; set; }

    public string SiteId { get; set; } = string.Empty;        // business id, e.g. S01
    public string SiteName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string SiteContactPerson { get; set; } = string.Empty;
    public string SiteContactNumber { get; set; } = string.Empty;

    public string GetSiteContactDetails() => $"{SiteContactPerson} — {SiteContactNumber}";
}
