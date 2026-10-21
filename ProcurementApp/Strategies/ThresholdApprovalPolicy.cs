using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>
/// STRATEGY #1 — the default organisation rule from the brief: orders at or below a
/// monetary threshold (default Rs.100,000) are auto-approved; anything above needs a
/// manager's approval.
/// </summary>
public class ThresholdApprovalPolicy : IApprovalPolicy
{
    private readonly decimal _threshold;

    public ThresholdApprovalPolicy(decimal threshold)
    {
        _threshold = threshold;
    }

    public string Name => $"Threshold (auto-approve ≤ Rs.{_threshold:N0})";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order)
    {
        if (order.TotalValue <= _threshold)
            return new ApprovalPolicyResult(false,
                $"Value Rs.{order.TotalValue:N2} is within the Rs.{_threshold:N0} auto-approval limit.");

        return new ApprovalPolicyResult(true,
            $"Value Rs.{order.TotalValue:N2} exceeds the Rs.{_threshold:N0} limit — manager approval required.");
    }
}
