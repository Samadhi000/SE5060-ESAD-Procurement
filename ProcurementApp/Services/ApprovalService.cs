using ProcurementApp.Data;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Strategies;

namespace ProcurementApp.Services;

public record ApprovalActionResult(bool Success, string Message);

/// <summary>
/// Business logic for Module 2 — Review and Approve / Decline / Refer a Requisition (UC2/SD2).
/// The controller stays thin and calls into this service. The service uses the injected
/// <see cref="IApprovalPolicy"/> (Strategy) and records history + fires notifications.
/// </summary>
public interface IApprovalService
{
    string PolicyName { get; }
    IReadOnlyList<PurchaseOrder> GetPending();
    PurchaseOrder? Get(string reference);
    ApprovalPolicyResult EvaluatePolicy(PurchaseOrder order);

    ApprovalActionResult Approve(string reference, string managerName, string? comment);
    ApprovalActionResult Decline(string reference, string managerName, string comment);
    ApprovalActionResult Refer(string reference, string managerName, string comment);

    /// <summary>Applies the policy (Strategy) to every pending order, auto-approving those that don't need a manager.</summary>
    int ApplyPolicyAutoApprovals(string systemActor);
}

public class ApprovalService : IApprovalService
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly INotificationService _notifications;
    private readonly IApprovalPolicy _policy;

    public ApprovalService(
        IPurchaseOrderRepository orders,
        INotificationService notifications,
        IApprovalPolicy policy)
    {
        _orders = orders;
        _notifications = notifications;
        _policy = policy;
    }

    public string PolicyName => _policy.Name;

    public IReadOnlyList<PurchaseOrder> GetPending() =>
        _orders.GetByStatus(PurchaseOrderStatus.WaitingForApproval);

    public PurchaseOrder? Get(string reference) => _orders.GetByReference(reference);

    public ApprovalPolicyResult EvaluatePolicy(PurchaseOrder order) => _policy.Evaluate(order);

    public ApprovalActionResult Approve(string reference, string managerName, string? comment)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return new ApprovalActionResult(false, $"Order {reference} not found.");
        if (order.Status != PurchaseOrderStatus.WaitingForApproval)
            return new ApprovalActionResult(false, $"Order {reference} is not awaiting approval.");

        order.ChangeStatus(PurchaseOrderStatus.Approved);
        order.ChangeStatus(PurchaseOrderStatus.Placed);   // approved orders are placed & sent to the supplier
        RecordDecision(order, ApprovalDecisionType.Approved, managerName, comment);
        _orders.Update(order);

        _notifications.Notify(order.CreatedBy?.Name ?? "Site Manager", "Your order was approved and placed.", order);
        _notifications.Notify(order.Supplier?.Name ?? "Supplier", "A new purchase order has been placed with you.", order);
        return new ApprovalActionResult(true, $"{reference} approved and placed.");
    }

    public ApprovalActionResult Decline(string reference, string managerName, string comment)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return new ApprovalActionResult(false, $"Order {reference} not found.");
        if (order.Status != PurchaseOrderStatus.WaitingForApproval)
            return new ApprovalActionResult(false, $"Order {reference} is not awaiting approval.");

        order.ChangeStatus(PurchaseOrderStatus.Declined);
        RecordDecision(order, ApprovalDecisionType.Declined, managerName, comment);
        _orders.Update(order);

        _notifications.Notify(order.CreatedBy?.Name ?? "Site Manager", $"Your order was declined: {comment}", order);
        return new ApprovalActionResult(true, $"{reference} declined.");
    }

    public ApprovalActionResult Refer(string reference, string managerName, string comment)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return new ApprovalActionResult(false, $"Order {reference} not found.");
        if (order.Status != PurchaseOrderStatus.WaitingForApproval)
            return new ApprovalActionResult(false, $"Order {reference} is not awaiting approval.");

        order.ChangeStatus(PurchaseOrderStatus.Referred);
        RecordDecision(order, ApprovalDecisionType.Referred, managerName, comment);
        _orders.Update(order);

        _notifications.Notify("Senior Approving Manager", $"Order referred for further review: {comment}", order);
        return new ApprovalActionResult(true, $"{reference} referred for further review.");
    }

    public int ApplyPolicyAutoApprovals(string systemActor)
    {
        var count = 0;
        foreach (var order in GetPending().ToList())
        {
            var result = _policy.Evaluate(order);
            if (!result.RequiresApproval)
            {
                order.ChangeStatus(PurchaseOrderStatus.Approved);
                order.ChangeStatus(PurchaseOrderStatus.Placed);
                RecordDecision(order, ApprovalDecisionType.AutoApproved, systemActor, result.Reason);
                _orders.Update(order);
                _notifications.Notify(order.CreatedBy?.Name ?? "Site Manager",
                    "Your order was auto-approved under company policy and placed.", order);
                count++;
            }
        }
        return count;
    }

    private static void RecordDecision(PurchaseOrder order, ApprovalDecisionType decision, string manager, string? comment)
    {
        order.Approvals.Add(new Approval
        {
            Decision = decision,
            ManagerName = manager,
            Comment = comment,
            DecisionDate = DateTime.Now
        });
    }
}
