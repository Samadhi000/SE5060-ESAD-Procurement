using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>
/// STRATEGY PATTERN — the interface (the "strategy").
///
/// The case-study brief states that approval rules vary by organisation
/// ("employees may be able to purchase goods under a limit of Rs.100,000 without requiring
/// approval ... different organizations have different hierarchies and rules"). Rather than
/// hard-code one rule, each rule is a swappable class implementing this interface. The
/// approval service holds an <see cref="IApprovalPolicy"/> and asks it to evaluate an order,
/// so the company's policy can change without touching the approval logic or the UI.
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
