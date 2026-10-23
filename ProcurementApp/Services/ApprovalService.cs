using ProcurementApp.Data;
using ProcurementApp.Domain;
using ProcurementApp.Domain.Enums;
using ProcurementApp.Strategies;

namespace ProcurementApp.Services;

public record ApprovalActionResult(bool Success, string Message);

/// <summary>Module 2: approve, decline or refer a requisition. Uses the injected approval policy (Strategy) and records the decision.</summary>
public interface IApprovalService
{
    string PolicyName { get; }
    IReadOnlyList<PurchaseOrder> GetPending();
    IReadOnlyList<PurchaseOrder> GetQueue();
    PurchaseOrder? Get(string reference);
    ApprovalPolicyResult EvaluatePolicy(PurchaseOrder order);

    ApprovalActionResult Approve(string reference, string managerName, string? comment);
    ApprovalActionResult Decline(string reference, string managerName, string comment);
    ApprovalActionResult Refer(string reference, string managerName, string comment);

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

    // Awaiting a decision, plus anything referred on.
    public IReadOnlyList<PurchaseOrder> GetQueue() =>
        _orders.GetByStatus(PurchaseOrderStatus.WaitingForApproval)
            .Concat(_orders.GetByStatus(PurchaseOrderStatus.Referred))
            .OrderByDescending(o => o.DateCreated)
            .ToList();

    public PurchaseOrder? Get(string reference) => _orders.GetByReference(reference);

    public ApprovalPolicyResult EvaluatePolicy(PurchaseOrder order) => _policy.Evaluate(order);

    public ApprovalActionResult Approve(string reference, string managerName, string? comment)
    {
        var order = _orders.GetByReference(reference);
        if (order is null) return new ApprovalActionResult(false, $"Order {reference} not found.");
        if (order.Status != PurchaseOrderStatus.WaitingForApproval)
            return new ApprovalActionResult(false, $"Order {reference} is not awaiting approval.");

        order.ChangeStatus(PurchaseOrderStatus.Approved);
        order.ChangeStatus(PurchaseOrderStatus.Placed);
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

    // Runs the policy over every pending order and auto-approves those it does not flag.
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
