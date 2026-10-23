using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>Approval rule: approve every requisition automatically.</summary>
public class AutoApproveAllPolicy : IApprovalPolicy
{
    public string Name => "Auto-approve all";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order) =>
        new(false, "Company policy auto-approves all requisitions.");
}
