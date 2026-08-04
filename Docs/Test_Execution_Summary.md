# RPMS Test Execution Summary

**Generated:** 2026-08-04 13:15:00  
**Tester:** AutoQA-RpmsTestExec  
**Environment:** SQL Server `.\SQLEXPRESS` / Database `RPMS` / .NET 8 WinForms  
**Duration (latest full pass):** 12.5 minutes  
**Harness:** `tools/RpmsTestExec` (real BLL + SQL + Auth session; WinForms resolve via RpmsSmoke)

## Totals (latest complete run)
| Metric | Value |
|--------|------:|
| Total test cases executed | 441 |
| Passed | 407 |
| Failed | 0 |
| Blocked | 34 |
| Pass Rate | **92.29%** |
| Open product bugs | 0 |
| Bugs found & fixed this cycle | 3 |

## Phase checklist
| Phase | Result |
|-------|--------|
| 1. Read source/docs/DB | Done |
| 2. Build `RPMS.sln` | Succeeded (0 errors) |
| 3. SQL Express + SchemaUpdater + Seeder | OK (Users/Houses/Rooms/Contracts seeded) |
| 4. WinForms launch | Process started (LoginForm); stopped after health check |
| RpmsSmoke (login all roles + form resolve) | ALL PASSED |
| Full suite 441 TC | Executed → reports below |

## Severity summary (fixed bugs)
| Severity | Count |
|----------|------:|
| Critical | 1 (empty password) |
| Major | 2 (empty room number; inactive tenant hardening) |

## Module coverage
See sheet `Summary` in `RPMS_TestExecution.xlsx` and module rows in prior auto-generated section. Highlights:
- Authentication / Authorization: full PASS
- Database integrity / Status transitions / Notifications / Regression hotspots: PASS
- Smoke Suite: BLOCKED in batch (covered by RpmsSmoke PASS)
- Registration / some Shared Chat: partial BLOCKED (harness)

## Bug list
See `Docs/Bug_Report.md` — BUG-0001..0003 **FIXED**, retested PASS.

## Blocked (34)
Not counted as product FAIL. Cause: headless batch STA timeout/warm-up when constructing heavy WinForms without message loop. Manual or FlaUI UI pass recommended for:
`TC-ADM-U-001`, `TC-ADM-P-001`, `TC-LL-H-001`, `TC-LL-R-001`, `TC-LL-A-001`, `TC-LL-C-001`, `TC-LL-AP-001`, `TC-TN-S-001`, `TC-TN-C-001`, `TC-TN-I-001`, `TC-MG-M-001`, `TC-MG-MT-001`, `TC-SMOKE-001..004`, `TC-REGU-002..004/007`, chat/calendar/security warm-ups — full list in `Docs/_blocked.txt`.

## Artifacts
| File | Description |
|------|-------------|
| `Docs/RPMS_TestCases.xlsx` | 441 designed test cases |
| `Docs/RPMS_TestExecution.xlsx` | Execution results (Expected/Actual/Status/Time/Bug ID) |
| `Docs/Bug_Report.xlsx` | Bug workbook (latest run had 0 open FAIL) |
| `Docs/Bug_Report.md` | Bug narrative + fixed defects |
| `Docs/Test_Execution_Summary.md` | This file |
| `Docs/test_run_log.txt` | Console log (earlier run) |
| `tools/RpmsTestExec` | Re-runnable executor |
| `tools/RpmsSmoke` | Form resolve + role smoke |

## How to re-run
```powershell
dotnet build E:\DoAn\RPMS\RPMS.sln -c Debug
dotnet run --project E:\DoAn\RPMS\tools\RpmsSmoke\RpmsSmoke.csproj -c Debug
dotnet run --project E:\DoAn\RPMS\tools\RpmsTestExec\RpmsTestExec.csproj -c Debug
```

## Product fixes shipped in this QA cycle
1. `AuthService.ChangePasswordAsync` — reject empty / short password  
2. `RoomService.CreateRoomAsync` — reject empty RoomNumber  
3. `ContractService.AssignTenantAsync` — explicit Tenant RoleID check  
