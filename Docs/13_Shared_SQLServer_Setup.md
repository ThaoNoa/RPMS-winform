# Hướng dẫn: Dùng chung một cơ sở dữ liệu SQL Server cho RPMS

**Mục tiêu:** Máy A chạy SQL Server (host), máy B (và các máy khác) kết nối tới database `RPMS` trên máy A rồi chạy ứng dụng RPMS WinForms — cùng một nguồn dữ liệu.

**Đối chiếu mã nguồn:** `RPMS.WinForms/Program.cs`, `RPMS.DAL/DatabaseSchemaUpdater.cs`, `RPMS.BLL/DataSeeder.cs`, `Database/RPMS_Full.sql`.

> Bản chính cũng có tại `E:\DocDoAn\13_Shared_SQLServer_Setup.md`.

---

## Connection string hiện tại trong code

Trong `RPMS.WinForms/Program.cs` (khoảng dòng 22–23), connection string **hardcode**, mặc định:

```csharp
public static string ConnectionString { get; private set; } =
    @"Server=.\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
```

| Thành phần | Giá trị mặc định |
|------------|------------------|
| Server | `.\SQLEXPRESS` (SQL Server Express local) |
| Database | `RPMS` |
| Auth | Windows (`Trusted_Connection=True`) |
| Khác | `TrustServerCertificate=True`, `MultipleActiveResultSets=True` |

> **Lưu ý:** Không có `appsettings.json`. Muốn máy B trỏ về máy A thì **sửa trực tiếp** `Program.ConnectionString` rồi build lại (tài liệu này không thay đổi mã ứng dụng).

Khi khởi động, `Main` gọi lần lượt:

1. `DatabaseSchemaUpdater.EnsureUpdatedAsync` — `EnsureCreatedAsync` + các patch schema (Chat, cột Reviews, …)
2. `DataSeeder.SeedAsync` — hash mật khẩu demo, sửa Unicode sample, seed tối thiểu nếu DB trống

Nếu lỗi DB, MessageBox gợi ý kiểm tra `.\SQLEXPRESS` và đã chạy script tạo database.

---

## Bước 1: Chuẩn bị trên máy chủ SQL Server (Máy A)

### 1.1. Cài đặt SQL Server Express (nếu chưa có)

1. Tải **SQL Server Express** từ trang chủ Microsoft.
2. Trong cài đặt, chọn **Mixed Mode Authentication** (Windows + SQL Server Authentication).
3. Đặt mật khẩu cho tài khoản `sa` và **ghi nhớ** mật khẩu này.

### 1.2. Cấu hình SQL Server cho phép kết nối từ xa

1. Mở **SQL Server Configuration Manager**.
2. Vào **SQL Server Network Configuration** → **Protocols for SQLEXPRESS** (hoặc tên instance bạn dùng).
3. Bật **TCP/IP** (Enable).
4. Chuột phải **TCP/IP** → **Properties** → tab **IP Addresses**.
5. Cuộn xuống **IPAll**:
   - Đặt **TCP Port** = `1433`
   - Để **TCP Dynamic Ports** trống
6. Khởi động lại dịch vụ: **SQL Server Services** → **SQL Server (SQLEXPRESS)** → **Restart**.

### 1.3. Mở cổng 1433 trên Windows Firewall

Trong **Windows Defender Firewall with Advanced Security**:

1. **Inbound Rules** → **New Rule** → **Port** → Next.
2. Chọn **TCP**, **Specific local ports:** `1433` → Next.
3. **Allow the connection** → Next.
4. Chọn profile (Domain / Private / Public) → Next.
5. Đặt tên rule (ví dụ `SQL Server 1433`) → **Finish**.

Hoặc bằng PowerShell (Admin):

```powershell
New-NetFirewallRule -DisplayName "SQL Server 1433" `
  -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

### 1.4. Tạo database RPMS

