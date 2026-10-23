using ProcurementApp.Domain;

namespace ProcurementApp.Strategies;

/// <summary>Approval rule: auto-approve orders up to the threshold; anything above needs a manager.</summary>
public class ThresholdApprovalPolicy : IApprovalPolicy
{
    private readonly decimal _threshold;

    public ThresholdApprovalPolicy(decimal threshold)
    {
        _threshold = threshold;
    }

    public string Name => $"Threshold (auto-approve ≤ LKR {_threshold:N0})";

    public ApprovalPolicyResult Evaluate(PurchaseOrder order)
    {
        if (order.TotalValue <= _threshold)
            return new ApprovalPolicyResult(false,
                $"Value LKR {order.TotalValue:N2} is within the LKR {_threshold:N0} auto-approval limit.");

        return new ApprovalPolicyResult(true,
            $"Value LKR {order.TotalValue:N2} exceeds the LKR {_threshold:N0} limit. Manager approval required.");
    }
}
