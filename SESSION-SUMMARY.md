# SESSION SUMMARY — what we've done so far

> Read this (plus `README.md`, `PROJECT-HANDOFF.md`, `MODULE-2-APPROVAL-GUIDE.md`) to pick up the
> project on another machine or in a new Claude session. It captures the decisions and the current
> state so nothing is lost.

---

## The assignment

- **Module:** SE5060 – Enterprise Software Analysis & Design (ESAD), SLIIT MSc.
- **This phase:** Implementation Walkthrough (Assignment 03), **due 24 Oct 2026**.
- **Task:** Build a working implementation of the **Procurement for Construction** system, using
  **another team's design** (the "ESAD-02" document). Group of 4 — each member owns one module + one
  GoF design pattern and defends it in a viva.
- **Owner of this repo:** Samadhi — owns **Module 2 (Approve/Decline/Refer + Strategy pattern)**.

### Marking (100)
- Basic implementation 30 (class structure 10 + sequence functionality 10 + UI fidelity 10)
- Critical Reflection Report 30 (class changes 10 + sequence changes 10 + UI usage 5 + **one new diagram 5**)
- Individual 40 (module enhancement 15 + coding conventions 5 + **design pattern 20**)

Biggest levers: the **design patterns** and the **reflection report**.

---

## Key decisions we made (and why)

1. **Web app, not a native mobile app.** The design shows a mobile app + web portal, but for a
   walkthrough we build ONE ASP.NET Core MVC web app. The "mobile" screens are phone-styled web pages.
   No hosting — it runs locally in a browser.
2. **Language: C# / ASP.NET Core MVC** — maps 1:1 to the ESAD-02 class diagram; patterns are easy to
   defend in the viva.
3. **Target framework: .NET 7** — chosen so the solution opens in BOTH Visual Studio 2022 and 2026.
   (VS 2022 can't build .NET 10; .NET 7 avoids that error. Framework version doesn't affect marks.)
4. **Database: SQLite via EF Core** — real DB, created + seeded automatically on first run.
5. **Design patterns (one per module + Repository):**
   - Module 1 Create & Submit PO → **Builder** (+ Singleton PO-reference generator)
   - Module 2 Approve/Decline/Refer → **Strategy** (+ Factory)  ← Samadhi
   - Module 3 Log Delivery → **State**
   - Module 4 Enquire/View + Reports + Notifications → **Observer**
   - Data layer → **Repository**
6. **No login/authentication** — out of scope per the design's own assumptions; demo users are fixed.
7. **Shared foundation:** `Domain/` (entities) + `Data/` (repositories) are built once and used by all
   modules. Each member works only in their own Controller/Service/pattern/Views files, so Git doesn't
   conflict.

---

## What is DONE and verified

- ✅ Full app built; all 4 modules implemented with their patterns.
- ✅ Real SQLite database (EF Core), auto-created and seeded with sites, suppliers, priced catalogue,
  and sample orders (values straddle Rs.100,000 to demonstrate the approval Strategy).
- ✅ All screens reproduce the ESAD-02 wireframes (Bootstrap-styled).
- ✅ **End-to-end flow tested in the browser:** created an order (Builder) → approved it (Strategy) →
  it appeared for delivery → recorded a partial delivery (State → Partially Delivered) → enquiry screen
  showed full approval + delivery history → notifications inbox captured every event (Observer).
- ✅ Pushed to GitHub (private): **https://github.com/Samadhi000/SE5060-ESAD-Procurement**
- ✅ Commits are authored by Samadhi only (no AI attribution).

### Locations
- Code (original laptop): `C:\Projects\ProcurementApp`  (open `ProcurementApp.sln`)
- GitHub: `https://github.com/Samadhi000/SE5060-ESAD-Procurement`
- The ESAD-02 design being implemented: `...\SE5060-ESAD-02 Implementation\ESAD-02\ESAD-02.pdf`

### How to run
Open `ProcurementApp.sln` in Visual Studio → press **F5** → click **Yes** on the HTTPS cert prompt.
The database creates itself. (Full steps in `PROJECT-HANDOFF.md`.)

---

## What's STILL TO DO

1. **Critical Reflection Report (30 marks)** — document the changes we made vs the ESAD-02 design:
   - Dropped `Employee.password` (authentication out of scope — the design's own assumption).
   - Fixed `logDelivery()` wrongly placed on `ApprovingManager` (belongs to the Site Manager flow).
   - Replaced the big status-enum + `changeStatus()` with the **State pattern**.
   - Added **Strategy** for approval rules (brief demands configurable rules).
   - Added **Observer** for notifications (the design's component diagram isolates a Notification Service).
   - Added **Builder** for step-by-step order creation.
   - Introduced the **Repository** pattern for swappable persistence (in-memory → SQLite).
2. **New diagram (5 marks)** — a PurchaseOrder **state-machine diagram** (pairs with the State pattern).
3. **Add teammates as GitHub collaborators** (repo → Settings → Collaborators) so they build their modules.
4. **Per-member viva prep** — each defends their module + pattern (Samadhi's script is in
   `MODULE-2-APPROVAL-GUIDE.md`).

---

## To continue in a NEW Claude session (e.g. on the other laptop)

Paste this to give the new session context:

> "I'm continuing a SLIIT SE5060 ESAD group assignment — the Implementation Walkthrough for Case
> Study 2 (Procurement for Construction). The full app is already built and on GitHub
> (https://github.com/Samadhi000/SE5060-ESAD-Procurement): an ASP.NET Core MVC web app (C#, targets
> .NET 7), SQLite database via EF Core, four modules each with a GoF design pattern (Builder,
> Strategy+Factory, State, Observer) plus Repository. It's cloned at `C:\Projects\ProcurementApp`
> (solution `ProcurementApp.sln`). Read `SESSION-SUMMARY.md`, `README.md`, `PROJECT-HANDOFF.md` and
> `MODULE-2-APPROVAL-GUIDE.md` in that folder for full context. I own Module 2 (Approval + Strategy).
> Help me with [running it / the critical reflection report / the state diagram / my teammate's module]."
