using System.ComponentModel.DataAnnotations;
using ProcurementApp.Domain.Enums;

namespace ProcurementApp.Domain;

/// <summary>
/// One entry in a purchase order's approval history (ESAD-02 class diagram: Approval).
/// Created every time a decision is recorded against a requisition (UC2 / SD2).
/// </summary>
public class Approval
{
    [Key] public int Id { get; set; }

    public string ApprovalId { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public ApprovalDecisionType Decision { get; set; }
    public DateTime DecisionDate { get; set; } = DateTime.Now;
    public int ApprovalLevel { get; set; } = 1;
    public string ManagerName { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