1. Mở **SQL Server Management Studio (SSMS)** và kết nối instance (Windows Auth hoặc `sa`).
2. Chạy script đầy đủ:

   ```text
   Database/RPMS_Full.sql
   ```

   (trong repo: `E:\DoAn\RPMS\Database\RPMS_Full.sql`)

3. Với **sqlcmd** (UTF-8 — xem `Database/README_ENCODING.md`):

   ```bat
   sqlcmd -S .\SQLEXPRESS -E -f 65001 -i Database\RPMS_Full.sql
   ```

> **Cảnh báo — đường dẫn `.mdf`:** Script tạo DB với path máy tạo script, ví dụ:
> `FILENAME = 'C:\Users\ACER\RPMS\RPMS.mdf'`.
> Trên máy A khác user/ổ đĩa, **sửa dòng `FILENAME`** trong `RPMS_Full.sql` cho phù hợp, tạo thư mục đích trước, rồi chạy lại.

> **Ghi chú:** Nếu chỉ tạo database rỗng, lúc mở app `DatabaseSchemaUpdater` gọi `EnsureCreatedAsync` + patch; `DataSeeder` có thể seed role/user tối thiểu khi bảng trống. **Khuyến nghị vẫn chạy `RPMS_Full.sql`** để có schema + indexes + dữ liệu mẫu đầy đủ hơn.

### 1.5. Cấp quyền cho tài khoản kết nối

Khuyến nghị dùng **SQL Authentication** (đơn giản hơn Windows Auth giữa các máy).

1. Trong SSMS: **Security** → **Logins** → **New Login**.
2. Tên ví dụ: `rpms_user`, chọn **SQL Server authentication**, đặt mật khẩu.
3. **User Mapping** → chọn database `RPMS` → gán role **`db_owner`** (đủ quyền cho schema updater / seeder / app).

Windows Authentication từ máy B cũng được, nhưng cần thêm Windows login của máy khách vào SQL Server (phức tạp hơn trên LAN thông thường).

---

## Bước 2: Trên máy khách (Máy B)

### 2.1. Cài .NET 8 SDK

Cần .NET 8 để build/chạy RPMS WinForms (nếu chưa có).

### 2.2. Kiểm tra mạng và SQL

```bat
ping <IP_may_A>
```

Thử `sqlcmd` (đổi IP, user, mật khẩu):

```bat
sqlcmd -S <IP_may_A>\SQLEXPRESS -U rpms_user -P <mat_khau>
```

Kết nối thành công → hiện dấu nhắc `1>`.

> Có thể thử trước bằng SSMS trên máy B với cùng server/login.

### 2.3. Sửa connection string trong `Program.cs`

Mở `RPMS.WinForms/Program.cs`, sửa **giá trị** của `ConnectionString` (giữ nguyên kiểu property `{ get; private set; }`).

**SQL Authentication (khuyến nghị):**

```csharp
public static string ConnectionString { get; private set; } =
    @"Server=192.168.1.100\SQLEXPRESS;Database=RPMS;User Id=rpms_user;Password=MAT_KHAU;TrustServerCertificate=True;MultipleActiveResultSets=True;";
```

Thay `192.168.1.100` bằng IP (hoặc tên máy) của máy A; thay `MAT_KHAU` cho đúng.

**Windows Authentication** (nếu đã cấp quyền Windows login trên máy A):

```csharp
public static string ConnectionString { get; private set; } =
    @"Server=192.168.1.100\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
```

> **Sửa so với bản nháp:** Code thật dùng `{ get; private set; } = @"...";`, **không** phải expression-bodied `=> $"Server={serverIp}\\..."`. Instance mặc định trong repo là `.\SQLEXPRESS`.

### 2.4. Build và chạy

```bat
dotnet build
dotnet run --project RPMS.WinForms
```

Hoặc F5 trong Visual Studio.

Lần đầu:

