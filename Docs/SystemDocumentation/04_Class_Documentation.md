# 04 — Class Documentation

**Section mapping:** §5 Entities (summary), §8 Classes, §10 Variables/constants

Inventory of **public/internal types** across RPMS.* projects, BCryptHelper, tools, FixUnicode. Grouped by project/namespace. For Designer-heavy forms, Designer fields summarized; business surface listed.

---

## RPMS.Common

### `RPMS.Common.Constants`

| Type | File | Purpose / members |
|------|------|-------------------|
| `AppColors` | `Constants/AppColors.cs` | Static `Color` theme (Primary, Sidebar, Success, Danger, Background, Card, TextMuted, …) |
| `AppLayout` | `Constants/AppLayout.cs` | Spacing/size constants (`PagePadding`, dialog mins, …) |
| `AppTypography` | `Constants/AppTypography.cs` | Segoe UI Font accessors (`Title`, `Heading`, `Body`, …) |
| `NotificationActions` | `Constants/NotificationActions.cs` | `ContractEdit`, `ContractCancel`, `Pending`, `Completed`, `Declined` |

### `RPMS.Common.Globals`

| Type | File | Members |
|------|------|---------|
| `UserSession` | `Globals/UserSession.cs` | `CurrentUser`, `Login`, `Logout`, `IsLoggedIn` |

### Placeholder

| `Class1` | `Class1.cs` | Empty scaffold |

---

## RPMS.DTO

Placeholder: `Class1`. All others are POCOs (properties only unless noted).

### Auth
- `LoginRequestDto` — Username, Password  
- `LoginResponseDto` — UserID, FullName, Username, RoleID, RoleName, Token  
- `ChangePasswordDto` — OldPassword, NewPassword, ConfirmNewPassword  

### User / Role
- `UserDto`, `CreateUserDto`, `UpdateUserDto`  
- `RoleDto` — RoleID, RoleName  

### House / Room / Amenity
- `HouseDto`, `CreateHouseDto`, `UpdateHouseDto`  
- `RoomDto`, `RoomDetailDto` (: RoomDto + Capacity, Bedroom, Bathroom, Furniture, Description, Images, Amenities)  
- `CreateRoomDto`, `UpdateRoomDto`  
- `AmenityDto`, `CreateAmenityDto`  

### Post
- `PostDto`, `PostDetailDto`, `CreatePostDto`, `RoomSearchFilterDto` (filters/sort flags)  

### Contract
- `ContractDto` (+ pending cancel/edit display fields, `CancelRequestLabel`)  
- `ContractDetailDto`, `CreateContractDto`, `UpdateContractDto`, `AssignTenantDto`  
- `BulkCreateDraftContractsDto`, `BulkCreateDraftContractsResultDto`  

### Invoice
- `InvoiceDto`, `InvoiceDetailDto`, `GenerateInvoiceDto`, `ProcessPaymentDto`, `MeterReadingSummaryDto`  

### Maintenance / Assignment / Review / Notification / ActivityLog
- Matching DTO files under `Maintenance/`, `Assignment/`, `Review/`, `Notification/`, `ActivityLog/`  

### Tenant
- `AppointmentDto`, `CreateAppointmentDto`, `FavoriteDto`, `TenantDashboardDto`  

### Chat (`ChatDto.cs`)
- `ConversationDto`, `ChatMessageDto`, `SendMessageDto`  

### Calendar
- `CalendarEventDto`, `CalendarDayDto`, enum `ColorHint`  

### Statistic / Report
- `AdminDashboardDto`, `NamedCountDto`, `LandlordDashboardDto`, `ManagerDashboardDto`, `RevenueChartData`  
- `ReportSummaryDto`  

---

## RPMS.DAL

### Root
| Type | File |
|------|------|
| `DalDependencyInjection` | `DalDependencyInjection.cs` — `AddDataAccessLayer` |
| `DatabaseSchemaUpdater` | `DatabaseSchemaUpdater.cs` — `EnsureUpdatedAsync` |

### Data
| `RPMSContext` | `Data/RPMSContext.cs` | All DbSets + OnModelCreating |

### Entities (`RPMS.DAL/Entities`)

