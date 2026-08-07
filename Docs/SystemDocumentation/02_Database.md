# 02 — Database

**Section mapping:** §4 Database, §5 Entities/ERD

Sources: `Database/RPMS_Full.sql`, `RPMS.DAL/Entities/*`, `RPMS.DAL/Configurations/*`, `RPMS.DAL/DatabaseSchemaUpdater.cs`, `RPMS.BLL/DataSeeder.cs`, `RPMS.WinForms/Program.cs`.

---

## 1. Engine & connection

| Item | Value |
|------|--------|
| RDBMS | Microsoft SQL Server |
| Database name | `RPMS` |
| Default instance (app) | `.\SQLEXPRESS` |
| Auth | Trusted_Connection + TrustServerCertificate |
| MARS | Enabled |

No `appsettings.json`. Connection string lives in `Program.ConnectionString`.

---

## 2. How schema is created

| Step | Artifact |
|------|----------|
| Manual full script | `Database/RPMS_Full.sql` — DROP DB if exists, CREATE DB, tables, CHECKs, indexes, sample data |
| App startup | `DatabaseSchemaUpdater.EnsureUpdatedAsync` then `DataSeeder.SeedAsync` |
| EF model | `RPMSContext` + Fluent configurations applied via `ApplyConfigurationsFromAssembly` |

**Note:** Script file path for MDF/LDF is hardcoded to `C:\Users\ACER\RPMS\...` — must be adjusted per machine before running SQL.

---

## 3. ERD (logical)

```mermaid
erDiagram
  Roles ||--o{ Users : has
  Users ||--o{ Houses : owns
  Houses ||--o{ Rooms : contains
  Rooms ||--o{ RoomImages : has
  Amenities ||--o{ RoomAmenities : listed
  Rooms ||--o{ RoomAmenities : has
  Rooms ||--o{ Posts : advertised
  Posts ||--o{ PostImages : has
  Users ||--o{ Favorites : saves
  Rooms ||--o{ Favorites : favorited
  Rooms ||--o{ Appointments : visit
  Users ||--o{ Appointments : books
  Rooms ||--o{ Contracts : rented
  Users ||--o{ Contracts : tenant
  Contracts ||--o| Reviews : one
  Contracts ||--o{ MeterReadings : meters
  MeterReadings ||--o{ Invoices : billed
  Contracts ||--o{ Invoices : billed
  Invoices ||--o{ Payments : pays
  Contracts ||--o{ MaintenanceRequests : issues
  Houses ||--o{ Assignments : managed
  Users ||--o{ Assignments : manager
  Users ||--o{ Notifications : receives
  Users ||--o{ ActivityLogs : acts
  Users ||--o{ ChatConversations : landlord
  Users ||--o{ ChatConversations : tenant
  ChatConversations ||--o{ ChatMessages : contains
```

Chat tables exist in EF + schema updater; **not** in original `CREATE TABLE` list of `RPMS_Full.sql`.

---

## 4. Tables & status CHECK constraints (SQL script)

| Table | PK | Notable CHECKs / uniques |
|-------|-----|---------------------------|
| Roles | RoleID | Unique RoleName |
| Users | UserID | Status `Active`/`Inactive`; unique Email, Username |
| Houses | HouseID | Status `Active`/`Inactive` |
| Rooms | RoomID | Status `Available`/`Occupied`/`Maintenance`; unique (HouseID, RoomNumber) |
| RoomImages | ImageID | Cascade from Room |
| Amenities | AmenityID | Unique AmenityName |
| RoomAmenities | RoomAmenityID | Unique (RoomID, AmenityID) |
| Posts | PostID | Status `Pending`/`Approved`/`Rejected` |
| PostImages | PostImageID | Cascade from Post |
| Favorites | FavoriteID | Unique (UserID, RoomID) |
| Appointments | AppointmentID | Status `Pending`/`Accepted`/`Rejected`/`Completed` (**script has no Cancelled**; EF/BLL uses Cancelled — see review) |
| Contracts | ContractID | Status `Draft`/`PendingConfirm`/`Active`/`Expired`/`Terminated`; TenantID nullable in script |
| MeterReadings | ReadingID | New ≥ Old electric/water |
| Invoices | InvoiceID | Status `Unpaid`/`Paid`/`Overdue` |
| Payments | PaymentID | Method Cash/Banking/Momo/VNPay/ZaloPay; Status Pending/Completed/Failed |
| MaintenanceRequests | RequestID | Pending/Processing/Completed |
| Assignments | AssignmentID | Active/Inactive; unique (HouseID, ManagerID) |
| Reviews | ReviewID | Rating 1–5; unique ContractID (**script lacks LandlordReply** — patched at runtime) |
| Notifications | NotificationID | (**script lacks ActionType/RelatedID/ActionStatus** — patched) |
| ActivityLogs | LogID | — |

---

## 5. Runtime schema patches (`DatabaseSchemaUpdater`)