- Đã chạy `RPMS_Full.sql` → updater chủ yếu áp patch còn thiếu; seeder hash mật khẩu plain-text trong sample (nếu còn).
- DB trống / thiếu roles-users → seeder tạo tối thiểu để app không crash.

### 2.5. Kiểm tra đồng bộ

1. Đăng nhập tài khoản demo (xem bảng dưới).
2. Thao tác trên máy B (ví dụ tạo hợp đồng).
3. Kiểm tra trên máy A (app hoặc SSMS) — dữ liệu phải giống nhau.

#### Tài khoản demo (`DataSeeder` / sample SQL)

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Admin |
| `namlandlord` | `123456` | Landlord |
| `tenant` | `123456` | Tenant |
| `manager` | `123456` | Manager |

(Seeder cũng hash các user cũ `landlord1` / `tenant1` / `manager1` → `123456` nếu còn trong DB.)

---

## Bước 3: (Tùy chọn) Nhiều máy cùng kết nối

1. Lặp lại **Bước 2** trên mỗi máy khách (cùng connection string tới máy A).
2. Tất cả máy trong cùng LAN hoặc VPN.
3. Với SQL Auth: bảo mật mật khẩu login; không commit mật khẩu lên Git nếu repo công khai.

---

## Lưu ý quan trọng

> **TrustServerCertificate=True** — bỏ qua xác thực chứng chỉ TLS. Phù hợp môi trường nội bộ / lab; cân nhắc kỹ nếu đưa ra môi trường production.

> Instance không phải `SQLEXPRESS` → đổi tên trong connection string và trong Configuration Manager cho khớp.

> Script `Database/RPMS_Full.sql` dùng `DROP`/`CREATE DATABASE` — chạy lại sẽ **xóa dữ liệu** hiện có. Backup trước khi chạy lại trên máy đang dùng chung.

---

## Checklist lỗi thường gặp

| Triệu chứng | Việc kiểm tra |
|-------------|----------------|
| Timeout / không kết nối được | Ping IP máy A; TCP/IP đã Enable; service đã Restart; firewall port `1433` |
| Ping / SQL 1433 bị chặn dù cùng Wi‑Fi | Xem **Nguyên nhân đã xác định** bên dưới (Public profile + AP Isolation) |
| `Login failed` | User/password SQL; User Mapping tới DB `RPMS`; Mixed Mode đã bật |
| Kết nối được `sqlcmd` nhưng app lỗi | Connection string trên máy B đã đổi IP/instance/auth chưa; đã rebuild |
| Lỗi tạo DB khi chạy script | Sửa `FILENAME` `.mdf`/`.ldf` trong `RPMS_Full.sql`; tạo thư mục đích |
| Tiếng Việt bị mojibake | Chạy sqlcmd với `-f 65001` hoặc UTF-8 BOM trong SSMS; mở lại app để `DataSeeder` sửa tên sample |
| MessageBox “Không thể khởi tạo/cập nhật database” | Xem message exception; kiểm tra quyền `db_owner`; server/instance đúng |
| Instance named khác | Dùng `IP\TEN_INSTANCE` hoặc `IP,1433` nếu đã cố định port |
| Hai máy không cùng mạng | Cần LAN/VPN; Public Wi‑Fi / AP Isolation thường chặn |

---

## Nguyên nhân đã xác định (Wi‑Fi trường / firewall)

Khi Máy B **không ping được** Máy A và **không mở được cổng 1433**, thường do hai lớp sau (đã xác nhận trên lab):

1. **Wi‑Fi Máy A ở profile Public (`NetworkCategory = Public`)**  
   Windows Firewall trên mạng Public thường **chặn inbound** → mất ping (ICMPv4) và SQL TCP `1433`, dù đã thêm rule firewall nếu rule chỉ áp dụng Private/Domain.

2. **Wi‑Fi trường (`uneti.edu.vn 3`) thường bật AP Isolation**  
   Các client cùng SSID **không nói chuyện được với nhau**. Firewall trên Máy A đúng rồi vẫn fail vì AP chặn peer-to-peer.

