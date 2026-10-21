namespace ProcurementApp.Strategies;

/// <summary>
/// FACTORY PATTERN (supporting the Strategy) — creates the correct <see cref="IApprovalPolicy"/>
/// from configuration, so switching the whole organisation's approval behaviour is a single
/// setting in appsettings.json ("Approval:Policy"). The rest of the app never does a `new`
/// on a concrete policy — it asks the factory.
/// </summary>
public static class ApprovalPolicyFactory
{
    public static IApprovalPolicy Create(string policyName, decimal threshold)
    {
        return policyName?.Trim().ToLowerInvariant() switch
        {
            "always" => new AlwaysRequireApprovalPolicy(),
            "autoapprove" or "auto" => new AutoApproveAllPolicy(),
            "threshold" => new ThresholdApprovalPolicy(threshold),
            _ => new ThresholdApprovalPolicy(threshold) // sensible default
        };
    }
}