| Entity | Key properties |
|--------|----------------|
| `Role` | RoleID, RoleName |
| `User` | UserID, RoleID, FullName, Phone, Email, Username, Password, Address, Status, timestamps + nav collections |
| `House` | HouseID, OwnerID, HouseName, Address, Description, Status |
| `Room` | RoomID, HouseID, RoomNumber, Floor, Area, Price, Capacity, Bedroom, Bathroom, Furniture, Status, Description |
| `RoomImage` | ImageID, RoomID, ImagePath, DisplayOrder |
| `Amenity` | AmenityID, AmenityName |
| `RoomAmenity` | RoomAmenityID, RoomID, AmenityID |
| `Post` | PostID, RoomID, Title, Description, PriceSnapshot, Status, ViewCount, ExpiryDate, IsFeatured, ApprovedBy, ApprovedDate |
| `PostImage` | PostImageID, PostID, ImagePath, IsMain, DisplayOrder |
| `Favorite` | FavoriteID, UserID, RoomID |
| `Appointment` | AppointmentID, RoomID, TenantID, AppointmentDate, Status, Note |
| `Contract` | See [02_Database.md](02_Database.md) — includes pending edit/cancel + previous price fields |
| `Review` | ReviewID, ContractID, Rating, Comment, LandlordReply, LandlordReplyDate |
| `MeterReading` | ReadingID, ContractID, ReadingMonth, Old/New Electric/Water, CreatedBy |
| `Invoice` | InvoiceID, InvoiceCode, ContractID, ReadingID, Rent, ElectricCost, WaterCost, OtherFee, Total, Status, DueDate, PaidDate |
| `Payment` | PaymentID, InvoiceID, PaymentDate, Amount, Method, Status |
| `MaintenanceRequest` | RequestID, ContractID, Title, Description, Image, Status, AssignedManager, CompletedDate |
| `Assignment` | AssignmentID, HouseID, ManagerID, AssignedDate, Status |
| `Notification` | NotificationID, UserID, Title, Content, IsRead, ActionType, RelatedID, ActionStatus |
| `ActivityLog` | LogID, UserID, Action, Details, IPAddress, CreatedDate |
| `ChatConversation` | ConversationID, LandlordID, TenantID, LastMessageAt |
| `ChatMessage` | MessageID, ConversationID, SenderID, Content, ImagePath, IsRead |

### Configurations
22 classes `*Configuration : IEntityTypeConfiguration<T>` under `Configurations/` — table names, keys, FKs, unique indexes, status CHECKs aligned with EF model (may be ahead of raw SQL script).

### Repositories
- `IGenericRepository<T>` / `GenericRepository<T>`  
- Marker interfaces + classes: Role, User, House, Room, RoomImage, Amenity, RoomAmenity, Post, PostImage, Favorite, Appointment, Contract, Review, MeterReading, Invoice, Payment, MaintenanceRequest, Assignment, Notification, ActivityLog, ChatConversation, ChatMessage  

### Unit of Work
- `IUnitOfWork` / `UnitOfWork` — repo properties + SaveChanges + transactions + Dispose  

---

## RPMS.BLL

| Type | File | Role |
|------|------|------|
| `BllDependencyInjection` | `BllDependencyInjection.cs` | Register AutoMapper + scoped services |
| `DataSeeder` | `DataSeeder.cs` | Startup seed/fix |
| `MappingProfile` | `Mappings/MappingProfile.cs` | AutoMapper Profile |
| `RPMSException`, `NotFoundException`, `BadRequestException`, `UnauthorizedException` | `Exceptions/RPMSException.cs` | Domain exceptions |
| `PasswordHelper` | `Helpers/PasswordHelper.cs` | Hash/Verify BCrypt |
| `RentProrationHelper`, `RentProrationResult` | `Helpers/RentProrationHelper.cs` | Day-based rent |
| `ContractPricingHelper` | `Helpers/ContractPricingHelper.cs` | Mid-month price weighting |

### Interfaces + Services (paired)

| Interface | Implementation |
|-----------|----------------|
| `IAuthService` | `AuthService` |
| `IUserService` | `UserService` |
| `IRoleService` | `RoleService` |
| `IHouseService` | `HouseService` |
| `IRoomService` | `RoomService` |
| `IAmenityService` | `AmenityService` |
| `IPostService` | `PostService` |
| `IContractService` | `ContractService` |
| `IInvoiceService` | `InvoiceService` |
| `IMaintenanceService` | `MaintenanceService` |
| `IStatisticService` | `StatisticService` |
| `ITenantInteractionService` | `TenantInteractionService` |
| `ILandlordService` | `LandlordService` |
| `ITenantService` | `TenantService` |
| `INotificationService` | `NotificationService` |
| `IAssignmentService` | `AssignmentService` |
| `IActivityLogService` | `ActivityLogService` |
| `IReviewService` | `ReviewService` |
| `IChatService` | `ChatService` |
| `ICalendarService` | `CalendarService` |
| `IReportService` | `ReportService` |
| `IBackupService` | **`BackupService` missing on disk** |

