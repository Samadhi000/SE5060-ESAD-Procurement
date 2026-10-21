namespace ProcurementApp.Domain.Enums;

/// <summary>
/// The outcome an Approving Manager can record against a requisition (UC2 / SD2).
/// </summary>
public enum ApprovalDecisionType
{
    Approved,
    Declined,
    Referred,
    AutoApproved   // approved automatically by the configured approval policy (Strategy)
}
