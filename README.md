# SitePro Procurement — SE5060 Case Study 2 Implementation

An ASP.NET Core MVC web application (C#, .NET 10) implementing the **Procurement for Construction**
design from the ESAD-02 team. It runs locally in a browser — no mobile app, no hosting.

The four core use cases are implemented, each with a GoF **design pattern**, over a real **SQLite**
database (EF Core).

---

## How to run

1. Open **`ProcurementApp.sln`** in Visual Studio 2022.
2. Press **F5**. A browser opens at `https://localhost:xxxx`.
3. The SQLite database (`procurement.db`) is created and seeded automatically on first run.

From a terminal instead: `dotnet run` inside the `ProcurementApp` folder.

---

## The four modules and their design patterns

| # | Module (use case) | Pattern | Where the pattern lives |
|---|---|---|---|
| 1 | **Create & Submit PO** (UC1) | **Builder** | `Builders/PurchaseOrderBuilder.cs` |
| 2 | **Approve / Decline / Refer** (UC2) | **Strategy** (+ Factory) | `Strategies/` |
| 3 | **Log Delivery** (UC3) | **State** | `States/OrderStates.cs` |
| 4 | **Enquire / View + Reports** (UC4) | **Observer** | `Services/Notifications/` |
| — | Data access | **Repository** | `Data/` (EF Core + SQLite) |

### 1. Builder — building a purchase order step by step
A purchase order is assembled across a multi-step wizard (site → supplier → lines). `PurchaseOrderBuilder`
adds each part and only produces a finished order via `BuildForApproval()`, which **validates** it first
(must have lines, none unpriced). This guarantees an order never exists half-valid.

### 2. Strategy — swappable approval rules
The brief says approval rules vary per organisation. `IApprovalPolicy` has three interchangeable rules
(`ThresholdApprovalPolicy`, `AlwaysRequireApprovalPolicy`, `AutoApproveAllPolicy`). The active one is
chosen in `appsettings.json` → `Approval:Policy` by a **Factory** (`ApprovalPolicyFactory`). Switching the
company's policy changes behaviour with **no change** to the approval code.

### 3. State — the order lifecycle controls deliveries
Whether an order can receive a delivery, and what status it becomes afterwards, depends on its state.
Each state is a class (`PlacedState`, `PartiallyDeliveredState`, `FullyDeliveredState`,
`NotDeliverableState`) that knows its own rules — no big `switch` on status scattered around.

### 4. Observer — notifications
When an order changes (approved, placed, delivered), `NotificationHub` (the **subject**) forwards the
event to every subscribed **observer** (`InAppInboxObserver` shows them on the Notifications page;
`ConsoleLogObserver` logs them). The modules that raise events don't know who is listening.

### Repository — swappable persistence
Every module depends on interfaces (`IPurchaseOrderRepository`, …), never on EF directly. The app uses
`EfPurchaseOrderRepository` (SQLite); it could be swapped for another store by changing one line in
`Program.cs`.

---

## Project structure

```
ProcurementApp/
├─ Domain/          Entity classes from the ESAD-02 class diagram (shared by all modules)
├─ Data/            DbContext, repositories (Repository pattern), seed data  → SQLite
├─ Builders/        PurchaseOrderBuilder            (Module 1 — Builder)
├─ Strategies/      IApprovalPolicy + rules + Factory (Module 2 — Strategy)
├─ States/          Order states                    (Module 3 — State)
├─ Services/
│   ├─ Notifications/  NotificationHub + observers  (Module 4 — Observer)
│   ├─ OrderCreationService.cs   (Module 1)
│   ├─ ApprovalService.cs        (Module 2)
│   ├─ DeliveryService.cs        (Module 3)
│   └─ EnquiryService.cs         (Module 4)
├─ Controllers/     One thin controller per module
├─ Views/           Razor screens (reproduce the ESAD-02 wireframes)
└─ Program.cs       Dependency injection wiring
```

## Try the full flow
1. **New Order** → pick site + supplier → add a quantity → Submit (Builder creates `PO-2026-#####`).
2. **Approvals** → open an over-Rs.100,000 order → Approve (Strategy; status → Placed).
3. **Deliveries** → the placed order appears → Record a partial delivery (State → Partially Delivered).
4. **My Orders** → open the order → see approval + delivery history.
5. **Notifications** → every step above produced an event (Observer).

## Notes
- **No login** — authentication is out of scope per the design's assumptions. Acting users are fixed demo
  staff ("Sarah Chen").
- The database file `procurement.db` is created on first run and ignored by Git. Delete it to reseed.
- Each team member owns one module's Controller / Service / pattern / Views; the `Domain` and `Data`
  folders are the shared foundation.
