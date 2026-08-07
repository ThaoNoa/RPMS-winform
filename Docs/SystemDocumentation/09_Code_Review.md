# 09 — Code Review

**Section mapping:** §18–19 Review / risks / gaps

Read-only review based on structure, DI, schema updater, BLL rules, and missing files. **No application code was changed.**

---

## 1. Strengths

| Area | Evidence |
|------|----------|
| Clear layering | Separate Common/DTO/DAL/BLL/WinForms projects |
| Consistent DI | `AddDataAccessLayer` / `AddBusinessLogicLayer` |
| Rich domain for contracts | Draft/PendingConfirm/edit/cancel via notifications |
| Schema evolution awareness | `DatabaseSchemaUpdater` patches legacy SQL |
| Password hashing | BCrypt + seeder upgrades plaintext |
| Per-screen scopes | Avoids shared DbContext races on navigation |
| Shared UI tokens | AppColors/Typography/Layout |

---

## 2. Critical / high issues

### 2.1 Missing Backup implementation
- `Program.cs` registers `BackupService` and `BackupForm`.
- **No** `BackupService.cs` / `BackupForm.cs` on disk.
- Admin menu “Backup DB” will throw on DI resolve (or show build errors if solution compiled strictly).

### 2.2 SQL CHECK vs application status strings
- Appointments: BLL allows `Cancelled`; original SQL CHECK may omit it (`RPMS_Full.sql` list Pending/Accepted/Rejected/Completed).
- Rooms: BLL draft logic references `Inactive`; SQL Rooms CHECK may be Available/Occupied/Maintenance only.
- Runtime may throw on INSERT/UPDATE if constraints not updated.

### 2.3 Contract `Expired` never set in ContractService
- ReviewService allows reviews for `Expired`, but termination path sets `Terminated` only.
- Reviews after natural end-date may be blocked unless status updated elsewhere.

### 2.4 Connection string / paths hard-coded
- SQLEXPRESS instance in `Program.cs`.
- `RPMS_Full.sql` MDF path under `C:\Users\ACER\RPMS\` — not portable.
- LoginForm error text mentions LocalDB while connection uses SQLEXPRESS.

### 2.5 Mock JWT
- `Token = "JWT_TOKEN_MOCK"` — fine for desktop session, misleading if someone expects real JWT.

---

## 3. Medium issues

| Issue | Detail |
|-------|--------|
| Empty marker repositories | All domain queries live in services → fat services, harder testing |
| Notification mapping manual | Easy to drift from DTO |
| Maintenance status unconstrained | `UpdateRequestStatusAsync` accepts any string |
| Invoice Overdue | In SQL CHECK but no clear BLL job to flip Unpaid→Overdue |
| EnsureCreated + SQL script dual path | Can diverge from Fluent configs vs hand SQL |
| Tools not in .sln | Easy to forget CI coverage |
| AssignmentManagementForm | Referenced in tools/docs history; **missing** (landlord uses `LandlordAssignmentForm` instead) |

---

## 4. Low / maintainability

- Scaffold `Class1.cs` left in Common/DTO.
- Large code-behind forms (`LandlordContractForm`) mix UI + orchestration.
- ActivityLog IP often unused/null.
- Silent `catch { }` on sample images / logout logging.

---

## 5. Security notes

| Topic | Note |
|-------|------|
| Desktop local DB | Trusted connection OK for thesis/lab |
| Authorization | Mostly UI-level; verify each service method enforces ownership (many do for landlord/tenant; not uniformly audited here line-by-line) |
| SQL in schema updater | Parameterized via raw DDL strings (admin-only startup) — not user input |
| Payment methods | Labels only; no PCI concern yet |

---

## 6. Test / QA artifacts

Existing: `Docs/E2E_*`, `Docs/Bug_Report*`, `tools/RpmsE2EFlows`, `tools/RpmsTestExec`, `tools/RpmsSmoke`.  
These document prior bugs/flows; **this review does not re-execute tests**.

---

## 7. Documentation coverage notes

| Area | Status |
|------|--------|
| Solution/projects/NuGet/DI/startup | Fully documented |
| Entities + schema updater + SQL CREATE tables | Fully / mostly |
| BLL interfaces + primary behaviors | Fully documented |
| WinForms menu map + major handlers | Fully |
| Designer control inventories | Summarized |
| ContractService / LandlordContractForm every private branch | **Summarized — chưa đọc hết từng dòng** |
| RPMS_Full.sql sample INSERT rows | **Chưa đọc hết** |
| tools TestExec/E2E Program internals | **Summarized — chưa đọc hết** |
| Existing Word/PDF overview | Not copied; not fully re-verified |

---

## 8. Recommendations (documentation only — not implemented)

1. Restore or remove Backup DI registrations.
2. Align SQL CHECKs with BLL status vocabularies (or migrate via updater).
3. Add expiry job or status transition to `Expired`.
4. Externalize connection string.
5. Consider thinning ContractService / extracting domain policies.
6. Add automated tests around contract edit/cancel notification actions.

---

## Coverage

Based on structure reads + inventory/BLL exploration agents + targeted file reads. Not a formal security audit or full static analysis run.
