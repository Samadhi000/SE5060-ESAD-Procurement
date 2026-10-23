namespace ProcurementApp.Strategies;

/// <summary>Factory: creates the approval policy named in appsettings.json ("Approval:Policy").</summary>
public static class ApprovalPolicyFactory
{
    public static IApprovalPolicy Create(string policyName, decimal threshold)
    {
        return policyName?.Trim().ToLowerInvariant() switch
        {
            "always" => new AlwaysRequireApprovalPolicy(),
            "autoapprove" or "auto" => new AutoApproveAllPolicy(),
            "threshold" => new ThresholdApprovalPolicy(threshold),
            _ => new ThresholdApprovalPolicy(threshold)
        };
    }
}
