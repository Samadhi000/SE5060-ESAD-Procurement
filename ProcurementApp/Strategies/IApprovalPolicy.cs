using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;///// <summary>
/// STRATEGY PATTERN — the interface (the "strategy").
/// </summary>
public interface IApprovalPolicy
{
    /// <summary>Human-readable name of the active policy (shown in the UI / viva demo).</summary>
    string Name { get; }

    /// <summary>Decides whether the given order requires a manager's approval.</summary>
    ApprovalPolicyResult Evaluate(PurchaseOrder order);
}

/// <summary>Outcome of evaluating an order against an approval policy.</summary>
public record ApprovalPolicyResult(bool RequiresApproval, string Reason);
