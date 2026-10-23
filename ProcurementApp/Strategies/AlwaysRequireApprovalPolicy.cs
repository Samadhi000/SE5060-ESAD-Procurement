using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>Approval rule: every requisition needs a manager, regardless of value.</summary>
public class AlwaysRequireApprovalPolicy : IApprovalPolicy
{
    public string Name => "Always require approval";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order) =>
        new(true, "Company policy requires every requisition to be approved by a manager.");
}
