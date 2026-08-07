# 07 — API

**Section mapping:** §11 API

---

## 1. Verdict: no HTTP API

`RPMS.sln` contains **no** ASP.NET Core / Web API / gRPC / SignalR host project.  
`RPMS.WinForms` is `OutputType=WinExe` (`net8.0-windows`).

There are **no** REST controllers, minimal APIs, OpenAPI specs, or HTTP endpoint attributes in the RPMS.* projects reviewed.

All “API” for the application is **in-process service interfaces** resolved via Microsoft.Extensions.DependencyInjection.

---

## 2. Internal service contract surface

Registered in `RPMS.BLL/BllDependencyInjection.cs` (scoped) unless noted:

| Interface | Implementation | Domain |
|-----------|----------------|--------|
| `IAuthService` | `AuthService` | Login, change/reset password |
| `IUserService` | `UserService` | User CRUD/status |
| `IRoleService` | `RoleService` | List roles |
| `IHouseService` | `HouseService` | Houses |
| `IRoomService` | `RoomService` | Rooms/images/amenities |
| `IAmenityService` | `AmenityService` | Amenity catalog |
| `IPostService` | `PostService` | Posts moderation |
| `IContractService` | `ContractService` | Contracts lifecycle |
| `IInvoiceService` | `InvoiceService` | Invoices/payments/meters |
| `IMaintenanceService` | `MaintenanceService` | Maintenance |
| `IStatisticService` | `StatisticService` | Dashboards |
| `ITenantInteractionService` | `TenantInteractionService` | Appointments/favorites |
| `ILandlordService` | `LandlordService` | Landlord appointments/broadcast |
| `ITenantService` | `TenantService` | Tenant dashboard/search/request |
| `INotificationService` | `NotificationService` | Notifications |
| `IAssignmentService` | `AssignmentService` | Manager assignments |
| `IActivityLogService` | `ActivityLogService` | Audit log |
| `IReviewService` | `ReviewService` | Reviews |
| `IChatService` | `ChatService` | Chat |
| `ICalendarService` | `CalendarService` | Calendar events |
| `IReportService` | `ReportService` | Reports |
| `IBackupService` | *(missing)* | Registered as Singleton in WinForms `Program` |

DAL “API”: `IUnitOfWork` + `IGenericRepository<T>` (+ marker repos).

Full method lists: [05_Method_Documentation.md](05_Method_Documentation.md).

---

## 3. DTO contracts

Request/response shapes live under `RPMS.DTO/**`. WinForms binds controls to these DTOs; BLL returns them after AutoMapper/manual mapping. This is the closest equivalent to API request/response models.

---

## 4. Auth “token”

`LoginResponseDto.Token` is set to the literal `"JWT_TOKEN_MOCK"` in `AuthService`. No middleware validates it. Session is `UserSession.CurrentUser` in process memory.

---

## 5. External integrations

| Integration | Status in code |
|-------------|----------------|
| SQL Server | Yes — EF Core + SqlClient |
| Payment gateways (Momo/VNPay/ZaloPay) | Enum/CHECK values on `Payments.Method` only; `ProcessPaymentAsync` records method string, **no** gateway SDK calls found |
| Email/SMS | Not present |
| HTTP clients | Not used for product features |

---

## 6. Tools “APIs”

Console tools (`RpmsSmoke`, `RpmsTestExec`, `RpmsE2EFlows`) call the same BLL interfaces headlessly. They do not expose network endpoints.

---

## Coverage

Confirmed absence of HTTP projects via solution/csproj inventory and entry `Program.cs`. If a future Web API is added, this section would need rewrite.
