using System;
using System.Data;
using Microsoft.Data.SqlClient;

var cs = @"Server=.\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;";
await using var conn = new SqlConnection(cs);
await conn.OpenAsync();

async Task Exec(string sql)
{
    await using var cmd = new SqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
}

await Exec("UPDATE Users SET FullName=N'Quản trị viên', [Address]=N'Hà Nội' WHERE Username=N'admin'");
await Exec("UPDATE Users SET FullName=N'Nguyễn Văn Nam', [Address]=N'123 Đường A, Quận B, TP.HCM' WHERE Username=N'namlandlord'");
await Exec("UPDATE Users SET FullName=N'Trần Văn An', [Address]=N'456 Đường C, Quận D, TP.HCM' WHERE Username=N'tenant'");
await Exec("UPDATE Users SET FullName=N'Lê Thị Mai', [Address]=N'789 Đường E, Quận F, TP.HCM' WHERE Username=N'manager'");

await Exec("UPDATE Houses SET HouseName=N'Nhà trọ Nam', [Address]=N'123 Đường A, Quận B, TP.HCM', Description=N'Nhà cho thuê nhiều phòng' WHERE HouseID=1");
await Exec("UPDATE Rooms SET Furniture=N'Giường, tủ quần áo, điều hòa', Description=N'Phòng đẹp, có cửa sổ' WHERE RoomID=1");
await Exec("UPDATE Rooms SET Furniture=N'Giường, tủ, điều hòa, ban công', Description=N'Phòng rộng, có ban công' WHERE RoomID=2");

await Exec("UPDATE Amenities SET AmenityName=N'Điều hòa' WHERE AmenityID=1");
await Exec("UPDATE Amenities SET AmenityName=N'Nóng lạnh' WHERE AmenityID=2");
await Exec("UPDATE Amenities SET AmenityName=N'Wifi' WHERE AmenityID=3");
await Exec("UPDATE Amenities SET AmenityName=N'Ban công' WHERE AmenityID=4");
await Exec("UPDATE Amenities SET AmenityName=N'Bếp' WHERE AmenityID=5");
await Exec("UPDATE Amenities SET AmenityName=N'Gara xe' WHERE AmenityID=6");

await Exec("UPDATE Posts SET Title=N'Cho thuê phòng 101 giá rẻ', Description=N'Phòng đẹp, đầy đủ tiện nghi' WHERE PostID=1");
await Exec("UPDATE Appointments SET Note=N'Khách muốn xem phòng' WHERE AppointmentID=1");
await Exec("UPDATE MaintenanceRequests SET Title=N'Bóng đèn hỏng', Description=N'Bóng đèn phòng tắm không sáng' WHERE RequestID=1");
await Exec("UPDATE Reviews SET Comment=N'Phòng đẹp, chủ nhà thân thiện, tiện nghi đầy đủ. Rất hài lòng!' WHERE ReviewID=1");
await Exec("UPDATE Notifications SET Title=N'Có lịch hẹn mới', Content=N'Người thuê Trần Văn An đặt lịch xem phòng 102 vào ngày 25/07/2026' WHERE NotificationID=1");
await Exec("UPDATE ActivityLogs SET Action=N'Đăng nhập', Details=N'Admin đăng nhập hệ thống' WHERE LogID=1");
await Exec("UPDATE ActivityLogs SET Action=N'Tạo hợp đồng', Details=N'Tạo hợp đồng HD00001 cho phòng 101' WHERE LogID=2");

await using (var cmd = new SqlCommand("SELECT Username, FullName FROM Users ORDER BY UserID", conn))
await using (var r = await cmd.ExecuteReaderAsync())
{
    while (await r.ReadAsync())
        Console.WriteLine($"{r.GetString(0)} => {r.GetString(1)}");
}

Console.WriteLine("UNICODE_FIX_OK");
