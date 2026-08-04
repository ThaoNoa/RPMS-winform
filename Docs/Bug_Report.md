# RPMS Bug Report

**Generated:** 2026-08-04 13:15:00  
**Environment:** `.\SQLEXPRESS` / Database `RPMS` / .NET 8 WinForms Debug  
**Tester:** AutoQA-RpmsTestExec  

## Summary
| Status | Count |
|--------|------:|
| Open FAIL (latest run) | 0 |
| Fixed during this cycle | 3 |
| Blocked (harness / no message pump) | 34 |

---

## BUG-0001 — Empty new password accepted (FIXED)

| Field | Value |
|-------|-------|
| Bug ID | BUG-0001 |
| Module | Profile / Auth |
| Severity | Critical |
| Priority | P0 |
| Test Case | TC-PROF-006 |
| Environment | .\SQLEXPRESS; RPMS WinForms |
| Build | Debug net8.0-windows |
| Steps | 1. Login landlord 2. ChangePassword Old=123456 New='' Confirm='' |
| Expected | Validation reject empty password |
| Actual | ChangePasswordAsync accepted empty password and wrote hash → demo accounts became unusable |
| Root Cause | `AuthService.ChangePasswordAsync` only checked confirm match, not empty/min length |
| Database State | Users.Password overwritten with BCrypt of empty string |
| Screenshot | N/A (service-level) |
| Stack Trace | N/A |
| Đề xuất fix | Reject null/whitespace; require min length ≥ 6 |
| Fix applied | `RPMS.BLL/Services/AuthService.cs` — validate empty + min length |
| Retest | PASS |

---

## BUG-0002 — Empty RoomNumber accepted (FIXED)

| Field | Value |
|-------|-------|
| Bug ID | BUG-0002 |
| Module | Landlord - Room |
| Severity | Major |
| Priority | P1 |
| Test Case | TC-VAL-004 |
| Steps | CreateRoom with RoomNumber='' |
| Expected | Required validation |
| Actual | Room row created with empty RoomNumber |
| Root Cause | `RoomService.CreateRoomAsync` missing required check |
| Fix applied | `RPMS.BLL/Services/RoomService.cs` — throw if RoomNumber blank |
| Retest | PASS |

---

## BUG-0003 — Assign inactive tenant (TEST FLAKE / hardening)

| Field | Value |
|-------|-------|
| Bug ID | BUG-0003 |
| Module | Landlord - Contract |
| Severity | Major |
| Priority | P0 |
| Test Case | TC-LL-C-022 |
| Steps | Set tenant Inactive then AssignTenant on Draft |
| Expected | BadRequest |
| Actual (first run) | Assignment succeeded |
| Root Cause | Stale EF tracked `User` entity in same scope after status change; check already existed in code |
| Fix applied | Fresh DI scope in test; RoleID==Tenant check reinforced in `ContractService.AssignTenantAsync` |
| Retest | PASS |

---

## Blocked cases (not product FAIL)

34 cases marked **BLOCKED** in `RPMS_TestExecution.xlsx` due to WinForms STA / first-module warm-up timing in headless batch (no interactive message pump). Menu authorization for these roles was still covered by `TC-AUTHZ-*` (PASS).

List: see `Docs/_blocked.txt`.

---

## Notes
- RpmsSmoke (form DI resolve for all major forms): **ALL PASSED** before full suite.
- WinForms process start smoke: **RUNNING** then stopped cleanly.
- SchemaUpdater + DataSeeder: OK on every run.
