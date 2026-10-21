using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>
/// STRATEGY #2 — a stricter organisation rule: every requisition, regardless of value,
/// must be approved by a manager. (e.g. a company with tight procurement controls.)
/// </summary>
public class AlwaysRequireApprovalPolicy : IApprovalPolicy
{
    public string Name => "Always require approval";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order) =>
        new(true, "Company policy requires every requisition to be approved by a manager.");
}
