# RPMS — Rental Property Management System

Hệ thống quản lý nhà trọ / cho thuê phòng (Desktop), hỗ trợ Admin, Chủ nhà, Người thuê và Quản lý viên.

## Công nghệ

| Thành phần | Chi tiết |
|------------|----------|
| Runtime | .NET 8 |
| UI | Windows Forms (Fluent-style) |
| ORM | Entity Framework Core 8 |
| Database | SQL Server Express (`.\SQLEXPRESS`) |
| DI | Microsoft.Extensions.DependencyInjection |
| Bảo mật mật khẩu | BCrypt |

## Cấu trúc solution

```
RPMS.sln
├── RPMS.WinForms     # Giao diện + Program.cs (DI, connection string)
├── RPMS.BLL          # Nghiệp vụ, DataSeeder, services
├── RPMS.DAL          # EF Core, repositories, Unit of Work, schema updater
├── RPMS.DTO          # Data Transfer Objects
├── RPMS.Common       # Constants, UserSession, helpers
└── Database/
    └── RPMS_Full.sql # Script tạo DB + dữ liệu mẫu
```

Kiến trúc: **WinForms → BLL → DAL** (không gọi DB trực tiếp từ UI).

## Yêu cầu môi trường

- Windows 10/11  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- Visual Studio 2022 (hoặc VS Code + C# extension)  
- **SQL Server Express** với instance mặc định: `.\SQLEXPRESS`  
- Quyền Windows Authentication (Trusted Connection)

## Cài đặt database

1. Mở SQL Server Management Studio / `sqlcmd` kết nối `.\SQLEXPRESS`.
2. Chạy script (khuyến nghị UTF-8 để tiếng Việt không lỗi):

```bash
sqlcmd -S .\SQLEXPRESS -E -f 65001 -i Database\RPMS_Full.sql
```

> Script có thể dùng đường dẫn file `.mdf` theo máy tạo script. Nếu lỗi path, sửa dòng `FILENAME` trong `Database/RPMS_Full.sql` cho phù hợp rồi chạy lại.

3. Lần đầu mở app, hệ thống sẽ:
   - Cập nhật schema bổ sung (Chat, cột Reviews, …) qua `DatabaseSchemaUpdater`
   - Hash mật khẩu demo (BCrypt) qua `DataSeeder`
   - Đồng bộ ngày mẫu / hóa đơn prorate theo thời gian thực

### Đổi connection string

Sửa trong `RPMS.WinForms/Program.cs`:

```csharp
Server=.\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

## Chạy ứng dụng

```bash
dotnet restore RPMS.sln
dotnet build RPMS.sln
dotnet run --project RPMS.WinForms
```

Hoặc mở `RPMS.sln` trong Visual Studio → đặt startup project `RPMS.WinForms` → F5.

## Tài khoản demo

| Username | Password | Vai trò |
|----------|----------|---------|
| `admin` | `admin123` | Admin |
| `namlandlord` | `123456` | Chủ nhà (Landlord) |
| `tenant` | `123456` | Người thuê (Tenant) |
| `manager` | `123456` | Quản lý viên (Manager) |

## Chức năng theo vai trò

### Admin
- Quản lý người dùng, duyệt tin đăng  
- Phân công Manager cho nhà  
- Đánh giá, nhật ký hoạt động  
- Sao lưu / khôi phục DB  

### Chủ nhà (Landlord)
- Quản lý nhà, phòng (**nhiều ảnh + video**), tiện nghi  
- Tin đăng (gallery media), lịch hẹn, hợp đồng (**In/PDF**)  
- Đánh giá / phản hồi, chat với khách thuê  
- Dashboard: doanh thu 6 tháng, **tỷ lệ lấp đầy** (biểu đồ)  
- Báo cáo / lịch  

### Người thuê (Tenant)
- Tìm phòng (**lọc nâng cao**: giá, diện tích, tiện ích, trạng thái, nổi bật…)  
- Chi tiết phòng + **gallery ảnh/video**  
- Yêu thích, đặt lịch xem  
- Hợp đồng (In/PDF), hóa đơn (prorate), thanh toán, **Xuất PDF/Excel**  
- Báo sự cố / bảo trì, đánh giá, chat  

### Quản lý (Manager)
- Ghi chỉ số điện/nước → tạo hóa đơn **tháng trước** (tháng đã kết thúc)  
- Xử lý sự cố: danh sách + **chi tiết** (ảnh, **timeline trạng thái**, In/PDF phiếu bảo trì)  

## Nghiệp vụ nổi bật

- **Tiền nhà prorate**:  
  `MonthlyRent ÷ số ngày trong tháng × số ngày thực ở`  
  (theo ngày nhận/trả phòng giao với tháng hóa đơn).
- **Hóa đơn**: chỉ tạo cho tháng đã kết thúc; mặc định form ghi chỉ số = tháng trước.
- **In / PDF**: HTML mở trình duyệt → *Microsoft Print to PDF* (hóa đơn, hợp đồng, phiếu bảo trì).
- **Excel**: xuất CSV UTF-8 (danh sách hóa đơn, báo cáo).
- **Chat**: Landlord ↔ Tenant, gửi text/ảnh.
- **Media**: đường dẫn `/uploads/...`; hỗ trợ ảnh + video; sample tự tạo ảnh minh họa nếu thiếu file.
- **UX**: loading overlay, empty state, toast thông báo.

## Tài liệu kỹ thuật

Chi tiết kiến trúc, class, luồng nghiệp vụ: [`Docs/TongQuanDuAn_RPMS.doc`](Docs/TongQuanDuAn_RPMS.doc) (mở bằng Word; phiên bản **1.1**).

## Build & ghi chú

- Đóng `RPMS.WinForms` trước khi build nếu gặp lỗi khóa DLL.  
- Tiếng Việt trong SQL: chạy `sqlcmd -f 65001` (xem thêm `Database/README_ENCODING.md`).  
- App tự sửa một số tên sample bị mojibake khi khởi động.

## Giấy phép / học thuật

Đồ án / dự án học tập — sử dụng và chỉnh sửa theo nhu cầu môn học hoặc nội bộ nhóm.
