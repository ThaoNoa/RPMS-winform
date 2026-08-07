using RPMS.BLL.Helpers;
using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL
{
    /// <summary>
    /// Đồng bộ mật khẩu demo với script SQL sample.
    /// Sample SQL lưu plain text — Auth dùng BCrypt nên phải hash khi khởi động.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(RPMSContext db)
        {
            // Không tạo lại schema/roles/users nếu DB đã có từ script SQL.
            // Chỉ hash mật khẩu plain-text → BCrypt cho tài khoản sample.

            await HashIfPlaintextAsync(db, "admin", "admin123");
            await HashIfPlaintextAsync(db, "namlandlord", "123456");
            await HashIfPlaintextAsync(db, "tenant", "123456");
            await HashIfPlaintextAsync(db, "manager", "123456");

            // Sửa tên tiếng Việt nếu sample SQL bị mojibake encoding
            await FixSampleDisplayNamesAsync(db);

            // Đảm bảo catalog tiện ích phòng (DB cũ / EnsureCreated thiếu dòng)
            await EnsureAmenitiesAsync(db);

            // Đồng bộ ngày sample về thời điểm thực (tháng trước nhận phòng ngày 15 → prorate)
            await SyncSampleTimelineAsync(db);

            // Tài khoản demo cũ (LocalDB) — nếu còn trong DB thì hash luôn
            await HashIfPlaintextAsync(db, "landlord1", "123456");
            await HashIfPlaintextAsync(db, "tenant1", "123456");
            await HashIfPlaintextAsync(db, "manager1", "123456");

            // Nếu Roles trống (DB rỗng chưa chạy script) — seed tối thiểu để app không crash
            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Landlord" },
                    new Role { RoleName = "Tenant" },
                    new Role { RoleName = "Manager" });
                await db.SaveChangesAsync();
            }

            if (!await db.Users.AnyAsync())
            {
                var roles = await db.Roles.ToListAsync();
                int adminRole = roles.First(r => r.RoleName == "Admin").RoleID;
                int landlordRole = roles.First(r => r.RoleName == "Landlord").RoleID;
                int tenantRole = roles.First(r => r.RoleName == "Tenant").RoleID;
                int managerRole = roles.First(r => r.RoleName == "Manager").RoleID;

                db.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        Password = PasswordHelper.HashPassword("admin123"),
                        FullName = "Quản trị viên",
                        Email = "admin@rpms.com",
                        Phone = "0900123456",
                        Address = "Hà Nội",
                        RoleID = adminRole,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    },
                    new User
                    {
                        Username = "namlandlord",
                        Password = PasswordHelper.HashPassword("123456"),
                        FullName = "Nguyễn Văn Nam",
                        Email = "nam@landlord.com",
                        Phone = "0912345678",
                        Address = "TP.HCM",
                        RoleID = landlordRole,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    },
                    new User
                    {
                        Username = "tenant",
                        Password = PasswordHelper.HashPassword("123456"),
                        FullName = "Trần Văn An",
                        Email = "an@tenant.com",
                        Phone = "0923456789",
                        Address = "TP.HCM",
                        RoleID = tenantRole,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    },
                    new User
                    {
                        Username = "manager",
                        Password = PasswordHelper.HashPassword("123456"),
                        FullName = "Lê Thị Mai",
                        Email = "mai@manager.com",
                        Phone = "0934567890",
                        Address = "TP.HCM",
                        RoleID = managerRole,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Catalog tiện ích chuẩn. DB sample bị mojibake + SchemaUpdater từng insert thêm
        /// → trùng tên khi rename theo AmenityID (UQ AmenityName). Gộp/xóa bản thừa an toàn.
        /// </summary>
        private static async Task EnsureAmenitiesAsync(RPMSContext db)
        {
            var catalog = new (int SampleId, string Name)[]
            {
                (1, "Điều hòa"), (2, "Nóng lạnh"), (3, "Wifi"), (4, "Ban công"),
                (5, "Bếp"), (6, "Gara xe"), (7, "Máy giặt"), (8, "Tủ lạnh"),
                (9, "Tủ quần áo"), (10, "Bồn rửa bát"), (11, "Sofa"), (12, "Bàn ghế")
            };

            var all = await db.Amenities.ToListAsync();
            var usedIds = (await db.RoomAmenities.Select(ra => ra.AmenityID).Distinct().ToListAsync()).ToHashSet();
            var catalogNames = new HashSet<string>(catalog.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);

            foreach (var (sampleId, name) in catalog)
            {
                var holders = all
                    .Where(a => string.Equals(a.AmenityName, name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var sample = all.FirstOrDefault(a => a.AmenityID == sampleId);

                Amenity keeper;
                if (sample != null)
                    keeper = sample;
                else if (holders.Count > 0)
                    keeper = holders.FirstOrDefault(h => usedIds.Contains(h.AmenityID))
                             ?? holders.OrderBy(h => h.AmenityID).First();
                else
                {
                    keeper = new Amenity { AmenityName = name };
                    db.Amenities.Add(keeper);
                    all.Add(keeper);
                    continue;
                }

                foreach (var extra in holders.Where(h => h.AmenityID != keeper.AmenityID).ToList())
                    await RemoveOrRemapAmenityAsync(db, all, usedIds, extra, keeper);

                // Free unique name if another row still blocks rename (e.g. different casing already handled)
                if (!string.Equals(keeper.AmenityName, name, StringComparison.Ordinal))
                {
                    foreach (var blocker in all
                        .Where(a => a.AmenityID != keeper.AmenityID
                                    && string.Equals(a.AmenityName, name, StringComparison.OrdinalIgnoreCase))
                        .ToList())
                        await RemoveOrRemapAmenityAsync(db, all, usedIds, blocker, keeper);

                    keeper.AmenityName = name;
                }
            }

            // Dọn amenity không thuộc catalog và không được phòng nào dùng (mojibake / bản thừa)
            foreach (var orphan in all
                .Where(a => !catalogNames.Contains(a.AmenityName) && !usedIds.Contains(a.AmenityID))
                .ToList())
            {
                db.Amenities.Remove(orphan);
                all.Remove(orphan);
            }

            await db.SaveChangesAsync();
        }

        private static async Task RemoveOrRemapAmenityAsync(
            RPMSContext db,
            List<Amenity> all,
            HashSet<int> usedIds,
            Amenity extra,
            Amenity keeper)
        {
            if (usedIds.Contains(extra.AmenityID))
            {
                var links = await db.RoomAmenities.Where(ra => ra.AmenityID == extra.AmenityID).ToListAsync();
                foreach (var link in links)
                {
                    bool alreadyLinked = await db.RoomAmenities.AnyAsync(ra =>
                        ra.RoomID == link.RoomID && ra.AmenityID == keeper.AmenityID);
                    if (alreadyLinked || keeper.AmenityID == 0)
                        db.RoomAmenities.Remove(link);
                    else
                        link.AmenityID = keeper.AmenityID;
                }
                usedIds.Remove(extra.AmenityID);
                if (keeper.AmenityID != 0)
                    usedIds.Add(keeper.AmenityID);
            }

            db.Amenities.Remove(extra);
            all.Remove(extra);
        }

        private static bool LooksLikeBcrypt(string password) =>
            password.StartsWith("$2a$", StringComparison.Ordinal) ||
            password.StartsWith("$2b$", StringComparison.Ordinal) ||
            password.StartsWith("$2y$", StringComparison.Ordinal);

        private static async Task HashIfPlaintextAsync(RPMSContext db, string username, string plainPassword)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return;
            if (LooksLikeBcrypt(user.Password)) return;

            user.Password = PasswordHelper.HashPassword(plainPassword);
            user.Status = "Active";
            user.UpdatedDate = DateTime.Now;
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Script SQL chạy bằng sqlcmd không UTF-8 thường làm hỏng tiếng Việt (mojibake).
        /// Sửa lại tên hiển thị cho tài khoản sample.
        /// </summary>
        private static async Task FixSampleDisplayNamesAsync(RPMSContext db)
        {
            async Task FixUser(string username, string fullName, string address)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return;
                if (user.FullName == fullName && user.Address == address) return;
                user.FullName = fullName;
                user.Address = address;
                user.UpdatedDate = DateTime.Now;
            }

            await FixUser("admin", "Quản trị viên", "Hà Nội");
            await FixUser("namlandlord", "Nguyễn Văn Nam", "123 Đường A, Quận B, TP.HCM");
            await FixUser("tenant", "Trần Văn An", "456 Đường C, Quận D, TP.HCM");
            await FixUser("manager", "Lê Thị Mai", "789 Đường E, Quận F, TP.HCM");
            await db.SaveChangesAsync();

            var house = await db.Houses.FirstOrDefaultAsync(h => h.HouseID == 1);
            if (house != null)
            {
                house.HouseName = "Nhà trọ Nam";
                house.Address = "123 Đường A, Quận B, TP.HCM";
                house.Description = "Nhà cho thuê nhiều phòng";
            }

            var room1 = await db.Rooms.FirstOrDefaultAsync(r => r.RoomID == 1);
            if (room1 != null)
            {
                room1.Furniture = "Giường, tủ quần áo, điều hòa";
                room1.Description = "Phòng đẹp, có cửa sổ";
            }
            var room2 = await db.Rooms.FirstOrDefaultAsync(r => r.RoomID == 2);
            if (room2 != null)
            {
                room2.Furniture = "Giường, tủ, điều hòa, ban công";
                room2.Description = "Phòng rộng, có ban công";
            }

            // Amenities: không rename theo ID tại đây — dễ đụng UQ AmenityName khi DB đã có
            // bản insert đúng Unicode (SchemaUpdater / seed trước). Xử lý trong EnsureAmenitiesAsync.

            var post = await db.Posts.FirstOrDefaultAsync(p => p.PostID == 1);
            if (post != null)
            {
                post.Title = "Cho thuê phòng 101 giá rẻ";
                post.Description = "Phòng đẹp, đầy đủ tiện nghi";
            }

            var appt = await db.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == 1);
            if (appt != null)
                appt.Note = "Khách muốn xem phòng";

            var maint = await db.MaintenanceRequests.FirstOrDefaultAsync(m => m.RequestID == 1);
            if (maint != null)
            {
                maint.Title = "Bóng đèn hỏng";
                maint.Description = "Bóng đèn phòng tắm không sáng";
            }

            var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewID == 1);
            if (review != null)
                review.Comment = "Phòng đẹp, chủ nhà thân thiện, tiện nghi đầy đủ. Rất hài lòng!";

            var noti = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == 1);
            if (noti != null)
            {
                noti.Title = "Có lịch hẹn mới";
                noti.Content = "Người thuê Trần Văn An đặt lịch xem phòng 102";
            }

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Đồng bộ sample theo thời gian thực (vd hôm nay 4/8):
        /// - Nhận phòng ngày 15 tháng T-3 (15/5) → demo prorate tháng đầu
        /// - Hóa đơn / chỉ số 3 tháng đã qua: T-3, T-2, T-1 (5, 6, 7) — không có tháng hiện tại
        /// </summary>
        private static async Task SyncSampleTimelineAsync(RPMSContext db)
        {
            var contract = await db.Contracts.FirstOrDefaultAsync(c => c.ContractID == 1);
            if (contract == null) return;

            var today = DateTime.Today;
            // 3 tháng đã kết thúc trước tháng hiện tại
            var month0 = new DateTime(today.AddMonths(-3).Year, today.AddMonths(-3).Month, 1); // 5
            var month1 = new DateTime(today.AddMonths(-2).Year, today.AddMonths(-2).Month, 1); // 6
            var month2 = new DateTime(today.AddMonths(-1).Year, today.AddMonths(-1).Month, 1); // 7
            var months = new[] { month0, month1, month2 };

            var moveIn = new DateTime(month0.Year, month0.Month, 15);
            contract.StartDate = moveIn;
            contract.EndDate = moveIn.AddYears(1).AddDays(-1);
            contract.MoveInDate = moveIn;
            contract.MoveOutDate = null;
            contract.Status = "Active";
            contract.UpdatedDate = DateTime.Now;

            var room = await db.Rooms.FirstOrDefaultAsync(r => r.RoomID == contract.RoomID);
            if (room != null)
            {
                room.Status = "Occupied";
                room.UpdatedDate = DateTime.Now;
            }

            // Chỉ số tích lũy: tháng 5 → 6 → 7
            var endsE = new decimal[] { 1100, 1250, 1400 };
            var endsW = new decimal[] { 55, 62, 70 };
            const decimal startE = 1000;
            const decimal startW = 50;

            for (int i = 0; i < months.Length; i++)
            {
                var monthStart = months[i];
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                decimal oldE = i == 0 ? startE : endsE[i - 1];
                decimal newE = endsE[i];
                decimal oldW = i == 0 ? startW : endsW[i - 1];
                decimal newW = endsW[i];

                var reading = await db.MeterReadings.FirstOrDefaultAsync(m =>
                    m.ContractID == 1 && m.ReadingMonth.Year == monthStart.Year && m.ReadingMonth.Month == monthStart.Month);
                if (reading == null)
                {
                    reading = new MeterReading
                    {
                        ContractID = 1,
                        ReadingMonth = monthStart,
                        CreatedBy = 4,
                        CreatedDate = DateTime.Now
                    };
                    db.MeterReadings.Add(reading);
                }

                reading.ReadingMonth = monthStart;
                reading.OldElectric = oldE;
                reading.NewElectric = newE;
                reading.OldWater = oldW;
                reading.NewWater = newW;
                reading.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync(); // cần ReadingID cho invoice mới

                var rentCalc = RentProrationHelper.Calculate(
                    contract.MonthlyRent,
                    monthStart,
                    contract.StartDate,
                    contract.EndDate,
                    contract.MoveInDate,
                    contract.MoveOutDate);

                decimal electricCost = (newE - oldE) * contract.ElectricPrice;
                decimal waterCost = (newW - oldW) * contract.WaterPrice;
                decimal total = rentCalc.ProratedRent + electricCost + waterCost;

                // Tháng đầu + giữa: đã thanh toán; tháng gần nhất (7): Unpaid để demo
                bool isLatest = i == months.Length - 1;
                string status = isLatest ? "Unpaid" : "Paid";
                DateTime? paidDate = isLatest ? null : monthStart.AddDays(Math.Min(20, monthEnd.Day));

                var invoice = await db.Invoices.FirstOrDefaultAsync(inv =>
                    inv.ContractID == 1 && inv.ReadingID == reading.ReadingID);
                if (invoice == null)
                {
                    // fallback: tìm theo DueDate trong tháng
                    invoice = await db.Invoices.FirstOrDefaultAsync(inv =>
                        inv.ContractID == 1 &&
                        inv.DueDate.Year == monthEnd.Year &&
                        inv.DueDate.Month == monthEnd.Month);
                }

                if (invoice == null)
                {
                    invoice = new Invoice
                    {
                        InvoiceCode = $"INV{monthStart:yyMM}01",
                        ContractID = 1,
                        ReadingID = reading.ReadingID,
                        CreatedDate = DateTime.Now
                    };
                    db.Invoices.Add(invoice);
                }

                invoice.ReadingID = reading.ReadingID;
                invoice.Rent = rentCalc.ProratedRent;
                invoice.ElectricCost = electricCost;
                invoice.WaterCost = waterCost;
                invoice.OtherFee = 0;
                invoice.Total = total;
                invoice.DueDate = monthEnd;
                invoice.PaidDate = paidDate;
                invoice.Status = status;
                invoice.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync();

                var payment = await db.Payments.FirstOrDefaultAsync(p => p.InvoiceID == invoice.InvoiceID);
                if (!isLatest)
                {
                    if (payment == null)
                    {
                        payment = new Payment
                        {
                            InvoiceID = invoice.InvoiceID,
                            Method = "Banking",
                            CreatedDate = DateTime.Now
                        };
                        db.Payments.Add(payment);
                    }
                    payment.PaymentDate = paidDate ?? monthEnd;
                    payment.Amount = total;
                    payment.Status = "Completed";
                    payment.UpdatedDate = DateTime.Now;
                }
                else if (payment != null)
                {
                    // Tháng mới nhất chưa thanh toán — xóa payment sample nếu còn
                    db.Payments.Remove(payment);
                }
            }

            // Xóa chỉ số / hóa đơn sample của tháng hiện tại (nếu từng tạo nhầm)
            var currentMonth = new DateTime(today.Year, today.Month, 1);
            var futureReadings = await db.MeterReadings
                .Where(m => m.ContractID == 1 && m.ReadingMonth >= currentMonth)
                .ToListAsync();
            foreach (var fr in futureReadings)
            {
                var invs = await db.Invoices.Where(i => i.ReadingID == fr.ReadingID).ToListAsync();
                foreach (var inv in invs)
                {
                    var pays = await db.Payments.Where(p => p.InvoiceID == inv.InvoiceID).ToListAsync();
                    db.Payments.RemoveRange(pays);
                    db.Invoices.Remove(inv);
                }
                db.MeterReadings.Remove(fr);
            }

            var appt = await db.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == 1);
            if (appt != null)
            {
                appt.AppointmentDate = today.AddDays(2).Date.AddHours(9);
                appt.Status = "Pending";
                appt.UpdatedDate = DateTime.Now;
            }

            var noti = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == 1);
            if (noti != null)
            {
                noti.Content =
                    $"Người thuê Trần Văn An đặt lịch xem phòng 102 vào ngày {appt?.AppointmentDate:dd/MM/yyyy}";
                noti.CreatedDate = DateTime.Now;
                noti.UpdatedDate = DateTime.Now;
            }

            var maint = await db.MaintenanceRequests.FirstOrDefaultAsync(m => m.RequestID == 1);
            if (maint != null)
            {
                maint.CreatedDate = today.AddDays(-3);
                maint.UpdatedDate = DateTime.Now;
            }

            await db.SaveChangesAsync();
        }
    }
}