Method signatures: [05_Method_Documentation.md](05_Method_Documentation.md).

---

## RPMS.WinForms

### Root
| `Program` (internal static) | `Program.cs` | `ServiceProvider`, `ConnectionString`, `Main`, `ConfigureServices` |

### Forms.Auth
| Form | Designer | Business surface |
|------|----------|------------------|
| `LoginForm` | yes | ctor(IAuthService); `btnLogin_Click`; `lblRegisterLink_Click`; `ShowError` |
| `RegisterForm` | yes | Load roles; `btnRegister_Click`; `RegisteredUsername` prop |

### Forms.Layout / Dashboard
| `MainForm` | yes | `GenerateMenu`, `LoadChildForm`, `OpenChildForm`, `btnLogout_Click` |
| `DashboardForm` | no | Role-based KPI cards/charts |

### Forms.Admin
| Form | Notes |
|------|-------|
| `UserManagementForm` / `UserModalForm` | Designer; CRUD/toggle |
| `PostManagementForm` / `PostDetailModalForm` | Approve/reject |
| `ActivityLogForm` | Grid of logs |
| `ReviewManagementForm` | Admin review list |
| `BackupForm` | **Missing file** (still in DI) |

### Forms.Landlord
| Form | Notes |
|------|-------|
| House/Room forms + modals | Designer partials; CRUD |
| `LandlordContractForm` | Large code-built UI; contract lifecycle |
| `LandlordAppointmentForm`, `LandlordPostForm`, `LandlordReviewForm`, `LandlordAssignmentForm` | Code UI |

### Forms.Tenant
| Form | Notes |
|------|-------|
| `TenantHomeForm`, `RoomDetailForm` | Search/detail/book/favorite |
| `TenantAppointmentModalForm`, `TenantFavoriteForm`, `TenantContractForm` | |
| `TenantInvoiceForm`, `InvoiceDetailForm` | Export/print |
| `TenantMaintenanceForm`, `TenantReviewForm` | |

### Forms.Manager
| `ManagerMeterForm`, `ManagerMaintenanceForm`, `MaintenanceDetailForm` |

### Forms.Shared
| Type | File | Notes |
|------|------|-------|
| `NotificationCenterForm` | own file | Mark read / open actions |
| `ProfileForm` | | Profile + change password |
| `ChatForm` | | Conversations/messages (+ nested list item type) |
| `CalendarForm`, `ReportForm` | | |
| `ContractDetailViewForm`, `NotificationActionForm`, `NotificationDtoWrap` | `ContractNotificationUi.cs` | Contract detail + approve/decline edit/cancel |

### Controls
`ModernButton`, `ModernTextBox`, `ModernDataGridView`, `SidebarButton`, `SummaryCard`, `RoomCardControl` (events OnBook/Favorite/Card), `LoadingPanel`, `EmptyStatePanel`, `OccupancyChartPanel`, `StatusTimelineControl`, `ToastNotifier` + enum `ToastKind`.

### UI helpers
`UIHelper`, `AppDialog`, `ExportHelper`, `ImagePathHelper`, `InvoicePrintHelper`, `ContractPrintHelper`, `MaintenancePrintHelper`.

---

## BCryptHelper

Top-level statements in `Program.cs` — **no named types**. Hashes `admin123` and `123456`.

---

## tools/*

| Project | Types |
|---------|-------|
| `RpmsSmoke` | `Program` — DI resolve smoke |
| `RpmsTestExec` | `Program`, `TestCaseRow`, `ExecResult`, `BugItem` (`Models.cs`) |
| `RpmsE2EFlows` | `Program` + nested `LoginFormProxy`, `StepResult`, `BugItem` |

Python scripts `gen_rpms_testcases.py`, `gen_rpms_doc.py`, `strip_forms.py` — not C# types.

---

## Database/FixUnicode

Top-level statements — **no named types**.

---

## Variables / constants (quick index)

| Kind | Location |
|------|----------|
| Connection string | `Program.ConnectionString` |
| Role IDs | Hardcoded 1–4 in MainForm / services |
| Notification action strings | `NotificationActions` |
| UI theme | `AppColors`, `AppTypography`, `AppLayout` |
| Session | `UserSession.CurrentUser` |
| Status strings | See [06_Business_Logic.md](06_Business_Logic.md) status tables |

---

## Coverage

Public type inventory intended to be **complete** for RPMS.* + tools. Designer field names inside `*.Designer.cs` **summarized** (not every `private System.Windows.Forms.Button`). Private helpers inside mega-forms **not** exhaustively listed.