File: `RPMS.DAL/DatabaseSchemaUpdater.cs`

| Patch | Purpose |
|-------|---------|
| `EnsureCreatedAsync` | Create from model if DB empty |
| Reviews.LandlordReply, LandlordReplyDate | Landlord reply feature |
| ChatConversations + ChatMessages | Chat feature |
| Contracts.TenantID → NULL | Draft contracts without tenant |
| CK_Contracts_Status ensure Draft + PendingConfirm | Align CHECK with app |
| Pending* / Previous* / PriceEffectiveDate / CancelRequest* columns | Contract edit & cancel workflow |
| Notifications.ActionType, RelatedID, ActionStatus | Actionable notifications |
| INSERT missing Amenities | Catalog completeness |

Contract pending/cancel columns (exact names from updater):

- `PendingMonthlyRent`, `PendingElectricPrice`, `PendingWaterPrice`, `PendingDeposit`, `PendingEndDate`, `PendingEditStatus`, `PendingEditNote`, `PendingEditAt`
- `PreviousMonthlyRent`, `PreviousElectricPrice`, `PreviousWaterPrice`, `PriceEffectiveDate`
- `CancelRequestStatus`, `CancelRequestedBy`, `CancelRequestNote`, `CancelRequestAt`

---

## 6. Entity property summary (DAL)

Full property lists for all entities are also in [04_Class_Documentation.md](04_Class_Documentation.md). Highlights:

### Contract (`RPMS.DAL/Entities/Contract.cs`)

Core: `ContractCode`, `RoomID`, `TenantID?`, dates, `Deposit`, `MonthlyRent`, utility prices, `Status`, `CreatedBy`.

Workflow: pending edit fields + previous prices + cancel request fields (see §5).

Nav: `Room`, `Tenant`, `CreatedByUser`, `Review`, `MeterReadings`, `Invoices`, `MaintenanceRequests`.

### Notification (`RPMS.DAL/Entities/Notification.cs`)

`Title`, `Content`, `IsRead`, `ActionType?`, `RelatedID?`, `ActionStatus?`.

Action types (code constants): `ContractEdit`, `ContractCancel` (`RPMS.Common/Constants/NotificationActions.cs`).

### User / Role

Roles seeded as Admin, Landlord, Tenant, Manager. Users hold BCrypt hash in `Password`.

---

## 7. DbContext

`RPMS.DAL/Data/RPMSContext.cs` exposes `DbSet<>` for all entities listed in §4 plus Chat*. Configurations auto-applied from assembly.

---

## 8. Indexes (script)

`RPMS_Full.sql` creates indexes on Username, RoleID, Status, House OwnerID, Room HouseID/Status/Price, Posts Status/Expiry/Featured, Favorites, Appointments, Contracts Room/Tenant/Status/Code, and further indexes for invoices/payments/etc. (remainder of script after line ~325 — **chưa liệt kê từng dòng index còn lại**; pattern is FK/status/code columns).

---

## 9. DataSeeder behavior

File: `RPMS.BLL/DataSeeder.cs`

Does **not** rebuild the whole sample world if tables already have data. It:

1. Hashes plaintext passwords for known usernames (`admin`→`admin123`; `namlandlord`/`tenant`/`manager`/`landlord1`/`tenant1`/`manager1`→`123456`) if not already `$2a$`/`$2b$`/`$2y$`; forces `Active`.
2. Fixes Vietnamese mojibake on sample names/addresses/amenities/posts/etc.
3. Ensures amenity catalog names.
4. If ContractID=1 exists: syncs a rolling sample timeline (move-in, meters, invoices paid/unpaid, appointment).
5. If empty DB: insert 4 roles + 4 demo users.

---

## 10. Ancillary SQL / tools

| File | Purpose |
|------|---------|
| `Database/Fix_Unicode_Sample.sql` | Sample Unicode fixes |
| `Database/FixUnicode/Program.cs` | Console UPDATEs + verify file |
| `Database/README_ENCODING.md` | Encoding notes |
| `BCryptHelper/Program.cs` | Print hashes for `admin123` / `123456` |

---

## 11. Divergence notes (script vs app)

| Topic | Observation from code |
|-------|------------------------|
| Appointment Cancelled | BLL/Landlord allow `Cancelled`; SQL CHECK in script may omit it |
| Room Inactive | BLL references skipping `Inactive` rooms in drafts; SQL Rooms CHECK may omit Inactive |
| Invoice Overdue | In SQL CHECK; BLL payment flow focuses Unpaid/Paid |
| Contract Expired | In CHECK + ReviewService; ContractService terminate uses `Terminated` only |
| Chat / notification actions / review reply | App-only via schema updater |

---

## Coverage

Schema CREATE TABLE block of `RPMS_Full.sql` read through ActivityLogs; indexes partially; sample INSERT data **not** fully transcribed. All entity classes and `DatabaseSchemaUpdater` read. Fluent configurations summarized from inventory pass (key CHECKs/FKs).
