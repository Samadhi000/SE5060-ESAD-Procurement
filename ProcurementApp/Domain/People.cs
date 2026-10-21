using System.ComponentModel.DataAnnotations;

namespace ProcurementApp.Domain;

/// <summary>
/// Base class for the staff roles in the system (ESAD-02 class diagram: Employee).
/// NOTE: login/password is intentionally omitted — authentication is out of scope
/// per the design's own assumptions (see the Critical Reflection Report).
/// EF Core maps this inheritance as a single table with a discriminator column.
/// </summary>
public abstract class Employee
{
    [Key] public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string GetContactDetails() => $"{Name} ({Email}, {ContactNumber})";
}

/// <summary>The mobile-app primary user who raises orders and logs deliveries.</summary>
public class SiteManager : Employee
{
    public int? AssignedSiteId { get; set; }
    public Site? AssignedSite { get; set; }
}

/// <summary>Reviews requisitions and records Approve / Decline / Refer decisions (UC2).</summary>
public class ApprovingManager : Employee
{
    /// <summary>The monetary limit this manager is authorised to approve (Rs.).</summary>
    public decimal ApprovalLimit { get; set; }
}

/// <summary>Maintains supplier and catalogue reference data.</summary>
public class ProcurementOfficer : Employee
{
}