### Cách xử lý

**Bước 1 — Chạy lại script Admin trên Máy A**

`Configure_Machine_A_SQL.ps1` (bản mới) sẽ:

- đặt Wi-Fi sang **Private** (nếu được)
- mở firewall **TCP 1433 / UDP 1434 + ICMPv4** cho **mọi profile** (**Domain / Private / Public**) — kể cả khi Wi-Fi vẫn ở Public
- (cùng các bước cũ: TCP/IP, login SQL, …)
```powershell
powershell -ExecutionPolicy Bypass -File "d:\ontap.Nett\RPMS\Configure_Machine_A_SQL.ps1"
```

**Bước 2 — Nếu B vẫn không ping được IP Máy A trên Wi‑Fi trường**

Dùng **Mobile Hotspot** (vd. phone hotspot **"NamBe 2"**), rồi kết nối Máy B vào hotspot đó (mạng 1‑1, bỏ qua AP Isolation của Wi‑Fi trường).

> **Lưu ý IP:** Sau khi đổi mạng/hotspot, IP Máy A **có thể đổi**. Máy B phải sửa `Program.cs` `Server=` theo IP mới. IP đang dùng trên phone hotspot **"NamBe 2"** có thể là **`172.20.10.2`** (trước đây trên Wi‑Fi trường từng là `172.31.141.192`). Host Windows Mobile Hotspot thường là **`192.168.137.1`**.

Kiểm tra nhanh trên Máy B:

```powershell
ping <IP_may_A>
Test-NetConnection -ComputerName <IP_may_A> -Port 1433
```

---

## 1-Click Máy A (PowerShell)

Trên **Máy A**, chạy script cấu hình SQL (TCP/IP, firewall **1433/1434 + ICMP trên Domain/Private/Public**, **Private Wi-Fi** nếu được, login `rpms_user`, …) bằng **PowerShell Admin**:

```powershell
powershell -ExecutionPolicy Bypass -File "d:\ontap.Nett\RPMS\Configure_Machine_A_SQL.ps1"
```

| Máy / clone | Path script (nếu có) |
|-------------|----------------------|
| Máy A (ontap) | `d:\ontap.Nett\RPMS\Configure_Machine_A_SQL.ps1` |
| Clone này | `E:\DoAn\RPMS\Configure_Machine_A_SQL.ps1` |

> **Firewall:** Bản script mới mở inbound **1433/1434 + ICMP** cho **Domain, Private và Public** (không chỉ Private).

> **Trạng thái:** File `Configure_Machine_A_SQL.ps1` **chưa có** trong clone `E:\DoAn\RPMS` (và không thấy bản copy tại `E:\DocDoAn`). Dùng đúng path trên Máy A (`d:\ontap.Nett\RPMS\…`). Không tự tạo script thay thế khi file thiếu.

Sau khi chạy xong trên Máy A, máy khách kiểm tra (đổi IP nếu đã dùng hotspot):

```bat
sqlcmd -S 172.20.10.2\SQLEXPRESS -U rpms_user -P <mat_khau>
```

---

## Tham chiếu nhanh trong repo

| Mục | Path |
|-----|------|
| Connection string | `RPMS.WinForms/Program.cs` → `Program.ConnectionString` |
| Script DB đầy đủ | `Database/RPMS_Full.sql` |
| 1-Click Máy A (nếu có) | `Configure_Machine_A_SQL.ps1` (Máy A: `d:\ontap.Nett\RPMS\`) |
| Encoding sqlcmd | `Database/README_ENCODING.md` |
| Schema patches | `RPMS.DAL/DatabaseSchemaUpdater.cs` |
| Seed / hash demo | `RPMS.BLL/DataSeeder.cs` |
| Onboarding local | `Docs/SystemDocumentation/10_Onboarding.md` |
