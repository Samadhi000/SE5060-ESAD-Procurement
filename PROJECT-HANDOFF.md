# PROJECT HANDOFF — SitePro Procurement (SE5060 Case Study 2)

> Copy this file with the project. It summarises everything needed to build and continue the
> project on another machine (e.g. a second laptop with a different Claude account).

---

## 1. What this project is

- **Module:** SE5060 – Enterprise Software Analysis & Design (ESAD), SLIIT MSc.
- **Assignment:** Implementation Walkthrough (Assignment 03), **due 24 Oct 2026**.
- **Task:** Implement the **Procurement for Construction** system from *another team's* design
  (the "ESAD-02" design document). Group of 4; each member owns one module + one design pattern.
- **This app** = the full working implementation: an **ASP.NET Core MVC web application (C#)**,
  runs locally in a browser, with a real **SQLite** database. It is a simulator/walkthrough, not a
  production system.

**Owner of this build:** Samadhi — owns **Module 2 (Approve/Decline/Refer + Strategy pattern)**.

---

## 2. Where the files are (on the original laptop)

```
C:\Projects\ProcurementApp\                 <-- the whole project (repo root)
├─ ProcurementApp.sln                        <-- open THIS in Visual Studio
├─ README.md                                 <-- full app + pattern explanations
├─ MODULE-2-APPROVAL-GUIDE.md                <-- Samadhi's module + viva script
├─ PROJECT-HANDOFF.md                        <-- this file
├─ .gitignore
└─ ProcurementApp\                            <-- the web project
   ├─ Program.cs                              <-- app startup + dependency injection
   ├─ appsettings.json                        <-- approval policy setting lives here
   ├─ Domain\                                 <-- entity classes (shared by all modules)
   ├─ Data\                                    <-- DbContext + repositories + seed (SQLite)
   ├─ Builders\                                <-- Module 1: Builder pattern
   ├─ Strategies\                              <-- Module 2: Strategy + Factory
   ├─ States\                                  <-- Module 3: State pattern
   ├─ Services\                                <-- all module services + Observer notifications
   ├─ Controllers\                             <-- one thin controller per module
   ├─ Views\                                   <-- Razor screens (reproduce the wireframes)
   └─ wwwroot\                                 <-- Bootstrap CSS/JS, site styles
```

The design document being implemented (for reference) is at:
`...\Enterprise Software Analysis and Design\SE5060-ESAD-02 Implementation\ESAD-02\ESAD-02.pdf`

---

## 3. How to move it to the other laptop

Pick either:

- **Option A — GitHub (recommended for the team):** push `C:\Projects\ProcurementApp` to a GitHub
  repo, then `git clone` it on the other laptop. The `.gitignore` keeps build junk out.
- **Option B — Copy the folder:** copy the whole `C:\Projects\ProcurementApp` folder via USB /
  OneDrive / zip. You can delete the `bin`, `obj` folders and `procurement.db` before copying — they
  are regenerated automatically.

**Important:** put the copy in a plain local folder like `C:\Projects\ProcurementApp` on the other
laptop too — **NOT inside OneDrive** (OneDrive sync fights with Visual Studio's build and causes errors).

---

## 4. What the other laptop needs

1. **Visual Studio 2022 or 2026** with the **"ASP.NET and web development"** workload.
   (During Visual Studio Installer, tick only that one workload.)
2. A **.NET SDK** — any of .NET 7, 8, 9 or 10 can build this project (it targets **.NET 7** on purpose,
   for maximum compatibility across machines). The ASP.NET workload already includes a suitable SDK.
   - If it ever complains it can't build, install the **.NET 7 SDK** from
     https://dotnet.microsoft.com/download/dotnet/7.0.

> Why .NET 7? The project is deliberately set to `net7.0` so it opens in **both** Visual Studio 2022
> and 2026 without an "SDK does not support .NET 10" error. The framework version does not affect marks.

---

## 5. How to open & run

1. Open **Visual Studio** → **"Open a project or solution"** (NOT "Open a local folder").
2. Open `C:\Projects\ProcurementApp\ProcurementApp.sln`.
3. Press **F5** (or the green ▶ button).
4. If asked to **trust the localhost HTTPS development certificate**, click **Yes** (one-time, safe).
5. A browser opens; the **SQLite database creates and seeds itself automatically** on first run.

Terminal alternative: `dotnet run` inside the `ProcurementApp` subfolder, then open the shown URL.

---

## 6. The four modules and their design patterns

| # | Module (use case) | Pattern | Key folder/files |
|---|---|---|---|
| 1 | Create & Submit PO | **Builder** (+ Singleton PO-reference) | `Builders/PurchaseOrderBuilder.cs`, `Services/OrderCreationService.cs` |
| 2 | Approve / Decline / Refer *(Samadhi)* | **Strategy** (+ Factory) | `Strategies/`, `Services/ApprovalService.cs` |
| 3 | Log Delivery (incl. partial) | **State** | `States/OrderStates.cs`, `Services/DeliveryService.cs` |
| 4 | Enquire / View + Reports | **Observer** | `Services/Notifications/`, `Services/EnquiryService.cs` |
| — | Data access | **Repository** | `Data/` (EF Core + SQLite) |

Full explanations are in **README.md**. Samadhi's viva script is in **MODULE-2-APPROVAL-GUIDE.md**.

The Strategy is demonstrated by changing `Approval:Policy` in `appsettings.json`
(`Threshold` | `Always` | `AutoApprove`) — the approval behaviour changes with no code edits.

---

## 7. Key design decisions (for the report / viva)

- **No login/authentication** — out of scope per the design's own assumptions; demo users are fixed.
- **Database** — real SQLite via EF Core; created + seeded on startup (`Data/DbSeeder.cs`). Delete
  `procurement.db` to reseed.
- **Shared foundation** — `Domain/` (entities) and `Data/` (repositories) are built once and used by
  all modules; each member works in their own Controller/Service/pattern/Views files so Git doesn't
  conflict.
- **Interfaces everywhere** — modules depend on `IPurchaseOrderRepository`, `INotificationService`,
  etc., so persistence and notifications can be swapped without touching module code.

---

## 8. What is still pending (next steps)

1. **Push to GitHub** (shared team repo) if not done.
2. **Critical Reflection Report** (30 marks) — document the changes we made vs the ESAD-02 design:
   dropped `Employee.password` (auth out of scope), fixed `logDelivery()` misplaced on
   ApprovingManager, introduced State / Strategy / Observer / Builder, added a new **State-machine
   diagram** for the PurchaseOrder lifecycle (the 5-mark "new diagram").
3. **Per-member viva prep** — each member defends their module + pattern.

---

## 9. To continue with a fresh Claude session on the other laptop

Paste this into the new Claude Code session to give it context:

> "I'm continuing a SLIIT SE5060 ESAD group assignment — the Implementation Walkthrough for Case
> Study 2 (Procurement for Construction). The full app is already built: an ASP.NET Core MVC web app
> (C#, targets .NET 7), SQLite database via EF Core, four modules each with a GoF design pattern
> (Builder, Strategy+Factory, State, Observer) plus Repository. It's at `C:\Projects\ProcurementApp`
> (solution `ProcurementApp.sln`). Read `README.md`, `PROJECT-HANDOFF.md` and
> `MODULE-2-APPROVAL-GUIDE.md` in that folder for full context. I own Module 2 (Approval + Strategy).
> Help me [run it / write the critical reflection report / draw the state diagram / etc.]."
