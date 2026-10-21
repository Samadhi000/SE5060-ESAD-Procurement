using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>
/// STRATEGY #3 — a lenient organisation rule (e.g. a small trusted site team): no
/// requisition needs manager approval; everything is auto-approved and placed.
/// </summary>
public class AutoApproveAllPolicy : IApprovalPolicy
{
    public string Name => "Auto-approve all";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order) =>
        new(false, "Company policy auto-approves all requisitions.");
}
