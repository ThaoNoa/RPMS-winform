# RPMS — System Documentation Index

**Project:** Rental Property Management System (RPMS)  
**Workspace:** `E:\DoAn\RPMS`  
**Generated from:** real source code (read-only documentation pass)  
**Stack:** .NET 8 · WinForms · EF Core 8 · SQL Server · layered BLL/DAL/DTO

This folder is the complete system documentation set. Read in order, or jump via the section map below.

---

## Document map (user sections 1–20)

| # | Topic | File |
|---|--------|------|
| 1 | Project overview | [00_Project_Overview.md](00_Project_Overview.md) |
| 2 | Architecture | [01_Architecture.md](01_Architecture.md) |
| 3 | Folder tree & dependencies | [01_Architecture.md](01_Architecture.md) (+ [08_Design.md](08_Design.md)) |
| 4 | Database | [02_Database.md](02_Database.md) |
| 5 | Entities / ERD | [02_Database.md](02_Database.md), [04_Class_Documentation.md](04_Class_Documentation.md) |
| 6 | Program / startup flow | [00_Project_Overview.md](00_Project_Overview.md), [01_Architecture.md](01_Architecture.md) |
| 7 | Modules (by role & feature) | [03_Modules.md](03_Modules.md) |
| 8 | Class inventory | [04_Class_Documentation.md](04_Class_Documentation.md) |
| 9 | Method documentation | [05_Method_Documentation.md](05_Method_Documentation.md) |
| 10 | Variables / constants / session | [04_Class_Documentation.md](04_Class_Documentation.md), [10_Onboarding.md](10_Onboarding.md) |
| 11 | API | [07_API.md](07_API.md) |
| 12 | Events (UI + domain notifications) | [03_Modules.md](03_Modules.md), [08_Design.md](08_Design.md) |
| 13 | Algorithms (pricing, proration, hash) | [06_Business_Logic.md](06_Business_Logic.md), [08_Design.md](08_Design.md) |
| 14 | Business logic & flows | [06_Business_Logic.md](06_Business_Logic.md) |
| 15 | Dependency graph | [01_Architecture.md](01_Architecture.md) |
| 16 | Call graphs (key flows) | [06_Business_Logic.md](06_Business_Logic.md) |
| 17 | Design patterns & UI design | [08_Design.md](08_Design.md) |
| 18–19 | Code review / gaps / risks | [09_Code_Review.md](09_Code_Review.md) |
| 20 | Onboarding (dev setup) | [10_Onboarding.md](10_Onboarding.md) |

Related (outside this folder): shared multi-machine SQL Server setup → [../13_Shared_SQLServer_Setup.md](../13_Shared_SQLServer_Setup.md).

---

## Quick facts (from code)

| Item | Value |
|------|--------|
| Entry | `RPMS.WinForms/Program.cs` |
| UI | Desktop WinForms only — **no HTTP API** |
| Connection string | Hardcoded in `Program.ConnectionString` → `Server=.\SQLEXPRESS;Database=RPMS;...` |
| Roles (IDs in UI) | 1 Admin · 2 Landlord · 3 Tenant · 4 Manager |
| Schema bootstrap | `Database/RPMS_Full.sql` + `DatabaseSchemaUpdater.EnsureUpdatedAsync` + `DataSeeder.SeedAsync` |
| Demo users (seeder) | `admin`/`admin123`; `namlandlord`, `tenant`, `manager` / `123456` |

---

## Coverage notes

See the footer of [09_Code_Review.md](09_Code_Review.md) and the delivery summary returned with this documentation set for “fully documented vs summarized” and any **chưa đọc hết** markers.
