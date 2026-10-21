# Module 2 — Approval (Strategy pattern) — Guide

This is **Samadhi's module**: *Review and Approve / Decline / Refer a Requisition* (UC2 / SD2),
built with the **Strategy** design pattern (+ a supporting **Factory**).

The project is an **ASP.NET Core MVC web app** (C#, .NET 10). It runs locally in a browser —
no mobile app, no hosting.

---

## How to open & run in Visual Studio

1. Open **`C:\Projects\ProcurementApp\ProcurementApp.sln`** in Visual Studio 2022.
2. Press **F5** (or the green ▶ "ProcurementApp" button). A browser opens automatically.
3. Click **Approvals** in the top nav (or go to `/Approval`).

You'll see the **Pending Approvals** list; click **Review** on any order to open the decision
screen and use **Approve / Decline / Refer**.

> Running from a terminal instead: `dotnet run` inside `C:\Projects\ProcurementApp\ProcurementApp`.

---

## What's built (file map)

### Shared foundation — built once, used by everyone (don't edit without telling the team)
| File | What it is |
|---|---|
| `Domain/*.cs` | The entity classes from the ESAD-02 class diagram (PurchaseOrder, Site, Supplier, Approval, …) |
| `Data/IPurchaseOrderRepository.cs` | Storage interface — modules depend on this, not on a database |
| `Data/InMemoryPurchaseOrderRepository.cs` | Test-data store (no DB yet); swapped for SQLite later with no code change |
| `Data/SeedData.cs` | Sample sites, suppliers, and 5 orders (values straddle Rs.100,000 for the demo) |
| `Services/INotificationService.cs` + `ConsoleNotificationService.cs` | Notification interface + stub (Module 4 replaces the stub with the real Observer) |

### Your module (Module 2)
| File | What it is |
|---|---|
| `Strategies/IApprovalPolicy.cs` | **The Strategy interface** |
| `Strategies/ThresholdApprovalPolicy.cs` | Rule 1: auto-approve ≤ Rs.100,000, else needs approval |
| `Strategies/AlwaysRequireApprovalPolicy.cs` | Rule 2: everything needs approval |
| `Strategies/AutoApproveAllPolicy.cs` | Rule 3: everything auto-approved |
| `Strategies/ApprovalPolicyFactory.cs` | **The Factory** — picks the rule from config |
| `Services/ApprovalService.cs` | Your business logic: Approve / Decline / Refer / ApplyPolicy |
| `Controllers/ApprovalController.cs` | Thin controller — just calls the service |
| `Views/Approval/Index.cshtml` | Pending Approvals list (wireframe Fig 10) |
| `Views/Approval/Details.cshtml` | Decision screen (wireframe Fig 11) |

---

## How the Strategy pattern works here (and how to DEMO it)

The brief says approval rules differ per organisation. So each rule is a **swappable class**
implementing `IApprovalPolicy`. The active one is chosen in **`appsettings.json`**:

```json
"Approval": {
  "Policy": "Threshold",      // "Threshold" | "Always" | "AutoApprove"
  "ThresholdLkr": 100000
}
```

**Demo in the viva (the money shot):**
1. With `"Threshold"`, open `/Approval` → some orders say **Auto-approvable**, some **Needs approval**.
   Click **Apply Approval Policy** → the small ones auto-approve and disappear; big ones remain.
2. Stop the app, change `"Policy"` to `"AutoApprove"`, run again, click **Apply Approval Policy**
   → **every** order is approved. Same button, different behaviour — *because we swapped the strategy,
   not the code.*
3. Change it to `"Always"` → nothing auto-approves; every order must be approved by hand.

That one change, with no edit to the controller/service/UI, **is** the Strategy pattern.

---

## Your viva script

> *"My module is the approval of requisitions. I used the **Strategy pattern** because the brief says
> approval rules vary by organisation — some auto-approve under Rs.100,000, some always require approval.
> So I made `IApprovalPolicy` an interface, with three interchangeable rules: `ThresholdApprovalPolicy`,
> `AlwaysRequireApprovalPolicy`, and `AutoApproveAllPolicy`. My `ApprovalService` just holds an
> `IApprovalPolicy` and asks it to evaluate each order — it never knows which concrete rule it's using.
> To switch the whole company's policy I change one line in appsettings; a small **Factory**
> (`ApprovalPolicyFactory`) reads that setting and creates the right rule. So the behaviour changes
> with zero change to the approval logic or the screens — that's the benefit of Strategy: the rule is
> decoupled from the thing that uses it."*

**Likely panel question — "Why not just an if/else on the value?"**
> *"An if/else hard-codes one company's rule. New rules mean editing and re-testing the core logic.
> With Strategy each rule is its own class, added without touching existing code (Open/Closed Principle),
> and the rule is chosen at runtime from configuration."*

---

## Notes for the team
- **No login** — out of scope; the acting approver is a fixed demo user ("Sarah Chen").
- **Status names** (`Domain/Enums/PurchaseOrderStatus.cs`) are shared — Module 3 (State pattern) owns the
  lifecycle; agree any changes together.
- **Notifications** currently just log — Module 4 (Observer) will provide the real `INotificationService`.
- **Database** is in-memory test data now; whoever sets up EF Core + SQLite implements
  `IPurchaseOrderRepository` and swaps the registration in `Program.cs`. No module code changes.
- **Git:** a `.gitignore` is included so `bin/` and `obj/` aren't committed. Each member works in their
  own Controller/Service/Views/pattern files, so commits won't collide.
